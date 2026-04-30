using Microsoft.AspNetCore.Mvc; // contient les les classes de base ( controllerBase ) et les outils comme apiController, route, httpget, etc.
//ainsi que les types d'action result (ok, badrequest, etc.)
using Soroubat.Api.Interfaces;
using Soroubat.Api.Models;

namespace Soroubat.Api.Controllers
{
    /// <summary>
    /// Authentification des chefs de chantier.
    /// Retourne un JWT signé à inclure dans le header Authorization des requêtes suivantes.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")] // définit le path de base : /api/auth (le [controller] est remplacé par le nom du controller sans "Controller")
    public class AuthController : ControllerBase // controllerBase fournit les outils essentiels pour gérer les échanges HTTP pour une api REST (pas de vues, pas de Razor, etc.)
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Authentifie un chef de chantier et retourne un JWT valable 8 heures.
        /// </summary>
        /// <remarks>
        /// Le token retourné contient les claims : email, sub, jti, projectNo.
        /// Il doit être transmis dans le header : Authorization: Bearer {token}
        /// </remarks>
        [HttpPost("login")] // définit le path complet : POST /api/auth/login (route + "login")
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto) // le LoginDto contient les champs Email et Password.
        // from body indique que les données doivent être extraites du corps de la requête HTTP (en JSON) 
        {
            // La validation des DataAnnotations ([Required], [EmailAddress]) est automatique
            // grâce à [ApiController] — ModelState.IsValid est vérifié avant d'entrer ici

            try
            {
                var token = await _authService.AuthenticateAsync(loginDto.Email, loginDto.Password);

                if (token == null)
                    return Unauthorized(new { message = "Email ou mot de passe incorrect, ou compte sans projet actif associé." });

                return Ok(new LoginResponseDto { Token = token });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }
    }


}