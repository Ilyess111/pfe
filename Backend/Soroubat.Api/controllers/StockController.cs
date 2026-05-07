using Microsoft.AspNetCore.Mvc;
using Soroubat.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace Soroubat.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class StockController : ControllerBase
    {
        private readonly IStockService _stockService;

        // Extraction sécurisée du projet depuis le token (minuscule "p" comme spécifié)
        private string UserProjectNo => User.FindFirst("projectNo")?.Value ?? "";

        public StockController(IStockService stockService)
        {
            _stockService = stockService;
        }

        [HttpGet("my-stock")]
        public async Task<IActionResult> GetMyStock()
        {
            var projectNo = UserProjectNo;

            if (string.IsNullOrEmpty(projectNo))
            {
                return Unauthorized(new { message = "Aucun projet associé à ce compte." });
            }

            try 
            {
                var stock = await _stockService.GetStockByProjectAsync(projectNo);
                
                if (stock == null || stock.Count == 0)
                    return NotFound(new { message = "Le stock de votre chantier est actuellement vide." });

                return Ok(stock);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}