using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Soroubat.Api.Interfaces;
using Soroubat.Api.Models;

namespace Soroubat.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class VehiculePointageController : ControllerBase
    {
        private readonly IVehiculeService _vehiculeService;
        private string UserProjectNo => User.FindFirst("projectNo")?.Value ?? "";

        public VehiculePointageController(IVehiculeService vehiculeService)
        {
            _vehiculeService = vehiculeService;
        }

        [HttpGet("my-pointages")]
        public async Task<IActionResult> GetMyPointages()
        {
            var records = await _vehiculeService.GetHeadersByJobAsync(UserProjectNo);
            return Ok(records);
        }

        [HttpGet("header/{id}")]
        public async Task<IActionResult> GetHeader(Guid id)
        {
            try {
                var header = await _vehiculeService.GetHeaderByIdAsync(id, UserProjectNo);
                return Ok(header);
            } catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpPost("header")]
        public async Task<IActionResult> CreateHeader([FromBody] VehiculePointageHeader header)
        {
            var result = await _vehiculeService.CreateHeaderAsync(header, UserProjectNo);
            return Ok(result);
        }

        // [HttpPatch("header/{id}")]
        // public async Task<IActionResult> UpdateHeader(Guid id, [FromBody] VehiculePointageHeader header)
        // {
        //     var updated = await _vehiculeService.UpdateHeaderAsync(id, header, UserProjectNo);
        //     return updated != null ? Ok(updated) : BadRequest();
        // }

        [HttpDelete("header/{id}")]
        public async Task<IActionResult> DeleteHeader(Guid id)
        {
            var deleted = await _vehiculeService.DeleteHeaderAsync(id, UserProjectNo);
            return deleted ? Ok() : NotFound();
        }

        [HttpPost("header/{id}/valider")]
        public async Task<IActionResult> ValiderPointage(Guid id)
        {
            try {
                var success = await _vehiculeService.ValiderPointageAsync(id, UserProjectNo);
                return success ? Ok(new { message = "Validé avec succès." }) : BadRequest();
            } catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpPatch("line/{id}")]
        public async Task<IActionResult> UpdateLine(Guid id, [FromBody] VehiculePointageLine line)
        {
            var result = await _vehiculeService.UpdateLineAsync(id, line, UserProjectNo);
            return result != null ? Ok(result) : BadRequest();
        }
    }
}