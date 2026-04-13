using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Soroubat.Api.Interfaces;
using Soroubat.Api.Models;

namespace Soroubat.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransferController : ControllerBase
    {
        private readonly ITransferService _service;

        public TransferController(ITransferService service)
        {
            _service = service;
        }

        // --- MÉTHODES EN-TÊTE (HEADER) ---

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TransferHeaderDto>>> GetAll()
        {
            try 
            {
                var transfers = await _service.GetAllTransfersAsync();
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
                var transfer = await _service.GetTransferByIdAsync(id);
                if (transfer == null) return NotFound();
                return Ok(transfer);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }




        // --- ACTIONS SUR LES LIGNES ---


        [HttpPatch("lines/{id}")]
        public async Task<IActionResult> PatchLine(Guid id, [FromBody] JsonElement body)
        {
            try 
            {
                var success = await _service.UpdateLineAsync(id, body);
                if (success) return NoContent();
                return BadRequest("Échec de la mise à jour de la ligne");
            } 
            catch (Exception ex) 
            {
                return BadRequest(new { message = ex.Message });
            }
        }


    }
}