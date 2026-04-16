using Soroubat.Api.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Soroubat.Api.Services
{
    public class StockService : IStockService
    {
        private readonly HttpClient _httpClient;

        public StockService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<StockChantierDto>> GetStockByChefEmailAsync(string email)
        {
            // 1. Récupération du projet lié au chef de chantier (API 50141)
            // Note : On utilise 'email' tel qu'il est défini dans votre nouvelle table BC
            var chefResponse = await _httpClient.GetAsync($"chefsChantier?$filter=email eq '{email}'");
            
            if (!chefResponse.IsSuccessStatusCode) return new List<StockChantierDto>();
            
            var chefData = await chefResponse.Content.ReadFromJsonAsync<BCResponse<ChefChantierDto>>();
            var chef = chefData?.Value?.FirstOrDefault();

            if (chef == null || string.IsNullOrEmpty(chef.NumProjet))
                return new List<StockChantierDto>();

            // 2. Récupération du stock filtré par le projet (JobNo)
            // Utilise la clé STG_Key38 sur le serveur pour la performance
            var stockResponse = await _httpClient.GetAsync($"itemLedgerEntries?$filter=jobNo eq '{chef.NumProjet}'");

            if (!stockResponse.IsSuccessStatusCode) return new List<StockChantierDto>();

            var data = await stockResponse.Content.ReadFromJsonAsync<BCResponse<StockChantierDto>>();

            if (data?.Value == null) return new List<StockChantierDto>();

            // 3. Agrégation pour obtenir la quantité totale par article et par magasin
            return data.Value
                .GroupBy(s => new { s.ItemNo, s.LocationCode, s.ItemDescription })
                .Select(g => new StockChantierDto
                {
                    ItemNo = g.Key.ItemNo,
                    ItemDescription = g.Key.ItemDescription,
                    LocationCode = g.Key.LocationCode,
                    Quantity = g.Sum(x => x.Quantity), // Somme des entrées/sorties
                    JobNo = chef.NumProjet
                })
                .Where(x => x.Quantity != 0) // On ne garde que ce qui est réellement en stock
                .ToList();
        }
    }
}