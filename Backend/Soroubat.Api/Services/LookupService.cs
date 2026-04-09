using System.Text.Json;
using System.Net.Http.Headers;

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

        public async Task<JsonElement> GetLookupDataAsync(string entitySetName, string? filter = null)
        {
            try
            {
                var requestUri = entitySetName; 

                if (!string.IsNullOrEmpty(filter))
                {
                    requestUri += $"?$filter={filter}";
                }

                // DEBUG : Ajoute cette ligne pour voir l'URL FINALE dans ta console
                var fullPath = new Uri(_httpClient.BaseAddress, requestUri);
                _logger.LogInformation($"URL RÉELLE APPELÉE : {fullPath}");

                var response = await _httpClient.GetAsync(requestUri);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Erreur BC ({response.StatusCode}): {errorContent}");
                    throw new HttpRequestException($"Erreur lors de la récupération des données : {response.StatusCode}");
                }

                return await response.Content.ReadFromJsonAsync<JsonElement>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception lors de l'accès au lookup {entitySetName}");
                throw;
            }
        }
    }
}