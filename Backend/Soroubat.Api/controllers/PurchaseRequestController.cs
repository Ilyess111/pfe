using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Soroubat.Api.Interfaces;
using Soroubat.Api.Models;

namespace Soroubat.Api.Controllers
{
    /// <summary>
    /// Gestion des demandes d'achat chantier.
    /// La création suit un flux en deux étapes : POST /header puis POST /lines.
    /// Toutes les routes sont scopées au projet du chef de chantier extrait du JWT.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PurchaseRequestController : ControllerBase
    {
        private readonly IPurchaseRequestService _purchaseRequestService;

        private string UserProjectNo => User.FindFirst("projectNo")?.Value ?? string.Empty;

        public PurchaseRequestController(IPurchaseRequestService purchaseRequestService)
        {
            _purchaseRequestService = purchaseRequestService;
        }

        // ─── EN-TÊTES ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Retourne toutes les demandes d'achat du projet du chef de chantier connecté.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<PurchaseRequestDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<PurchaseRequestDto>>> GetAll()
        {
            if (string.IsNullOrEmpty(UserProjectNo))
                return BadRequest(new { message = "Aucun projet assigné dans votre profil." });

            try
            {
                var requests = await _purchaseRequestService.GetAllRequestsAsync(UserProjectNo);
                return Ok(requests);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Retourne une demande d'achat avec ses lignes.
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(PurchaseRequestDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PurchaseRequestDto>> GetById(Guid id)
        {
            if (string.IsNullOrEmpty(UserProjectNo))
                return Unauthorized(new { message = "Token invalide : aucun projet associé." });

            try
            {
                var request = await _purchaseRequestService.GetRequestByIdAsync(id, UserProjectNo);

                if (request == null)
                    return NotFound(new { message = "Demande d'achat introuvable." });

                return Ok(request);
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
        /// Crée l'en-tête d'une nouvelle demande d'achat.
        /// Le jobNo est automatiquement forcé depuis le JWT.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(PurchaseRequestDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PurchaseRequestDto>> CreateHeader([FromBody] PurchaseRequestDto headerDto)
        {
            if (string.IsNullOrEmpty(UserProjectNo))
                return Unauthorized(new { message = "Token invalide : aucun projet associé." });

            try
            {
                var created = await _purchaseRequestService.CreateHeaderAsync(headerDto, UserProjectNo);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Met à jour les champs modifiables d'un en-tête de demande d'achat.
        /// </summary>
        [HttpPatch("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateHeader(Guid id, [FromBody] PurchaseRequestDto headerDto)
        {
            if (string.IsNullOrEmpty(UserProjectNo))
                return Unauthorized(new { message = "Token invalide : aucun projet associé." });

            try
            {
                var success = await _purchaseRequestService.UpdateHeaderAsync(id, headerDto, UserProjectNo);

                if (!success)
                    return NotFound(new { message = "Demande d'achat introuvable." });

                return Ok(new { message = "Demande d'achat mise à jour avec succès." });
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
        /// Soumet une demande d'achat pour approbation (Open → To Approve).
        /// </summary>
        [HttpPost("{id:guid}/submit")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SubmitForApproval(Guid id)
        {
            if (string.IsNullOrEmpty(UserProjectNo))
                return Unauthorized(new { message = "Token invalide : aucun projet associé." });

            try
            {
                var success = await _purchaseRequestService.SubmitForApprovalAsync(id, UserProjectNo);

                if (!success)
                    return NotFound(new { message = "Demande d'achat introuvable." });

                return Ok(new { message = "Demande soumise pour approbation avec succès." });
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

        /// <summary>
        /// Supprime une demande d'achat et ses lignes.
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
                var success = await _purchaseRequestService.DeleteRequestAsync(id, UserProjectNo);

                if (!success)
                    return NotFound(new { message = "Demande d'achat introuvable." });

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

        // ─── LIGNES ───────────────────────────────────────────────────────────────

        /// <summary>
        /// Crée toutes les lignes d'un en-tête en une seule opération.
        /// </summary>
        [HttpPost("lines")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateLines([FromBody] List<PurchaseRequestLineDto> lines)
        {
            if (string.IsNullOrEmpty(UserProjectNo))
                return Unauthorized(new { message = "Token invalide : aucun projet associé." });

            if (lines == null || !lines.Any())
                return BadRequest(new { message = "La liste des lignes ne peut pas être vide." });

            try
            {
                var success = await _purchaseRequestService.CreateLinesAsync(lines, UserProjectNo);

                if (!success)
                    return BadRequest(new { message = "Échec de la création des lignes." });

                return StatusCode(StatusCodes.Status201Created, new { message = "Lignes créées avec succès." });
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
        /// Met à jour une ligne de demande d'achat.
        /// </summary>
        [HttpPatch("lines/{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateLine(Guid id, [FromBody] PurchaseRequestLineDto lineDto)
        {
            if (string.IsNullOrEmpty(UserProjectNo))
                return Unauthorized(new { message = "Token invalide : aucun projet associé." });

            try
            {
                var success = await _purchaseRequestService.UpdateLineAsync(id, lineDto, UserProjectNo);

                if (!success)
                    return NotFound(new { message = "Ligne introuvable." });

                return Ok(new { message = "Ligne mise à jour avec succès." });
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
        /// Supprime une ligne de demande d'achat.
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
                var success = await _purchaseRequestService.DeleteLineAsync(id, UserProjectNo);

                if (!success)
                    return NotFound(new { message = "Ligne introuvable." });

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