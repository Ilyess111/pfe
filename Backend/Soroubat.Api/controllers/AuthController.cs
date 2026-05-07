using Microsoft.AspNetCore.Mvc;
using Soroubat.Api.Models;
using Soroubat.Api.Interfaces;

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

        /// <summary>
        /// Authentifie un chef de chantier et retourne un JWT signé.
        /// </summary>
        /// <param name="loginDto">Email et mot de passe.</param>
        /// <returns>Token JWT à utiliser dans le header Authorization: Bearer.</returns>
        [HttpPost("login")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            // ModelState vérifie automatiquement les [Required] et [EmailAddress] du DTO.
            // Si le JSON est malformé ou qu'un champ obligatoire manque, on retourne 400.
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var token = await _authService.AuthenticateAsync(loginDto.Email, loginDto.Password);

            if (token == null)
                return Unauthorized(new { message = "Email ou mot de passe incorrect." });

            return Ok(new { token });
        }
    }
}