using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Soroubat.Api.Interfaces;
using Soroubat.Api.Models;

namespace Soroubat.Api.Services
{
    /// <summary>
    /// Service de gestion des fiches gasoil.
    /// Toutes les opérations de modification vérifient l'appartenance de la ressource
    /// au projet du chef de chantier connecté avant tout appel BC.
    /// </summary>
    public class GasoilService : BaseService, IGasoilService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<GasoilService> _logger;

        private static readonly JsonSerializerOptions _writeOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        // Valeurs de statut BC — centralisées pour éviter les fautes de frappe
        private const string StatutEnCours = "En Cours";
        private const string StatutValide  = "Valider";

        public GasoilService(HttpClient httpClient, ILogger<GasoilService> logger)
        {
            _httpClient = httpClient;
            _logger     = logger;
        }

        // ─── HEADERS ──────────────────────────────────────────────────────────────

        public async Task<IEnumerable<GasoilHeader>> GetHeadersByJobAsync(string projectNo)
        {
            var url = $"gasoilHeaders?$filter=jobNo eq '{ODataEncode(projectNo)}'";
            _logger.LogInformation("[Gasoil] GET {Url}", url);

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                await HandleErrorResponse(response);

            var result = await response.Content.ReadFromJsonAsync<BCResponse<GasoilHeader>>();
            return result?.Value ?? Enumerable.Empty<GasoilHeader>();
        }

        public async Task<GasoilHeader?> GetHeaderByIdAsync(Guid id, string projectNo)
        {
            return await GetAndVerifyHeaderAsync(id, projectNo);
        }

        public async Task<GasoilHeader?> CreateHeaderAsync(GasoilHeader header, string projectNo)
        {
            // Sécurité : jobNo toujours forcé depuis le JWT
            header.JobNo   = projectNo;
            header.Status  = null; // Statut géré par BC à la création
            header.Lines   = null;

            var json    = JsonSerializer.Serialize(header, _writeOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogInformation("[Gasoil] POST gasoilHeaders");

            var response = await _httpClient.PostAsync("gasoilHeaders", content);

            if (!response.IsSuccessStatusCode)
                await HandleErrorResponse(response);

            return await response.Content.ReadFromJsonAsync<GasoilHeader>();
        }

        public async Task<GasoilHeader?> UpdateHeaderAsync(Guid id, GasoilHeader header, string projectNo)
        {
            // Vérification appartenance avant la mise à jour
            await GetAndVerifyHeaderAsync(id, projectNo);

            // Champs non modifiables via ce endpoint
            header.JobNo      = null;
            header.Status     = null;
            header.DocumentNo = null;
            header.FileNo     = null;
            header.Lines      = null;

            var json = JsonSerializer.Serialize(header, _writeOptions);
            _logger.LogInformation("[Gasoil] PATCH gasoilHeaders({Id})", id);

            var request  = BuildPatchRequest($"gasoilHeaders({id})", json);
            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                await HandleErrorResponse(response);

            return await response.Content.ReadFromJsonAsync<GasoilHeader>();
        }

        public async Task<bool> DeleteHeaderAsync(Guid id, string projectNo)
        {
            // Vérification appartenance avant suppression
            await GetAndVerifyHeaderAsync(id, projectNo);

            _logger.LogInformation("[Gasoil] DELETE gasoilHeaders({Id})", id);

            var request  = new HttpRequestMessage(HttpMethod.Delete, $"gasoilHeaders({id})");
            request.Headers.TryAddWithoutValidation("If-Match", "*");

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                await HandleErrorResponse(response);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ValiderFicheAsync(Guid id, string projectNo)
        {
            // 1. Vérification appartenance + statut actuel
            var existing = await GetAndVerifyHeaderAsync(id, projectNo);

            if (existing == null) return false;

            // 2. Validation métier : seule une fiche 'En Cours' peut être validée
            if (!existing.Status!.Equals(StatutEnCours, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException(
                    $"Validation impossible : statut actuel '{existing.Status}', attendu '{StatutEnCours}'.");

            // 3. PATCH sur le statut uniquement
            var json = JsonSerializer.Serialize(new { status = StatutValide });
            _logger.LogInformation("[Gasoil] PATCH valider gasoilHeaders({Id})", id);

            var request  = BuildPatchRequest($"gasoilHeaders({id})", json);
            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                await HandleErrorResponse(response);

            return response.IsSuccessStatusCode;
        }

        // ─── LIGNES ───────────────────────────────────────────────────────────────

        public async Task<GasoilLine?> CreateLineAsync(GasoilLine line, string projectNo)
        {
            if (string.IsNullOrEmpty(line.DocumentNo))
                throw new ArgumentException("Le numéro de document est requis pour créer une ligne.");

            // Vérification appartenance du document cible au projet
            await VerifyDocumentOwnershipAsync(line.DocumentNo, projectNo);

            // Sécurité : projectNo toujours forcé depuis le JWT
            line.ProjectNo  = projectNo;
            line.LineNo     = null; // Numéro de ligne géré par BC

            var json    = JsonSerializer.Serialize(line, _writeOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogInformation("[Gasoil] POST gasoilLines — Doc: {Doc}", line.DocumentNo);

            var response = await _httpClient.PostAsync("gasoilLines", content);

            if (!response.IsSuccessStatusCode)
                await HandleErrorResponse(response);

            return await response.Content.ReadFromJsonAsync<GasoilLine>();
        }

        public async Task<GasoilLine?> UpdateLineAsync(Guid id, GasoilLine line, string projectNo)
        {
            // Vérification appartenance + ETag
            var (existingLine, etag) = await GetLineAndEtagAsync(id, projectNo);
            if (existingLine == null) return null;

            // Champs non modifiables via ce endpoint
            line.ProjectNo  = null;
            line.DocumentNo = null;
            line.LineNo     = null;
            line.VehiclePlate = null;

            var json = JsonSerializer.Serialize(line, _writeOptions);
            _logger.LogInformation("[Gasoil] PATCH gasoilLines({Id})", id);

            var request  = BuildPatchRequest($"gasoilLines({id})", json, etag);
            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                await HandleErrorResponse(response);

            return await response.Content.ReadFromJsonAsync<GasoilLine>();
        }

        public async Task<bool> DeleteLineAsync(Guid id, string projectNo)
        {
            // Vérification appartenance avant suppression
            var (existingLine, _) = await GetLineAndEtagAsync(id, projectNo);
            if (existingLine == null) return false;

            _logger.LogInformation("[Gasoil] DELETE gasoilLines({Id})", id);

            var request  = new HttpRequestMessage(HttpMethod.Delete, $"gasoilLines({id})");
            request.Headers.TryAddWithoutValidation("If-Match", "*");

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                await HandleErrorResponse(response);

            return response.IsSuccessStatusCode;
        }

        // ─── ALERTES ──────────────────────────────────────────────────────────────

        public async Task<IEnumerable<GasoilHeader>> GetHeadersWithLinesAsync(string projectNo)
        {
            var url = $"gasoilHeaders?$filter=jobNo eq '{ODataEncode(projectNo)}'&$expand=gasoilLines";
            _logger.LogInformation("[Gasoil] GET (with lines) {Url}", url);

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                await HandleErrorResponse(response);

            var result = await response.Content.ReadFromJsonAsync<BCResponse<GasoilHeader>>();
            return result?.Value ?? Enumerable.Empty<GasoilHeader>();
        }

        // ─── HELPERS PRIVÉS ───────────────────────────────────────────────────────

        /// <summary>
        /// Récupère une fiche gasoil avec ses lignes et vérifie l'appartenance au projet.
        /// Retourne null si introuvable (404).
        /// Lève UnauthorizedAccessException si le projet ne correspond pas.
        /// </summary>
        private async Task<GasoilHeader?> GetAndVerifyHeaderAsync(Guid id, string projectNo)
        {
            var response = await _httpClient.GetAsync($"gasoilHeaders({id})?$expand=gasoilLines");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            if (!response.IsSuccessStatusCode)
                await HandleErrorResponse(response);

            var header = await response.Content.ReadFromJsonAsync<GasoilHeader>();

            if (header == null)
                return null;

            if (!header.JobNo!.Equals(projectNo, StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException(
                    "Accès refusé : cette fiche n'appartient pas à votre projet.");

            return header;
        }

        /// <summary>
        /// Récupère une ligne gasoil et son ETag, et vérifie l'appartenance au projet.
        /// Retourne (null, null) si introuvable (404).
        /// Lève UnauthorizedAccessException si le projet ne correspond pas.
        /// </summary>
        private async Task<(GasoilLine? dto, string? etag)> GetLineAndEtagAsync(Guid lineId, string projectNo)
        {
            var response = await _httpClient.GetAsync($"gasoilLines({lineId})");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return (null, null);

            if (!response.IsSuccessStatusCode)
                await HandleErrorResponse(response);

            var dto  = await response.Content.ReadFromJsonAsync<GasoilLine>();
            var etag = response.Headers.ETag?.ToString();

            if (dto != null && !dto.ProjectNo!.Equals(projectNo, StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException(
                    "Accès refusé : cette ligne n'appartient pas à votre projet.");

            return (dto, etag);
        }

        /// <summary>
        /// Vérifie que le document gasoil (par numéro) appartient au projet du chef connecté.
        /// Utilisé lors de la création de lignes pour sécuriser le document cible.
        /// </summary>
        private async Task VerifyDocumentOwnershipAsync(string documentNo, string projectNo)
        {
            var url      = $"gasoilHeaders?$filter=no eq '{ODataEncode(documentNo)}'";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                await HandleErrorResponse(response);

            var result = await response.Content.ReadFromJsonAsync<BCResponse<GasoilHeader>>();
            var header = result?.Value?.FirstOrDefault();

            if (header == null)
                throw new KeyNotFoundException($"Fiche gasoil '{documentNo}' introuvable.");

            if (!header.JobNo!.Equals(projectNo, StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException(
                    "Accès refusé : cette fiche n'appartient pas à votre projet.");
        }

        /// <summary>
        /// Construit une requête PATCH avec le header If-Match requis par BC.
        /// N'utilise pas DefaultRequestHeaders pour éviter les problèmes de concurrence.
        /// </summary>
        private static HttpRequestMessage BuildPatchRequest(string url, string json, string? etag = null)
        {
            var request = new HttpRequestMessage(HttpMethod.Patch, url)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            request.Headers.TryAddWithoutValidation("If-Match", etag ?? "*");
            return request;
        }
    }
}