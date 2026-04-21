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

    public StockService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<StockChantierDto>> GetStockByProjectAsync(string projectNo)
    {
        // Filtrage direct dans Business Central via OData
        var stockResponse = await _httpClient.GetAsync($"itemLedgerEntries?$filter=jobNo eq '{projectNo}'");    

        if (!stockResponse.IsSuccessStatusCode) 
            return new List<StockChantierDto>();

        var data = await stockResponse.Content.ReadFromJsonAsync<BCResponse<StockChantierDto>>();

        if (data?.Value == null) 
            return new List<StockChantierDto>();

        // Agrégation par article et magasin (Location)
        return data.Value
            .GroupBy(s => new { s.ItemNo, s.LocationCode, s.ItemDescription })
            .Select(g => new StockChantierDto
            {
                ItemNo = g.Key.ItemNo,
                ItemDescription = g.Key.ItemDescription,
                LocationCode = g.Key.LocationCode,
                Quantity = g.Sum(x => x.Quantity), // Somme des entrées/sorties
                JobNo = projectNo
            })
            .Where(x => x.Quantity != 0) // On ne montre que ce qui est en stock
            .ToList();
    }
}
}