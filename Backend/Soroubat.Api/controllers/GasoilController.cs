using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Soroubat.Api.Interfaces;
using Soroubat.Api.Models;

namespace Soroubat.Api.Controllers
{
    /// <summary>
    /// Gestion des fiches gasoil journalières.
    /// Toutes les routes sont scopées au projet du chef de chantier extrait du JWT.
    /// Le flux de validation est : création (En Cours) → saisie des lignes → validation (/valider).
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class GasoilController : ControllerBase
    {
        private readonly IGasoilService _gasoilService;

        private string UserProjectNo => User.FindFirst("projectNo")?.Value ?? string.Empty;

        public GasoilController(IGasoilService gasoilService)
        {
            _gasoilService = gasoilService;
        }

        // ─── HEADERS ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Retourne toutes les fiches gasoil du chantier du chef de chantier connecté.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<GasoilHeader>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<GasoilHeader>>> GetAll()
        {
            if (string.IsNullOrEmpty(UserProjectNo))
                return Unauthorized(new { message = "Token invalide : aucun projet associé." });

            try
            {
                var headers = await _gasoilService.GetHeadersByJobAsync(UserProjectNo);
                return Ok(headers);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Retourne une fiche gasoil avec ses lignes.
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(GasoilHeader), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GasoilHeader>> GetById(Guid id)
        {
            if (string.IsNullOrEmpty(UserProjectNo))
                return Unauthorized(new { message = "Token invalide : aucun projet associé." });

            try
            {
                var result = await _gasoilService.GetHeaderByIdAsync(id, UserProjectNo);

                if (result == null)
                    return NotFound(new { message = "Fiche gasoil introuvable." });

                return Ok(result);
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
        /// Crée une fiche gasoil. jobNo est automatiquement forcé depuis le JWT.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(GasoilHeader), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<GasoilHeader>> CreateHeader([FromBody] GasoilHeader header)
        {
            if (string.IsNullOrEmpty(UserProjectNo))
                return Unauthorized(new { message = "Token invalide : aucun projet associé." });

            try
            {
                var created = await _gasoilService.CreateHeaderAsync(header, UserProjectNo);

                if (created == null)
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        new { message = "Business Central n'a pas retourné la fiche créée." });

                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Met à jour les champs modifiables d'une fiche gasoil.
        /// </summary>
        [HttpPatch("{id:guid}")]
        [ProducesResponseType(typeof(GasoilHeader), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GasoilHeader>> UpdateHeader(Guid id, [FromBody] GasoilHeader header)
        {
            if (string.IsNullOrEmpty(UserProjectNo))
                return Unauthorized(new { message = "Token invalide : aucun projet associé." });

            try
            {
                var updated = await _gasoilService.UpdateHeaderAsync(id, header, UserProjectNo);

                if (updated == null)
                    return NotFound(new { message = "Fiche gasoil introuvable." });

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
        /// Supprime une fiche gasoil.
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
                var success = await _gasoilService.DeleteHeaderAsync(id, UserProjectNo);

                if (!success)
                    return NotFound(new { message = "Fiche gasoil introuvable." });

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
        /// Valide une fiche gasoil (En Cours → Validé).
        /// </summary>
        [HttpPost("{id:guid}/valider")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ValiderFiche(Guid id)
        {
            if (string.IsNullOrEmpty(UserProjectNo))
                return Unauthorized(new { message = "Token invalide : aucun projet associé." });

            try
            {
                var success = await _gasoilService.ValiderFicheAsync(id, UserProjectNo);

                if (!success)
                    return NotFound(new { message = "Fiche gasoil introuvable." });

                return Ok(new { message = "Fiche gasoil validée avec succès." });
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
        /// Crée une ligne de distribution gasoil.
        /// </summary>
        [HttpPost("lines")]
        [ProducesResponseType(typeof(GasoilLine), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<GasoilLine>> CreateLine([FromBody] GasoilLine line)
        {
            if (string.IsNullOrEmpty(UserProjectNo))
                return Unauthorized(new { message = "Token invalide : aucun projet associé." });

            try
            {
                var created = await _gasoilService.CreateLineAsync(line, UserProjectNo);

                if (created == null)
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        new { message = "Business Central n'a pas retourné la ligne créée." });

                return StatusCode(StatusCodes.Status201Created, created);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Met à jour une ligne de distribution gasoil.
        /// </summary>
        [HttpPatch("lines/{id:guid}")]
        [ProducesResponseType(typeof(GasoilLine), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GasoilLine>> UpdateLine(Guid id, [FromBody] GasoilLine line)
        {
            if (string.IsNullOrEmpty(UserProjectNo))
                return Unauthorized(new { message = "Token invalide : aucun projet associé." });

            try
            {
                var updated = await _gasoilService.UpdateLineAsync(id, line, UserProjectNo);

                if (updated == null)
                    return NotFound(new { message = "Ligne gasoil introuvable." });

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
        /// Supprime une ligne de distribution gasoil.
        /// </summary>
        [HttpDelete("lines/{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteLine(Guid id)
        {
            if (string.IsNullOrEmpty(UserProjectNo))
                return Unauthorized(new { message = "Token invalide : aucun projet associé." });

            try
            {
                var success = await _gasoilService.DeleteLineAsync(id, UserProjectNo);

                if (!success)
                    return NotFound(new { message = "Ligne gasoil introuvable." });

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
    }
}