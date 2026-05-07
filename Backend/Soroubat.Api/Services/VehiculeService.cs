using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net.Http.Json;
using System.Text;
using Soroubat.Api.Interfaces;
using Soroubat.Api.Models;
using Microsoft.Extensions.Logging;

namespace Soroubat.Api.Services
{
    public class VehiculeService : BaseService, IVehiculeService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<VehiculeService> _logger;

        public VehiculeService(HttpClient httpClient, ILogger<VehiculeService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<IEnumerable<VehiculePointageHeader>> GetHeadersByJobAsync(string projectNo)
        {
            var url = $"vehiculePointageHeaders?$filter=jobNo eq '{projectNo}'";
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) await HandleErrorResponseAsync(response);
            var result = await response.Content.ReadFromJsonAsync<BCResponse<VehiculePointageHeader>>();
            return result?.Value ?? Enumerable.Empty<VehiculePointageHeader>();
        }

        public async Task<VehiculePointageHeader> GetHeaderByIdAsync(Guid id, string projectNo)
        {
            var url = $"vehiculePointageHeaders({id})?$expand=vehiculePointageLines";
            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var header = await response.Content.ReadFromJsonAsync<VehiculePointageHeader>();
                if (header != null && header.JobNo == projectNo) return header;
                throw new UnauthorizedAccessException("Ce document n'appartient pas à votre projet.");
            }
            throw new Exception("Document introuvable.");
        }

        public async Task<VehiculePointageHeader?> CreateHeaderAsync(VehiculePointageHeader header, string projectNo)
        {
            header.JobNo = projectNo;
            var response = await _httpClient.PostAsJsonAsync("vehiculePointageHeaders?$expand=vehiculePointageLines", header);
            return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<VehiculePointageHeader>() : null;
        }

        // public async Task<VehiculePointageHeader?> UpdateHeaderAsync(Guid id, VehiculePointageHeader header, string projectNo)
        // {
        //     header.JobNo = projectNo;
        //     var options = new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
        //     _httpClient.DefaultRequestHeaders.Remove("If-Match");
        //     _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("If-Match", "*");
        //     var response = await _httpClient.PatchAsJsonAsync($"vehiculePointageHeaders({id})", header, options);
        //     return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<VehiculePointageHeader>() : null;
        // }

        public async Task<bool> DeleteHeaderAsync(Guid id, string projectNo)
        {
            await GetHeaderByIdAsync(id, projectNo); // Vérification projet
            _httpClient.DefaultRequestHeaders.Remove("If-Match");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("If-Match", "*");
            var response = await _httpClient.DeleteAsync($"vehiculePointageHeaders({id})");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ValiderPointageAsync(Guid id, string projectNo)
        {
            var existing = await GetHeaderByIdAsync(id, projectNo);
            if (!existing.Status.Equals("Ouvert", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Statut invalide pour validation.");

            var content = new StringContent("{\"status\": \"Validé\"}", Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"vehiculePointageHeaders({id})") { Content = content };
            request.Headers.TryAddWithoutValidation("If-Match", "*");
            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        public async Task<VehiculePointageLine?> UpdateLineAsync(Guid id, VehiculePointageLine line, string projectNo)
        {
            var options = new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
            _httpClient.DefaultRequestHeaders.Remove("If-Match");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("If-Match", "*");
            var response = await _httpClient.PatchAsJsonAsync($"vehiculePointageLines({id})", line, options);
            return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<VehiculePointageLine>() : null;
        }

        public async Task<IEnumerable<VehiculePointageHeader>> GetHeadersWithLinesAsync(string projectNo)
        {
            var url = $"vehiculePointageHeaders?$filter=jobNo eq '{projectNo}'&$expand=vehiculePointageLines";
            var response = await _httpClient.GetAsync(url);
            var result = await response.Content.ReadFromJsonAsync<BCResponse<VehiculePointageHeader>>();
            return result?.Value ?? Enumerable.Empty<VehiculePointageHeader>();
        }
    }
}