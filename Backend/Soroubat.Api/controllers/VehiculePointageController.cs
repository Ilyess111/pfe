using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Soroubat.Api.Interfaces;
using Soroubat.Api.Models;

namespace Soroubat.Api.Controllers
{
    /// <summary>
    /// Gestion des pointages véhicules journaliers.
    /// Toutes les routes sont scopées au projet du chef de chantier extrait du JWT.
    /// Les lignes sont créées automatiquement par BC à la création de l'en-tête.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class VehiculePointageController : ControllerBase
    {
        private readonly IVehiculeService _vehiculeService;

        private string UserProjectNo => User.FindFirst("projectNo")?.Value ?? string.Empty;

        public VehiculePointageController(IVehiculeService vehiculeService)
        {
            _vehiculeService = vehiculeService;
        }

        // ─── HEADERS ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Retourne tous les pointages du chantier du chef de chantier connecté.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<VehiculePointageHeader>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<VehiculePointageHeader>>> GetAll()
        {
            if (string.IsNullOrEmpty(UserProjectNo))
                return Unauthorized(new { message = "Token invalide : aucun projet associé." });

            try
            {
                var records = await _vehiculeService.GetHeadersByJobAsync(UserProjectNo);
                return Ok(records);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Retourne un pointage avec ses lignes.
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(VehiculePointageHeader), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<VehiculePointageHeader>> GetById(Guid id)
        {
            if (string.IsNullOrEmpty(UserProjectNo))
                return Unauthorized(new { message = "Token invalide : aucun projet associé." });

            try
            {
                var header = await _vehiculeService.GetHeaderByIdAsync(id, UserProjectNo);

                if (header == null)
                    return NotFound(new { message = "Pointage introuvable." });

                return Ok(header);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Crée un en-tête de pointage. Les lignes sont créées automatiquement par BC.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(VehiculePointageHeader), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<VehiculePointageHeader>> CreateHeader([FromBody] VehiculePointageHeader header)
        {
            if (string.IsNullOrEmpty(UserProjectNo))
                return Unauthorized(new { message = "Token invalide : aucun projet associé." });

            try
            {
                var created = await _vehiculeService.CreateHeaderAsync(header, UserProjectNo);

                if (created == null)
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        new { message = "Business Central n'a pas retourné le pointage créé." });

                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Met à jour les champs modifiables d'un en-tête de pointage.
        /// </summary>
        [HttpPatch("{id:guid}")]
        [ProducesResponseType(typeof(VehiculePointageHeader), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<VehiculePointageHeader>> UpdateHeader(Guid id, [FromBody] VehiculePointageHeader header)
        {
            if (string.IsNullOrEmpty(UserProjectNo))
                return Unauthorized(new { message = "Token invalide : aucun projet associé." });

            try
            {
                var updated = await _vehiculeService.UpdateHeaderAsync(id, header, UserProjectNo);

                if (updated == null)
                    return NotFound(new { message = "Pointage introuvable." });

                return Ok(updated);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Supprime un pointage.
        /// </summary>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteHeader(Guid id)
        {
            if (string.IsNullOrEmpty(UserProjectNo))
                return Unauthorized(new { message = "Token invalide : aucun projet associé." });

            try
            {
                var success = await _vehiculeService.DeleteHeaderAsync(id, UserProjectNo);

                if (!success)
                    return NotFound(new { message = "Pointage introuvable." });

                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Valide un pointage (Ouvert → Validé).
        /// </summary>
        [HttpPost("{id:guid}/valider")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ValiderPointage(Guid id)
        {
            if (string.IsNullOrEmpty(UserProjectNo))
                return Unauthorized(new { message = "Token invalide : aucun projet associé." });

            try
            {
                var success = await _vehiculeService.ValiderPointageAsync(id, UserProjectNo);

                if (!success)
                    return NotFound(new { message = "Pointage introuvable." });

                return Ok(new { message = "Pointage validé avec succès." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        // ─── LIGNES ───────────────────────────────────────────────────────────────

        /// <summary>
        /// Met à jour une ligne de pointage véhicule.
        /// </summary>
        [HttpPatch("lines/{id:guid}")]
        [ProducesResponseType(typeof(VehiculePointageLine), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<VehiculePointageLine>> UpdateLine(Guid id, [FromBody] VehiculePointageLine line)
        {
            if (string.IsNullOrEmpty(UserProjectNo))
                return Unauthorized(new { message = "Token invalide : aucun projet associé." });

            try
            {
                var updated = await _vehiculeService.UpdateLineAsync(id, line, UserProjectNo);

                if (updated == null)
                    return NotFound(new { message = "Ligne de pointage introuvable." });

                return Ok(updated);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }
    }
}