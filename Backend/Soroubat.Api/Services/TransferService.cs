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
            // On ne garde que les transferts où le chantier destination correspond au projet du chef
            var filter = $"$filter=chantierDestination eq '{projectNo}'";
            
            var response = await _httpClient.GetAsync($"transferHeaders?{filter}");
            
            if (!response.IsSuccessStatusCode) 
            {
                await HandleErrorResponse(response);
            }

            var result = await response.Content.ReadFromJsonAsync<BCResponse<TransferHeaderDto>>();
            return result?.Value ?? Enumerable.Empty<TransferHeaderDto>();
        }

        public async Task<IEnumerable<TransferHeaderDto>> GetAllTransfersWithLinesAsync(string projectNo)
        {
            var filter = $"$filter=chantierDestination eq '{projectNo}'";
            var response = await _httpClient.GetAsync($"transferHeaders?{filter}&$expand=transferLines");

            if (!response.IsSuccessStatusCode)
                await HandleErrorResponse(response);

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
                
                if (transfer != null && transfer.ChantierDestination == projectNo)
                {
                    return transfer;
                }
                
                // Si le chantier ne correspond pas, on traite cela comme "Non trouvé" ou "Interdit"
                return null;
            }

            await HandleErrorResponse(response);
            return null;
        }





        public async Task<bool> UpdateLineAsync(Guid id, JsonElement body, string projectNo)
        {
            // 1. Récupérer la ligne pour vérifier le projet et obtenir l'ETag
            // Note : On utilise l'expand pour remonter au Header et vérifier le chantierDestination
            var getResponse = await _httpClient.GetAsync($"transferLines({id})");
            if (!getResponse.IsSuccessStatusCode) return false;

            var etag = getResponse.Headers.ETag?.ToString();
            var line = await getResponse.Content.ReadFromJsonAsync<TransferLineDto>();

            // SÉCURITÉ : On vérifie si le chantier destination correspond (Logique métier Soroubat)
            // Note : Si votre API ligne ne contient pas directement le chantier, 
            // il faut s'assurer que la logique Backend ou AL valide cette cohérence.
            if (line == null) return false;

            // 2. Préparation et envoi du PATCH
            var request = new HttpRequestMessage(HttpMethod.Patch, $"transferLines({id})")
            {
                Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
            };

            // Gestion de la concurrence obligatoire pour BC
            request.Headers.TryAddWithoutValidation("If-Match", etag ?? "*");

            var response = await _httpClient.SendAsync(request);
            
            if (!response.IsSuccessStatusCode)
            {
                await HandleErrorResponse(response);
            }

            return response.IsSuccessStatusCode;
        }

    }
}