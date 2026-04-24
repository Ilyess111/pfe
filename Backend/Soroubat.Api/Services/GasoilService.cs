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
            _options = new JsonSerializerOptions 
            { 
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull 
            };
        }

        // ─── HELPERS PRIVÉS ───────────────────────────────────────────────

        private void PrepareHeaders()
        {
            _httpClient.DefaultRequestHeaders.Remove("If-Match");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("If-Match", "*");
        }

        // Vérification centralisée : récupère le header ET vérifie le projet
        private async Task<GasoilHeader> GetAndVerifyHeaderAsync(Guid id, string projectNo)
        {
            var header = await _httpClient.GetFromJsonAsync<GasoilHeader>(
                $"gasoilHeaders({id})?$expand=gasoilLines");

            if (header == null)
                throw new KeyNotFoundException("Fiche gasoil introuvable.");

            if (!header.JobNo.Equals(projectNo, StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException(
                    "Accès refusé : cette fiche n'appartient pas à votre projet.");

            return header;
        }

        // ─── HEADERS ──────────────────────────────────────────────────────

        public async Task<IEnumerable<GasoilHeader>> GetHeadersByJobAsync(string projectNo)
        {
            var url = $"gasoilHeaders?$filter=jobNo eq '{projectNo}'";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return Enumerable.Empty<GasoilHeader>();

            if (response.Content.Headers.ContentLength == 0)
                return Enumerable.Empty<GasoilHeader>();

            try
            {
                var result = await response.Content.ReadFromJsonAsync<BCResponse<GasoilHeader>>();
                return result?.Value ?? Enumerable.Empty<GasoilHeader>();
            }
            catch (JsonException)
            {
                return Enumerable.Empty<GasoilHeader>();
            }
        }

        public async Task<GasoilHeader?> GetHeaderByIdAsync(Guid id, string projectNo)
        {
            // GetAndVerifyHeaderAsync lance UnauthorizedAccessException si projet différent
            return await GetAndVerifyHeaderAsync(id, projectNo);
        }

        public async Task<GasoilHeader?> CreateHeaderAsync(GasoilHeader header, string projectNo)
        {
            // SÉCURITÉ : forcer le projet depuis le JWT
            header.JobNo = projectNo;

            var response = await _httpClient.PostAsJsonAsync("gasoilHeaders", header, _options);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                throw new Exception($"Erreur BC : {errorBody}");
            }

            return await response.Content.ReadFromJsonAsync<GasoilHeader>();
        }

        public async Task<GasoilHeader?> UpdateHeaderAsync(Guid id, GasoilHeader header, string projectNo)
        {
            // SÉCURITÉ : vérifier que le header appartient au projet
            await GetAndVerifyHeaderAsync(id, projectNo);

            // Forcer le projet pour éviter toute manipulation
            header.JobNo = projectNo;

            PrepareHeaders();
            var response = await _httpClient.PatchAsJsonAsync($"gasoilHeaders({id})", header, _options);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                throw new Exception($"Erreur BC : {errorBody}");
            }

            return await response.Content.ReadFromJsonAsync<GasoilHeader>();
        }

        public async Task<bool> DeleteHeaderAsync(Guid id, string projectNo)
        {
            // SÉCURITÉ : vérifier appartenance avant suppression
            await GetAndVerifyHeaderAsync(id, projectNo);

            PrepareHeaders();
            var response = await _httpClient.DeleteAsync($"gasoilHeaders({id})");

            if (!response.IsSuccessStatusCode)
                await HandleErrorResponse(response);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ValiderFicheAsync(Guid id, string projectNo)
        {
            // SÉCURITÉ : vérifier appartenance
            var existing = await GetAndVerifyHeaderAsync(id, projectNo);

            // VALIDATION MÉTIER
            if (!existing.Status.Equals("En Cours", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException(
                    $"Impossible de valider : statut actuel '{existing.Status}', attendu 'En Cours'.");

            var json = """{"status": "Valider"}""";
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"gasoilHeaders({id})")
            {
                Content = content
            };
            request.Headers.TryAddWithoutValidation("If-Match", "*");

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                await HandleErrorResponse(response);

            return response.IsSuccessStatusCode;
        }

        // ─── LIGNES ───────────────────────────────────────────────────────

        public async Task<GasoilLine?> CreateLineAsync(GasoilLine line, string projectNo)
        {
            // SÉCURITÉ : vérifier que le header cible appartient au projet
            if (string.IsNullOrEmpty(line.DocumentNo))
                throw new ArgumentException("Le numéro de document est requis pour créer une ligne.");

            // Récupérer le header via son DocumentNo pour vérifier le projet
            var url = $"gasoilHeaders?$filter=documentNo eq '{line.DocumentNo}'";
            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<BCResponse<GasoilHeader>>();
                var header = result?.Value?.FirstOrDefault();

                if (header == null)
                    throw new KeyNotFoundException("Header introuvable pour ce numéro de document.");

                if (!header.JobNo.Equals(projectNo, StringComparison.OrdinalIgnoreCase))
                    throw new UnauthorizedAccessException(
                        "Accès refusé : ce document n'appartient pas à votre projet.");
            }

            // SÉCURITÉ : forcer le projet sur la ligne
            line.ProjectNo = projectNo;

            var createResponse = await _httpClient.PostAsJsonAsync("gasoilLines", line, _options);

            if (!createResponse.IsSuccessStatusCode)
            {
                var errorBody = await createResponse.Content.ReadAsStringAsync();
                throw new Exception($"Erreur BC : {errorBody}");
            }

            return await createResponse.Content.ReadFromJsonAsync<GasoilLine>();
        }

        public async Task<GasoilLine?> UpdateLineAsync(Guid id, GasoilLine line, string projectNo)
        {
            // SÉCURITÉ : récupérer la ligne existante et vérifier le projet
            var existingLine = await _httpClient.GetFromJsonAsync<GasoilLine>($"gasoilLines({id})");

            if (existingLine == null)
                throw new KeyNotFoundException("Ligne introuvable.");

            if (!existingLine.ProjectNo.Equals(projectNo, StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException(
                    "Accès refusé : cette ligne n'appartient pas à votre projet.");

            // Forcer le projet
            line.ProjectNo = projectNo;

            PrepareHeaders();
            var response = await _httpClient.PatchAsJsonAsync($"gasoilLines({id})", line, _options);

            if (!response.IsSuccessStatusCode)
                await HandleErrorResponse(response);

            return await response.Content.ReadFromJsonAsync<GasoilLine>();
        }

        public async Task<bool> DeleteLineAsync(Guid id, string projectNo)
        {
            // SÉCURITÉ : vérifier appartenance avant suppression
            var existingLine = await _httpClient.GetFromJsonAsync<GasoilLine>($"gasoilLines({id})");

            if (existingLine == null)
                throw new KeyNotFoundException("Ligne introuvable.");

            if (!existingLine.ProjectNo.Equals(projectNo, StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException(
                    "Accès refusé : cette ligne n'appartient pas à votre projet.");

            PrepareHeaders();
            var response = await _httpClient.DeleteAsync($"gasoilLines({id})");

            if (!response.IsSuccessStatusCode)
                await HandleErrorResponse(response);

            return response.IsSuccessStatusCode;
        }
    }
}