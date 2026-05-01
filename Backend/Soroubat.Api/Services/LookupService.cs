using System.Text.Json;
using System.Net.Http.Headers;
using Soroubat.Api.Interfaces;


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

public async Task<JsonElement> GetLookupDataAsync(
    string entitySetName,
    string numProjet,
    string? additionalFilter = null)
{
    // string? projectFilter = entitySetName switch
    // {
    //     "projects"     => $"code eq '{numProjet}'",
    //     "projectTasks" => $"projectNo eq '{numProjet}'",
    //     _              => null
    // };
    string? projectFilter = entitySetName switch
    {
        "projects"     => $"code eq '{numProjet}'",     
        "projectTasks" => $"projectNo eq '{numProjet}'", 
    };

    var filters = new[] { projectFilter, additionalFilter }
        .Where(f => !string.IsNullOrEmpty(f))
        .ToList();

    var requestUri = entitySetName;
    if (filters.Any())
        requestUri += $"?$filter={string.Join(" and ", filters)}";

    _logger.LogInformation($"[Lookup] URL : {new Uri(_httpClient.BaseAddress!, requestUri)}");

    var response = await _httpClient.GetAsync(requestUri);

    if (!response.IsSuccessStatusCode)
    {
        var errorContent = await response.Content.ReadAsStringAsync();
        _logger.LogError($"Erreur BC ({response.StatusCode}): {errorContent}");
        throw new HttpRequestException($"Erreur lookup : {response.StatusCode}");
    }

    return await response.Content.ReadFromJsonAsync<JsonElement>();
}
}
}