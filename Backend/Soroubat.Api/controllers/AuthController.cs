using Microsoft.AspNetCore.Mvc;
using Soroubat.Api.Models;
using Soroubat.Api.Interfaces;
using System.Threading.Tasks;

namespace Soroubat.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto login)
        {
            // Appel au service pour vérifier les identifiants dans la base SQL
            var token = await _authService.AuthenticateAsync(login.Email, login.Password);

            if (token == null)
            {
                // Retourne 401 si l'email ou le mot de passe est faux
                return Unauthorized(new { message = "Email ou mot de passe incorrect" });
            }

            // Retourne le jeton à Angular pour les futures requêtes
            return Ok(new { token });
        }
    }
}