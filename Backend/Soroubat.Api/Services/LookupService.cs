using System.Text.Json;
using System.Net.Http.Headers;
using Soroubat.Api.Interfaces;
// dernier commit 

namespace Soroubat.Api.Services
{
    public class LookupService : ILookupService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<LookupService> _logger;

        public LookupService(HttpClient httpClient, ILogger<LookupService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

public async Task<JsonElement> GetLookupDataAsync(string entitySetName, string numProjet, string? additionalFilter = null)
{
    // Ajout de la logique de filtrage pour les salariés par chantier[cite: 10]
    string? projectFilter = entitySetName switch
    {
        "projects"     => $"code eq '{numProjet}'",
        "projectTasks" => $"projectNo eq '{numProjet}'",
        // "employees"    => $"chantier eq '{numProjet}'", 
        _              => null
    };

    var filters = new[] { projectFilter, additionalFilter }
        .Where(f => !string.IsNullOrEmpty(f))
        .ToList();

    var requestUri = entitySetName;
    if (filters.Any())
        requestUri += $"?$filter={string.Join(" and ", filters)}";

    var response = await _httpClient.GetAsync(requestUri);
    if (!response.IsSuccessStatusCode)
    {
        throw new HttpRequestException($"Erreur BC : {response.StatusCode}");
    }

    return await response.Content.ReadFromJsonAsync<JsonElement>();
}
}
}