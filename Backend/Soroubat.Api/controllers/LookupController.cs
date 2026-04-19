using Microsoft.AspNetCore.Mvc;
using Soroubat.Api.Services;
using Soroubat.Api.Interfaces;

namespace Soroubat.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LookupController : ControllerBase
    {
        // On injecte l'INTERFACE du service, pas le HttpClient
        private readonly ILookupService _lookupService;

        public LookupController(ILookupService lookupService)
        {
            _lookupService = lookupService;
        }

        [HttpGet("{entityName}")]
        public async Task<IActionResult> GetLookup(string entityName, [FromQuery] string? filter = null)
        {
            try
            {
                // Plus besoin de manipuler des URLs ici !
                // On délègue toute la "cuisine" au service.
                var content = await _lookupService.GetLookupDataAsync(entityName, filter);
                
                return Ok(content);
            }
            catch (Exception ex)
            {
                // Le service s'occupe de la logique technique, 
                // le contrôleur s'occupe de la réponse HTTP.
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}