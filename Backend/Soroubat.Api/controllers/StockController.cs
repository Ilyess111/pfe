using Microsoft.AspNetCore.Mvc;
using Soroubat.Api.Services;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Soroubat.Api.Controllers
{
    [Authorize] // Sécurise l'accès : le jeton JWT est désormais obligatoire
    [ApiController]
    [Route("api/[controller]")]
    public class StockController : ControllerBase
    {
        private readonly IStockService _stockService;

        public StockController(IStockService stockService)
        {
            _stockService = stockService;
        }

        [HttpGet("my-stock")]
        public async Task<IActionResult> GetMyStock()
        {
            // Récupération de l'email directement depuis les Claims du Token JWT
            var userEmail = User.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized(new { message = "Email non trouvé dans le jeton." });
            }

            var stock = await _stockService.GetStockByChefEmailAsync(userEmail);
            
            if (stock == null || stock.Count == 0)
                return NotFound(new { message = "Aucun stock trouvé pour ce projet ou utilisateur inconnu." });

            return Ok(stock);
        }
    }
}