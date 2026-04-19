using Soroubat.Api.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Soroubat.Api.Interfaces; // Assurez-vous d'avoir cet espace de noms pour IChefChantierService

namespace Soroubat.Api.Services
{
    public class StockService : IStockService
    {
        private readonly HttpClient _httpClient;
        private readonly IChefChantierService _chefChantierService; // Nouveau service injecté

        // Injection du HttpClient et du service ChefChantier
        public StockService(HttpClient httpClient, IChefChantierService chefChantierService)
        {
            _httpClient = httpClient;
            _chefChantierService = chefChantierService;
        }

        public async Task<List<StockChantierDto>> GetStockByChefEmailAsync(string email)
        {
            // 1. Récupération simplifiée du projet via le service dédié
            var jobNo = await _chefChantierService.GetJobNoByEmailAsync(email);
      
            // Si le projet n'est pas trouvé, on retourne une liste vide
            if (string.IsNullOrEmpty(jobNo))
                return new List<StockChantierDto>();

            // 2. Récupération du stock filtré par le projet (JobNo) dans Business Central
            // L'URL de base est déjà gérée par le HttpClient configuré dans Program.cs
            var stockResponse = await _httpClient.GetAsync($"itemLedgerEntries?$filter=jobNo eq '{jobNo}'");    

            if (!stockResponse.IsSuccessStatusCode) 
                return new List<StockChantierDto>();

            // Utilisation de votre classe de réponse standard BCResponse
            var data = await stockResponse.Content.ReadFromJsonAsync<BCResponse<StockChantierDto>>();

            if (data?.Value == null) 
                return new List<StockChantierDto>();

            // 3. Agrégation pour obtenir la quantité totale par article et par magasin
            return data.Value
                .GroupBy(s => new { s.ItemNo, s.LocationCode, s.ItemDescription })
                .Select(g => new StockChantierDto
                {
                    ItemNo = g.Key.ItemNo,
                    ItemDescription = g.Key.ItemDescription,
                    LocationCode = g.Key.LocationCode,
                    Quantity = g.Sum(x => x.Quantity), 
                    JobNo = jobNo
                })
                .Where(x => x.Quantity != 0) 
                .ToList();
        }
    }
}