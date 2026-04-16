using Microsoft.AspNetCore.Mvc;
using Soroubat.Api.Services;
using System.Threading.Tasks;

namespace Soroubat.Api.Controllers
{
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
            // RECUPÉRATION DE L'IDENTITÉ
            // Si User.Identity.Name est vide (pas de JWT/Session), on utilise l'email de test
            var userEmail = User.Identity?.Name;

            if (string.IsNullOrEmpty(userEmail))
            {
                // À REMPLACER PAR L'EMAIL RÉEL LORSQUE LE LOGIN SERA PRÊT
                userEmail = "test.email@exemple.com"; 
            }

            var stock = await _stockService.GetStockByChefEmailAsync(userEmail);
            
            if (stock == null || stock.Count == 0)
                return NotFound(new { message = "Aucun stock trouvé pour ce projet ou utilisateur inconnu." });

            return Ok(stock);
        }
    }
}