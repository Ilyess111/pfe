using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Soroubat.Api.Data;
using Soroubat.Api.Interfaces;

namespace Soroubat.Api.Services
{
    /// <summary>
    /// Service d'authentification.
    /// Vérifie les identifiants dans la base SQLite locale, résout le projet BC,
    /// puis génère un JWT signé contenant les claims nécessaires à l'autorisation.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _config;
        private readonly AuthDbContext _context;
        private readonly IChefChantierService _chefChantierService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IConfiguration config,
            AuthDbContext context,
            IChefChantierService chefChantierService,
            ILogger<AuthService> logger) // authService ici est le type générique du logger, qui permet d'avoir des logs catégorisés par classe (ici AuthService)
        {
            _config = config;
            _context = context;
            _chefChantierService = chefChantierService;
            _logger = logger;
        }

        public async Task<string?> AuthenticateAsync(string email, string password) // on utilise ? aprés string pour indiquer que la méthode peut retourner null en cas d'échec d'authentification
        {
            // 1. Recherche de l'utilisateur dans la base SQLite locale
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email); // retourne le premier trouvé ou null si aucun 

            // 2. Vérification du mot de passe avec BCrypt 
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                // Message volontairement générique — ne pas indiquer si c'est l'email ou le MDP qui est faux
                _logger.LogWarning("[Auth] Échec de connexion pour '{Email}'.", email);
                return null;
            }

            // 3. Résolution du numéro de projet depuis Business Central
            var projectNo = await _chefChantierService.GetJobNoByEmailAsync(email);

            // 4. Blocage si aucun projet actif n'est associé au compte
            // Un chef de chantier sans projet ne peut pas utiliser l'application
            if (string.IsNullOrEmpty(projectNo))
            {
                _logger.LogWarning("[Auth] Connexion refusée pour '{Email}' : aucun projet BC actif associé.", email);
                return null;
            }

            _logger.LogInformation("[Auth] Connexion réussie pour '{Email}' — projet '{ProjectNo}'.", email, projectNo);

            // 5. Génération du JWT avec les claims nécessaires
            return GenerateJwtToken(user.Email, projectNo);
        }

        public string GenerateJwtToken(string email, string projectNo)
        {
            var jwtKey = _config["Jwt:Key"]
                ?? throw new InvalidOperationException("La clé JWT 'Jwt:Key' est manquante dans la configuration.");

            var claims = new[]
            {
                new Claim(ClaimTypes.Email, email), // claim standard pour l'email — pas obligatoire mais souvent utilisé pour l'identification de l'utilisateur
                new Claim(JwtRegisteredClaimNames.Sub, email), // sub (subject) est un claim standard recommandé pour identifier l'utilisateur principal du token — ici on utilise l'email qui est unique
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                // Claim métier — scopé au projet du chef de chantier, lu par tous les contrôleurs
                new Claim("projectNo", projectNo)
            };

            var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256); // hmacsha256 est un algorithme de signature symétrique courant pour les JWT — il utilise la même clé pour signer et vérifier le token

            var token = new JwtSecurityToken(
                issuer:            _config["Jwt:Issuer"],
                audience:          _config["Jwt:Audience"],
                claims:            claims,
                // UTC — évite les problèmes de fuseau horaire entre le serveur et les clients
                notBefore:         DateTime.UtcNow,
                expires:           DateTime.UtcNow.AddHours(8),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}