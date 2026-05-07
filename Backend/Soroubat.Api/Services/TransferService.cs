using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Soroubat.Api.Interfaces;
using Soroubat.Api.Models;

namespace Soroubat.Api.Services
{
    public class TransferService : BaseService, ITransferService
    {
        private readonly HttpClient _httpClient;

        public TransferService(HttpClient httpClient) => _httpClient = httpClient;

        public async Task<IEnumerable<TransferHeaderDto>> GetAllTransfersAsync(string projectNo)
        {
            var filter = $"$filter=chantierDestination eq '{projectNo}'";
            var response = await _httpClient.GetAsync($"transferHeaders?{filter}");
            
            if (!response.IsSuccessStatusCode) 
                await HandleErrorResponseAsync(response);

            var result = await response.Content.ReadFromJsonAsync<BCResponse<TransferHeaderDto>>();
            return result?.Value ?? Enumerable.Empty<TransferHeaderDto>();
        }

        public async Task<IEnumerable<TransferHeaderDto>> GetAllTransfersWithLinesAsync(string projectNo)
        {
            var filter = $"$filter=chantierDestination eq '{projectNo}'";
            var response = await _httpClient.GetAsync($"transferHeaders?{filter}&$expand=transferLines");

            if (!response.IsSuccessStatusCode)
                await HandleErrorResponseAsync(response);

            var result = await response.Content.ReadFromJsonAsync<BCResponse<TransferHeaderDto>>();
            return result?.Value ?? Enumerable.Empty<TransferHeaderDto>();
        }

        public async Task<TransferHeaderDto?> GetTransferByIdAsync(Guid id, string projectNo)
        {
            var url = $"transferHeaders({id})?$expand=transferLines";
            var response = await _httpClient.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                var transfer = await response.Content.ReadFromJsonAsync<TransferHeaderDto>();
                
                // Isolation : Vérification post-récupération
                if (transfer != null && transfer.ChantierDestination == projectNo)
                {
                    return transfer;
                }
                return null;
            }

            await HandleErrorResponseAsync(response);
            return null;
        }

        public async Task<bool> UpdateLineAsync(Guid id, JsonElement body, string projectNo)
        {
            // 1. Récupérer la ligne pour obtenir l'ETag et le DocumentNo
            var getLineResponse = await _httpClient.GetAsync($"transferLines({id})");
            if (!getLineResponse.IsSuccessStatusCode) return false;

            var etag = getLineResponse.Headers.ETag?.ToString();
            var line = await getLineResponse.Content.ReadFromJsonAsync<TransferLineDto>();

            if (line == null || string.IsNullOrEmpty(line.DocumentNo)) return false;

            // 2. SÉCURITÉ : Récupérer le header parent pour valider le ChantierDestination
            var headerFilter = $"$filter=no eq '{line.DocumentNo}'";
            var headerResponse = await _httpClient.GetAsync($"transferHeaders?{headerFilter}");
            if (!headerResponse.IsSuccessStatusCode) return false;

            var headerResult = await headerResponse.Content.ReadFromJsonAsync<BCResponse<TransferHeaderDto>>();
            var header = headerResult?.Value?.FirstOrDefault();

            if (header == null || header.ChantierDestination != projectNo)
            {
                // Rejet : Tentative de modification d'une ligne d'un autre chantier
                return false; 
            }

            // 3. Préparation et envoi du PATCH
            var request = new HttpRequestMessage(HttpMethod.Patch, $"transferLines({id})")
            {
                Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
            };

            // Gestion de la concurrence obligatoire pour BC
            request.Headers.TryAddWithoutValidation("If-Match", etag ?? "*");

            var patchResponse = await _httpClient.SendAsync(request);
            
            if (!patchResponse.IsSuccessStatusCode)
                await HandleErrorResponseAsync(patchResponse);

            return patchResponse.IsSuccessStatusCode;
        }
    }
}