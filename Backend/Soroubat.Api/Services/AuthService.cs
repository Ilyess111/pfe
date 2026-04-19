using Microsoft.IdentityModel.Tokens;
using Soroubat.Api.Interfaces;
using Soroubat.Api.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net; // Nécessaire pour BCrypt.Verify

namespace Soroubat.Api.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _config;
        private readonly AuthDbContext _context;
        private readonly IChefChantierService _chefChantierService;

        public AuthService(IConfiguration config, AuthDbContext context, IChefChantierService chefChantierService)
        {
            _config = config;
            _context = context;
            _chefChantierService = chefChantierService;
        }

        public async Task<string> AuthenticateAsync(string email, string password)
        {
            // 1. Vérification dans la base locale SQLite
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            // 2. Vérification sécurisée du mot de passe avec BCrypt
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash)) 
            {
                return null;
            }

            // 3. Récupération du numéro de projet depuis Business Central via votre service
            // On utilise la méthode exacte de votre fichier IChefChantierService.cs
            string projectNo = await _chefChantierService.GetJobNoByEmailAsync(email);

            // 4. Génération du token avec le matricule du projet (ou "N/A" s'il n'est pas assigné)
            return GenerateJwtToken(user.Email, projectNo ?? "N/A");
        }

        public string GenerateJwtToken(string email, string projectNo)
        {
            var claims = new[] {
                new Claim(ClaimTypes.Email, email),
                new Claim(JwtRegisteredClaimNames.Sub, email),
                new Claim("projectNo", projectNo), // Le matricule du projet est injecté ici
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(8),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}