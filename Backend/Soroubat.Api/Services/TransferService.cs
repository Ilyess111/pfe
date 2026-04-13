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

        public async Task<IEnumerable<TransferHeaderDto>> GetAllTransfersAsync()
        {
            var response = await _httpClient.GetAsync("transferHeaders");
            if (!response.IsSuccessStatusCode) await HandleErrorResponse(response);
            var result = await response.Content.ReadFromJsonAsync<BCResponse<TransferHeaderDto>>();
            return result?.Value ?? Enumerable.Empty<TransferHeaderDto>();
        }
        public async Task<TransferHeaderDto?> GetTransferByIdAsync(Guid id)
        {
            // Ajout des ' ' autour de l'ID pour respecter la syntaxe stricte OData
            // Et vérification qu'il n'y a aucun espace invisible
            var url = $"transferHeaders({id})?$expand=transferLines";
            
            var response = await _httpClient.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<TransferHeaderDto>();
            }

            await HandleErrorResponse(response);
            return null;
        }





        public async Task<bool> UpdateLineAsync(Guid id, JsonElement body)
        {
            var request = new HttpRequestMessage(HttpMethod.Patch, $"transferLines({id})")
            {
                Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
            };
            request.Headers.Add("If-Match", "*");
            return (await _httpClient.SendAsync(request)).IsSuccessStatusCode;
        }


        private async Task<int> GetNextLineNo(string docNo)
        {
            var response = await _httpClient.GetAsync($"transferLines?$filter=documentNo eq '{docNo}'&$orderby=lineNo desc&$top=1");
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<BCResponse<TransferLineDto>>();
                return (result?.Value?.FirstOrDefault()?.LineNo ?? 0) + 10000;
            }
            return 10000;
        }
    }
}