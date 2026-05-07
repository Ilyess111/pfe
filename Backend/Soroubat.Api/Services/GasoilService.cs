using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Soroubat.Api.Interfaces;
using Soroubat.Api.Models;

namespace Soroubat.Api.Services
{
    public class GasoilService : BaseService, IGasoilService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _options;

        public GasoilService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _options = new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
        }

        private void PrepareHeaders()
        {
            _httpClient.DefaultRequestHeaders.Remove("If-Match");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("If-Match", "*");
        }

        private async Task<GasoilHeader> GetAndVerifyHeaderAsync(Guid id, string projectNo)
        {
            var header = await _httpClient.GetFromJsonAsync<GasoilHeader>($"gasoilHeaders({id})?$expand=gasoilLines");
            if (header == null) throw new KeyNotFoundException("Fiche introuvable.");
            if (!header.JobNo.Equals(projectNo, StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException("Accès refusé pour ce projet.");
            return header;
        }

        public async Task<IEnumerable<GasoilHeader>> GetHeadersByJobAsync(string projectNo)
        {
            var response = await _httpClient.GetAsync($"gasoilHeaders?$filter=jobNo eq '{projectNo}'");
            if (!response.IsSuccessStatusCode) return Enumerable.Empty<GasoilHeader>();
            var result = await response.Content.ReadFromJsonAsync<BCResponse<GasoilHeader>>();
            return result?.Value ?? Enumerable.Empty<GasoilHeader>();
        }

        public async Task<GasoilHeader?> GetHeaderByIdAsync(Guid id, string projectNo) => await GetAndVerifyHeaderAsync(id, projectNo);

        public async Task<GasoilHeader?> CreateHeaderAsync(GasoilHeader header, string projectNo)
        {
            header.JobNo = projectNo;
            var response = await _httpClient.PostAsJsonAsync("gasoilHeaders", header, _options);
            if (!response.IsSuccessStatusCode) throw new Exception(await response.Content.ReadAsStringAsync());
            return await response.Content.ReadFromJsonAsync<GasoilHeader>();
        }

        public async Task<GasoilHeader?> UpdateHeaderAsync(Guid id, GasoilHeader header, string projectNo)
        {
            await GetAndVerifyHeaderAsync(id, projectNo);
            header.JobNo = projectNo;
            PrepareHeaders();
            var response = await _httpClient.PatchAsJsonAsync($"gasoilHeaders({id})", header, _options);
            return await response.Content.ReadFromJsonAsync<GasoilHeader>();
        }

        public async Task<bool> DeleteHeaderAsync(Guid id, string projectNo)
        {
            await GetAndVerifyHeaderAsync(id, projectNo);
            PrepareHeaders();
            var response = await _httpClient.DeleteAsync($"gasoilHeaders({id})");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ValiderFicheAsync(Guid id, string projectNo)
        {
            var existing = await GetAndVerifyHeaderAsync(id, projectNo);
            if (!existing.Status.Equals("En Cours", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Statut invalide.");

            var content = new StringContent("{\"status\": \"Valider\"}", Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"gasoilHeaders({id})") { Content = content };
            request.Headers.TryAddWithoutValidation("If-Match", "*");
            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        public async Task<GasoilLine?> CreateLineAsync(GasoilLine line, string projectNo)
        {
            line.ProjectNo = projectNo;
            var response = await _httpClient.PostAsJsonAsync("gasoilLines", line, _options);
            return await response.Content.ReadFromJsonAsync<GasoilLine>();
        }

        public async Task<GasoilLine?> UpdateLineAsync(Guid id, GasoilLine line, string projectNo)
        {
            PrepareHeaders();
            var response = await _httpClient.PatchAsJsonAsync($"gasoilLines({id})", line, _options);
            return await response.Content.ReadFromJsonAsync<GasoilLine>();
        }

        public async Task<bool> DeleteLineAsync(Guid id, string projectNo)
        {
            PrepareHeaders();
            var response = await _httpClient.DeleteAsync($"gasoilLines({id})");
            return response.IsSuccessStatusCode;
        }

        public async Task<IEnumerable<GasoilHeader>> GetHeadersWithLinesAsync(string projectNo)
        {
            var response = await _httpClient.GetAsync($"gasoilHeaders?$filter=jobNo eq '{projectNo}'&$expand=gasoilLines");
            var result = await response.Content.ReadFromJsonAsync<BCResponse<GasoilHeader>>();
            return result?.Value ?? Enumerable.Empty<GasoilHeader>();
        }
    }
}