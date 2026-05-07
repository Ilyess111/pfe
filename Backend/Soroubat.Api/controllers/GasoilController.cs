using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Soroubat.Api.Interfaces;
using Soroubat.Api.Models;

namespace Soroubat.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class GasoilController : ControllerBase
    {
        private readonly IGasoilService _gasoilService;

        // Récupération sécurisée du code projet depuis le token JWT
        private string UserProjectNo
        {
            get
            {
                return User.FindFirst("projectNo")?.Value ?? "";
            }
        }

        public GasoilController(IGasoilService gasoilService)
        {
            _gasoilService = gasoilService;
        }

        // ─── GESTION DES ENTETES (HEADERS) ───────────────────────────────

        [HttpGet]
        public async Task<IActionResult> GetHeaders()
        {
            var headers = await _gasoilService.GetHeadersByJobAsync(UserProjectNo);
            return Ok(headers);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var result = await _gasoilService.GetHeaderByIdAsync(id, UserProjectNo);
                if (result == null) 
                {
                    return NotFound(new { message = "Fiche gasoil introuvable." });
                }
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateHeader([FromBody] GasoilHeader header)
        {
            try
            {
                var result = await _gasoilService.CreateHeaderAsync(header, UserProjectNo);
                if (result == null) 
                {
                    return BadRequest(new { message = "Erreur lors de la création de l'entête." });
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateHeader(Guid id, [FromBody] GasoilHeader header)
        {
            try
            {
                var result = await _gasoilService.UpdateHeaderAsync(id, header, UserProjectNo);
                if (result == null) 
                {
                    return BadRequest(new { message = "Erreur lors de la mise à jour." });
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHeader(Guid id)
        {
            try
            {
                var success = await _gasoilService.DeleteHeaderAsync(id, UserProjectNo);
                if (!success) 
                {
                    return BadRequest(new { message = "Erreur lors de la suppression." });
                }
                return Ok(new { message = "Entête supprimée avec succès." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{id}/valider")]
        public async Task<IActionResult> Validate(Guid id)
        {
            try
            {
                var success = await _gasoilService.ValiderFicheAsync(id, UserProjectNo);
                if (!success) 
                {
                    return BadRequest(new { message = "Erreur lors de la validation." });
                }
                return Ok(new { message = "Fiche validée avec succès." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ─── GESTION DES LIGNES (LINES) ──────────────────────────────────

        [HttpPost("line")]
        public async Task<IActionResult> CreateLine([FromBody] GasoilLine line)
        {
            try
            {
                var result = await _gasoilService.CreateLineAsync(line, UserProjectNo);
                if (result == null) 
                {
                    return BadRequest(new { message = "Erreur lors de la création de la ligne." });
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPatch("line/{id}")]
        public async Task<IActionResult> UpdateLine(Guid id, [FromBody] GasoilLine line)
        {
            try
            {
                var result = await _gasoilService.UpdateLineAsync(id, line, UserProjectNo);
                if (result == null) 
                {
                    return BadRequest(new { message = "Erreur lors de la mise à jour de la ligne." });
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("line/{id}")]
        public async Task<IActionResult> DeleteLine(Guid id)
        {
            try
            {
                var success = await _gasoilService.DeleteLineAsync(id, UserProjectNo);
                if (!success) 
                {
                    return BadRequest(new { message = "Erreur lors de la suppression de la ligne." });
                }
                return Ok(new { message = "Ligne supprimée avec succès." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}