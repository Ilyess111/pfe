using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Soroubat.Api.Interfaces;
using Soroubat.Api.Models;

namespace Soroubat.Api.Controllers
{
    /// <summary>
    /// Consultation du stock chantier.
    /// Le stock est calculé en agrégeant les écritures comptables articles filtrées
    /// par projet. Ce contrôleur est en lecture seule.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class StockController : ControllerBase
    {
        private readonly IStockService _stockService;

        private string UserProjectNo => User.FindFirst("projectNo")?.Value ?? string.Empty;

        public StockController(IStockService stockService)
        {
            _stockService = stockService;
        }

        /// <summary>
        /// Retourne le stock agrégé du chantier du chef de chantier connecté.
        /// Retourne une liste vide si aucun article n'est en stock.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<StockChantierDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<StockChantierDto>>> GetMyStock()
        {
            if (string.IsNullOrEmpty(UserProjectNo))
                return Unauthorized(new { message = "Token invalide : aucun projet associé." });

            try
            {
                var stock = await _stockService.GetStockByProjectAsync(UserProjectNo);
                return Ok(stock);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }
    }
}