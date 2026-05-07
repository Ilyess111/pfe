using Soroubat.Api.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Soroubat.Api.Interfaces;

namespace Soroubat.Api.Services
{
    public class StockService : BaseService, IStockService
    {
        private readonly HttpClient _httpClient;

        public StockService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<StockChantierDto>> GetStockByProjectAsync(string projectNo)
        {
            // Récupération des écritures comptables filtrées par Projet
            var stockResponse = await _httpClient.GetAsync($"itemLedgerEntries?$filter=jobNo eq '{projectNo}'");

            if (!stockResponse.IsSuccessStatusCode)
            {
                await HandleErrorResponseAsync(stockResponse);
                return new List<StockChantierDto>();
            }

            var data = await stockResponse.Content.ReadFromJsonAsync<BCResponse<StockChantierDto>>();

            if (data?.Value == null)
                return new List<StockChantierDto>();

            // AGRÉGATION : Groupement par Article et Emplacement pour obtenir le stock réel
            return data.Value
                .GroupBy(s => new { s.ItemNo, s.LocationCode, s.ItemDescription })
                .Select(g => new StockChantierDto
                {
                    ItemNo          = g.Key.ItemNo,
                    ItemDescription = g.Key.ItemDescription,
                    LocationCode    = g.Key.LocationCode,
                    Quantity        = g.Sum(x => x.Quantity), // Somme des entrées/sorties
                    JobNo           = projectNo,
                    LastPostingDate = g.Max(x => x.PostingDate) // Date du dernier mouvement
                })
                .Where(x => x.Quantity != 0) // On ne renvoie que les articles encore en stock
                .OrderBy(x => x.ItemDescription)
                .ToList();
        }
    }
}