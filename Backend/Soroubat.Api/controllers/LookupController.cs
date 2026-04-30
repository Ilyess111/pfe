using Microsoft.AspNetCore.Mvc;
using Soroubat.Api.Services;
using Soroubat.Api.Interfaces;
using System.Security.Claims; 

namespace Soroubat.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LookupController : ControllerBase
    {
        private readonly ILookupService _lookupService;
        private readonly IChefChantierService _chefChantierService;

        public LookupController(ILookupService lookupService , IChefChantierService chefChantierService)
        {
            _lookupService = lookupService;
            _chefChantierService = chefChantierService;
        }

[HttpGet("{entityName}")]
public async Task<IActionResult> GetLookup(string entityName, [FromQuery] string? filter = null)
{
    try
    {
        var email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email");

        if (string.IsNullOrEmpty(email))
            return Unauthorized(new { message = "Email introuvable dans le token." });

        var numProjet = await _chefChantierService.GetJobNoByEmailAsync(email);

        if (string.IsNullOrEmpty(numProjet))
            return NotFound(new { message = $"Aucun projet associé au compte '{email}'." });

        var content = await _lookupService.GetLookupDataAsync(entityName, numProjet, filter);
        return Ok(content);
    }
    catch (Exception ex)
    {
        return BadRequest(new { message = ex.Message });
    }
}
    }
}