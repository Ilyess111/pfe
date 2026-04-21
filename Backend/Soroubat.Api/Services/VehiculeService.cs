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
            
            if (!response.IsSuccessStatusCode)
            {
                await HandleErrorResponse(response);
            }

            var result = await response.Content.ReadFromJsonAsync<BCResponse<VehiculePointageHeader>>();
            return result?.Value ?? Enumerable.Empty<VehiculePointageHeader>();
        }

        public async Task<VehiculePointageHeader?> GetHeaderByIdAsync(Guid id, string projectNo)
        {
            var url = $"vehiculePointageHeaders({id})?$expand=vehiculePointageLines";
            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var header = await response.Content.ReadFromJsonAsync<VehiculePointageHeader>();
                if (header != null && header.JobNo == projectNo)
                {
                    return header;
                }
                throw new UnauthorizedAccessException("Accès refusé à ce projet.");
            }

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;

            await HandleErrorResponse(response);
            return null;
        }

        public async Task<VehiculePointageHeader?> CreateHeaderAsync(VehiculePointageHeader header, string projectNo)
        {
            header.JobNo = projectNo;

            var response = await _httpClient.PostAsJsonAsync("vehiculePointageHeaders?$expand=vehiculePointageLines", header);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<VehiculePointageHeader>();
            }

            await HandleErrorResponse(response);
            return null;
        }

        public async Task<VehiculePointageHeader?> UpdateHeaderAsync(Guid id, VehiculePointageHeader header, string projectNo)
        {
            header.JobNo = projectNo;

            var options = new JsonSerializerOptions 
            { 
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull 
            };

            _httpClient.DefaultRequestHeaders.Remove("If-Match");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("If-Match", "*");

            var response = await _httpClient.PatchAsJsonAsync($"vehiculePointageHeaders({id})", header, options);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<VehiculePointageHeader>();
            }

            await HandleErrorResponse(response);
            return null;
        }

        public async Task<bool> DeleteHeaderAsync(Guid id, string projectNo)
        {
            // 1. SÉCURITÉ : On tente de récupérer le header
            // Si le projet ne correspond pas, GetHeaderByIdAsync lancera une UnauthorizedAccessException
            var header = await GetHeaderByIdAsync(id, projectNo);
            
            if (header == null) return false;

            // 2. SUPPRESSION : Si on arrive ici, l'utilisateur est autorisé
            // On nettoie les headers pour éviter les conflits d'ETag sur le DELETE
            _httpClient.DefaultRequestHeaders.Remove("If-Match");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("If-Match", "*");

            var response = await _httpClient.DeleteAsync($"vehiculePointageHeaders({id})");

            if (!response.IsSuccessStatusCode)
            {
                await HandleErrorResponse(response);
            }

            return response.IsSuccessStatusCode;
        }

        public async Task<VehiculePointageLine?> UpdateLineAsync(Guid id, VehiculePointageLine line, string projectNo)
        {
            // 1. Configuration de la sérialisation (ignorer les nulls pour ne pas écraser BC)
            var options = new JsonSerializerOptions 
            { 
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull 
            };

            // 2. Gestion de la concurrence (ETag) pour Business Central
            _httpClient.DefaultRequestHeaders.Remove("If-Match");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("If-Match", "*");

            // 3. Envoi de la requête PATCH
            // Note : On utilise PatchAsJsonAsync pour l'homogénéité
            var response = await _httpClient.PatchAsJsonAsync($"vehiculePointageLines({id})", line, options);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<VehiculePointageLine>();
            }

            // 4. Gestion d'erreur centralisée
            await HandleErrorResponse(response);
            return null;
        }


    }
}