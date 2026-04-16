using Microsoft.IdentityModel.Tokens;
using Soroubat.Api.Interfaces;
using Soroubat.Api.Data; // Nécessaire pour accéder à votre contexte de base de données
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Soroubat.Api.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _config;
        private readonly AuthDbContext _context; // Injection de la base de données locale

        public AuthService(IConfiguration config, AuthDbContext context)
        {
            _config = config;
            _context = context;
        }

        public async Task<string> AuthenticateAsync(string email, string password)
        {
            // 1. Chercher l'utilisateur dans la base SQL locale par son email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            // 2. Vérifier si l'utilisateur existe et si le mot de passe est correct
            // Note : Dans un projet réel, utilisez BCrypt pour vérifier le mot de passe haché
            if (user == null || user.PasswordHash != password) 
            {
                return null;
            }

            // 3. Si l'authentification réussit, générer le jeton
            return GenerateJwtToken(user.Email);
        }

        public string GenerateJwtToken(string email)
        {
            var claims = new[] {
                new Claim(ClaimTypes.Email, email), // L'email sert de lien avec BC
                new Claim(JwtRegisteredClaimNames.Sub, email),
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