using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Soroubat.Api.Interfaces;
using Soroubat.Api.Models;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Soroubat.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class VehiculePointageController : ControllerBase
    {
        private readonly IVehiculeService _vehiculeService;
        private readonly IChefChantierService _chefService; // Service qui fait le lien Email -> JobNo

        public VehiculePointageController(IVehiculeService vehiculeService, IChefChantierService chefService)
        {
            _vehiculeService = vehiculeService;
            _chefService = chefService;
        }

        // --- Opérations sur les HEADERS ---

        [HttpGet("my-pointages")]
        public async Task<IActionResult> GetMyPointages()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var jobNo = await _chefService.GetJobNoByEmailAsync(email);
            return Ok(await _vehiculeService.GetHeadersByJobAsync(jobNo));
        }

        [HttpGet("header/{id}")]
        public async Task<IActionResult> GetHeader(Guid id)
        {
            var header = await _vehiculeService.GetHeaderByIdAsync(id);
            return header != null ? Ok(header) : NotFound();
        }

        [HttpPost("header")]
        public async Task<IActionResult> CreateHeader([FromBody] VehiculePointageHeader header)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            header.JobNo = await _chefService.GetJobNoByEmailAsync(email);
            var result = await _vehiculeService.CreateHeaderAsync(header);
            return Ok(result);
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateHeader(Guid id, [FromBody] VehiculePointageHeader header)
        {
            try 
            {
                // On récupère l'objet mis à jour au lieu d'un bool
                var updatedHeader = await _vehiculeService.UpdateHeaderAsync(id, header);

                if (updatedHeader != null)
                {
                    return Ok(updatedHeader); // On renvoie l'objet à Angular
                }
                
                return NotFound(new { message = "L'en-tête n'a pas pu être mis à jour." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("header/{id}")]
        public async Task<IActionResult> DeleteHeader(Guid id)
        {
            var success = await _vehiculeService.DeleteHeaderAsync(id);
            return success ? Ok() : BadRequest("Erreur lors de la suppression du header");
        }

        // --- Opérations sur les LINES ---


        [HttpPost("line")]
        public async Task<IActionResult> AddLine([FromBody] VehiculePointageLine line)
        {
            var result = await _vehiculeService.AddLineAsync(line);
            return Ok(result);
        }

        [HttpPatch("line/{id}")]
        public async Task<IActionResult> UpdateLine(Guid id, [FromBody] VehiculePointageLine line)
        {
            try 
            {
                // On récupère l'objet mis à jour depuis le service
                var updatedLine = await _vehiculeService.UpdateLineAsync(id, line);

                if (updatedLine != null)
                {
                    // Succès : On renvoie l'objet à Angular avec un code 200
                    return Ok(updatedLine);
                }
                
                // Si le service renvoie null sans lancer d'exception
                return NotFound(new { message = $"La ligne avec l'ID {id} n'a pas pu être trouvée ou mise à jour." });
            }
            catch (Exception ex)
            {
                // On renvoie le message d'erreur propre extrait par HandleErrorResponse
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("line/{id}")]
        public async Task<IActionResult> DeleteLine(Guid id)
        {
            var success = await _vehiculeService.DeleteLineAsync(id);
            return success ? Ok() : BadRequest("Erreur lors de la suppression de la ligne");
        }
    }
}