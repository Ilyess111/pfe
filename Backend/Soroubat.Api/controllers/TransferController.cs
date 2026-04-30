using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Soroubat.Api.Interfaces;
using Soroubat.Api.Models;

namespace Soroubat.Api.Controllers
{
    /// <summary>
    /// Gestion des ordres de transfert.
    /// Les transferts sont créés dans BC par le magasinier — le chef de chantier
    /// consulte uniquement les transferts à destination de son chantier
    /// et saisit les quantités réceptionnées sur les lignes.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TransferController : ControllerBase
    {
        private readonly ITransferService _transferService;

        private string UserProjectNo => User.FindFirst("projectNo")?.Value ?? string.Empty;

        public TransferController(ITransferService transferService)
        {
            _transferService = transferService;
        }

        // ─── HEADERS ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Retourne tous les ordres de transfert à destination du chantier du chef connecté.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<TransferHeaderDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<TransferHeaderDto>>> GetAll()
        {
            if (string.IsNullOrEmpty(UserProjectNo))
                return Unauthorized(new { message = "Token invalide : aucun projet associé." });

            try
            {
                var transfers = await _transferService.GetAllTransfersAsync(UserProjectNo);
                return Ok(transfers);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Retourne un ordre de transfert avec ses lignes.
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(TransferHeaderDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TransferHeaderDto>> GetById(Guid id)
        {
            if (string.IsNullOrEmpty(UserProjectNo))
                return Unauthorized(new { message = "Token invalide : aucun projet associé." });

            try
            {
                var transfer = await _transferService.GetTransferByIdAsync(id, UserProjectNo);

                if (transfer == null)
                    return NotFound(new { message = "Ordre de transfert introuvable." });

                return Ok(transfer);
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
        /// Met à jour la quantité à réceptionner (qtyToReceive) d'une ligne de transfert.
        /// </summary>
        [HttpPatch("lines/{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateLine(Guid id, [FromBody] TransferLineUpdateDto updateDto)
        {
            if (string.IsNullOrEmpty(UserProjectNo))
                return Unauthorized(new { message = "Token invalide : aucun projet associé." });

            try
            {
                var success = await _transferService.UpdateLineAsync(id, updateDto, UserProjectNo);

                if (!success)
                    return NotFound(new { message = "Ligne de transfert introuvable." });

                return Ok(new { message = "Réception mise à jour avec succès." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }
    }
}