using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Soroubat.Api.Interfaces;
using Soroubat.Api.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Soroubat.Api.Controllers
{
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class VehiculePointageController : ControllerBase
{
    private readonly IVehiculeService _vehiculeService;

    // Récupération standardisée du projet depuis le jeton JWT (claim "projectNo")
    private string UserProjectNo => User.FindFirst("projectNo")?.Value ?? "";

    public VehiculePointageController(IVehiculeService vehiculeService)
    {
        _vehiculeService = vehiculeService;
    }

        [HttpGet("my-pointages")]
        public async Task<IActionResult> GetMyPointages()
        {
            try 
            {
                var projectNo = UserProjectNo;
                if (string.IsNullOrEmpty(projectNo)) return BadRequest("Projet non identifié.");

                var records = await _vehiculeService.GetHeadersByJobAsync(projectNo);
                return Ok(records);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("header/{id}")]
        public async Task<IActionResult> GetHeader(Guid id)
        {
            try 
            {
                var header = await _vehiculeService.GetHeaderByIdAsync(id, UserProjectNo);
                if (header == null) return NotFound();
                return Ok(header);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

    [HttpPost("header")]
    public async Task<IActionResult> CreateHeader([FromBody] VehiculePointageHeader header)
    {
        try 
        {
            var projectNo = UserProjectNo;
            if (string.IsNullOrEmpty(projectNo)) return Unauthorized();

            header.JobNo = projectNo;

            var result = await _vehiculeService.CreateHeaderAsync(header, projectNo);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
            
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("header/{id}")]
    public async Task<IActionResult> UpdateHeader(Guid id, [FromBody] VehiculePointageHeader header)
    {
        try 
        {
            var updated = await _vehiculeService.UpdateHeaderAsync(id, header, UserProjectNo);
            
            if (updated == null) return BadRequest(new { message = "Erreur lors de la mise à jour de l'en-tête." });
            
            return Ok(updated);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("header/{id}")]
    public async Task<IActionResult> DeleteHeader(Guid id)
    {
        try
        {
            var deleted = await _vehiculeService.DeleteHeaderAsync(id, UserProjectNo);
            if (deleted) return Ok(new { message = "Pointage supprimé avec succès." });
            return NotFound();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }


        // [HttpPost("line")]
        // public async Task<IActionResult> AddLine([FromBody] VehiculePointageLine line)
        // {
        //     var result = await _vehiculeService.AddLineAsync(line);
        //     return Ok(result);
        // }

        [HttpPatch("line/{id}")]
        public async Task<IActionResult> UpdateLine(Guid id, [FromBody] VehiculePointageLine line)
        {
            try 
            {
                // On récupère le projet depuis le token pour la sécurité
                var result = await _vehiculeService.UpdateLineAsync(id, line, UserProjectNo);
                
                if (result == null) return BadRequest(new { message = "Erreur lors de la mise à jour de la ligne." });
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // [HttpDelete("line/{id}")]
        // public async Task<IActionResult> DeleteLine(Guid id)
        // {
        //     var success = await _vehiculeService.DeleteLineAsync(id);
        //     return success ? Ok() : BadRequest("Erreur lors de la suppression de la ligne");
        // }
    }
}