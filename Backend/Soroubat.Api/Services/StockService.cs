using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Soroubat.Api.Interfaces;
using Soroubat.Api.Models;

namespace Soroubat.Api.Services
{
    /// <summary>
    /// Service de consultation du stock chantier.
    /// Les écritures comptables articles sont récupérées depuis BC puis agrégées en mémoire
    /// par article et magasin pour produire une vue stock synthétique par chantier.
    /// </summary>
    public class StockService : BaseService, IStockService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<StockService> _logger;

        public StockService(HttpClient httpClient, ILogger<StockService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<StockChantierDto>> GetStockByProjectAsync(string projectNo)
        {
            var url = $"itemLedgerEntries?$filter=jobNo eq '{ODataEncode(projectNo)}'";
            _logger.LogInformation("[Stock] GET {Url}", url);

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("[Stock] Erreur BC lors de la récupération du stock — HTTP {Status}", (int)response.StatusCode);
                await HandleErrorResponse(response);
            }

            var data = await response.Content.ReadFromJsonAsync<BCResponse<StockChantierDto>>();

            if (data?.Value == null || !data.Value.Any())
                return new List<StockChantierDto>();

            // Agrégation en mémoire : somme des quantités par article + magasin
            // et calcul du dernier mouvement (Max postingDate) pour le suivi des stocks dormants
            return data.Value
                .GroupBy(entry => new
                {
                    entry.ItemNo,
                    entry.LocationCode,
                    entry.ItemDescription
                })
                .Select(g => new StockChantierDto
                {
                    ItemNo          = g.Key.ItemNo,
                    ItemDescription = g.Key.ItemDescription,
                    LocationCode    = g.Key.LocationCode,
                    JobNo           = projectNo,
                    Quantity        = g.Sum(x => x.Quantity),
                    // LastPostingDate = dernier mouvement connu — utilisé par AlertService
                    LastPostingDate = g.Max(x => x.LastPostingDate)
                })
                // Exclure les articles soldés (entrées et sorties s'annulent)
                .Where(x => x.Quantity != 0)
                .OrderBy(x => x.ItemNo)
                .ToList();
        }
    }
}