using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Soroubat.Api.Interfaces;
using Soroubat.Api.Models;
using Microsoft.AspNetCore.Authorization;

namespace Soroubat.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TransferController : ControllerBase
    {
        private readonly ITransferService _service;

        // Extraction centralisée et sécurisée du projet depuis le Token JWT
        private string UserProjectNo => User.FindFirst("projectNo")?.Value ?? "";

        public TransferController(ITransferService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TransferHeaderDto>>> GetAll()
        {
            try 
            {
                var projectNo = UserProjectNo;
                if (string.IsNullOrEmpty(projectNo))
                    return Unauthorized(new { message = "Aucun projet associé." });

                var transfers = await _service.GetAllTransfersAsync(projectNo);
                return Ok(transfers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TransferHeaderDto>> GetById(Guid id)
        {
            try 
            {
                var projectNo = UserProjectNo; 
                if (string.IsNullOrEmpty(projectNo)) 
                    return Unauthorized(new { message = "Aucun projet associé." });

                var transfer = await _service.GetTransferByIdAsync(id, projectNo);
                
                if (transfer == null) 
                    return NotFound(new { message = "Ordre de transfert introuvable ou accès non autorisé." });
                    
                return Ok(transfer);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPatch("lines/{id}")]
        public async Task<IActionResult> PatchLine(Guid id, [FromBody] JsonElement body)
        {
            try 
            {
                var projectNo = UserProjectNo;
                if (string.IsNullOrEmpty(projectNo)) 
                    return Unauthorized(new { message = "Aucun projet associé." });

                var success = await _service.UpdateLineAsync(id, body, projectNo);
                
                if (success) 
                    return Ok(new { message = "Réception mise à jour avec succès." });
                
                return BadRequest(new { message = "Échec de la mise à jour de la ligne (Vérifiez vos droits ou l'ID)." });
            } 
            catch (Exception ex) 
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}