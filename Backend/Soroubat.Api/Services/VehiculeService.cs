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
    /// Service de gestion des pointages véhicules.
    /// Toutes les opérations de modification vérifient l'appartenance du pointage
    /// au projet du chef de chantier connecté avant tout appel BC.
    /// </summary>
    public class VehiculeService : BaseService, IVehiculeService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<VehiculeService> _logger;

        private static readonly JsonSerializerOptions _writeOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        // Valeurs de statut BC — centralisées pour éviter les fautes de frappe
        private const string StatutOuvert = "Ouvert";
        private const string StatutValide = "Validé";

        public VehiculeService(HttpClient httpClient, ILogger<VehiculeService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        // ─── HEADERS ──────────────────────────────────────────────────────────────

        public async Task<IEnumerable<VehiculePointageHeader>> GetHeadersByJobAsync(string projectNo)
        {
            var url = $"vehiculePointageHeaders?$filter=jobNo eq '{ODataEncode(projectNo)}'";
            _logger.LogInformation("[VehiculePointage] GET {Url}", url);

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                await HandleErrorResponse(response);

            var result = await response.Content.ReadFromJsonAsync<BCResponse<VehiculePointageHeader>>();
            return result?.Value ?? Enumerable.Empty<VehiculePointageHeader>();
        }

        public async Task<VehiculePointageHeader?> GetHeaderByIdAsync(Guid id, string projectNo)
        {
            var url = $"vehiculePointageHeaders({id})?$expand=vehiculePointageLines";
            _logger.LogInformation("[VehiculePointage] GET {Url}", url);

            var response = await _httpClient.GetAsync(url);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            if (!response.IsSuccessStatusCode)
                await HandleErrorResponse(response);

            var header = await response.Content.ReadFromJsonAsync<VehiculePointageHeader>();

            if (header == null)
                return null;

            // Vérification sécurité : le pointage doit appartenir au projet du chef connecté
            if (!header.JobNo!.Equals(projectNo, StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException(
                    "Accès refusé : ce pointage n'appartient pas à votre chantier.");

            return header;
        }

        public async Task<VehiculePointageHeader?> CreateHeaderAsync(VehiculePointageHeader header, string projectNo)
        {
            // Sécurité : jobNo toujours forcé depuis le JWT
            header.JobNo   = projectNo;
            header.Status  = null; // Statut géré par BC à la création
            header.Lines   = null; // Les lignes sont créées par BC

            var json    = JsonSerializer.Serialize(header, _writeOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogInformation("[VehiculePointage] POST vehiculePointageHeaders");

            // $expand sur le POST pour récupérer les lignes créées automatiquement par BC
            var response = await _httpClient.PostAsync(
                "vehiculePointageHeaders?$expand=vehiculePointageLines", content);

            if (!response.IsSuccessStatusCode)
                await HandleErrorResponse(response);

            return await response.Content.ReadFromJsonAsync<VehiculePointageHeader>();
        }

        public async Task<VehiculePointageHeader?> UpdateHeaderAsync(Guid id, VehiculePointageHeader header, string projectNo)
        {
            // Vérification appartenance avant la mise à jour
            await GetHeaderByIdAsync(id, projectNo);

            // Champs non modifiables via ce endpoint
            header.JobNo      = null;
            header.Status     = null;
            header.DocumentNo = null;
            header.Lines      = null;

            var json = JsonSerializer.Serialize(header, _writeOptions);
            _logger.LogInformation("[VehiculePointage] PATCH vehiculePointageHeaders({Id})", id);

            var request  = BuildPatchRequest($"vehiculePointageHeaders({id})", json);
            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                await HandleErrorResponse(response);

            return await response.Content.ReadFromJsonAsync<VehiculePointageHeader>();
        }

        public async Task<bool> DeleteHeaderAsync(Guid id, string projectNo)
        {
            // Vérification appartenance avant suppression
            var header = await GetHeaderByIdAsync(id, projectNo);
            if (header == null) return false;

            _logger.LogInformation("[VehiculePointage] DELETE vehiculePointageHeaders({Id})", id);

            var request  = new HttpRequestMessage(HttpMethod.Delete, $"vehiculePointageHeaders({id})");
            request.Headers.TryAddWithoutValidation("If-Match", "*");

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                await HandleErrorResponse(response);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ValiderPointageAsync(Guid id, string projectNo)
        {
            // 1. Vérification appartenance + récupération du statut actuel
            var existing = await GetHeaderByIdAsync(id, projectNo);
            if (existing == null) return false;

            // 2. Validation métier : seul un pointage 'Ouvert' peut être validé
            if (!existing.Status!.Equals(StatutOuvert, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException(
                    $"Validation impossible : statut actuel '{existing.Status}', attendu '{StatutOuvert}'.");

            // 3. PATCH sur le statut uniquement
            var json    = JsonSerializer.Serialize(new { status = StatutValide });
            _logger.LogInformation("[VehiculePointage] PATCH valider vehiculePointageHeaders({Id})", id);

            var request  = BuildPatchRequest($"vehiculePointageHeaders({id})", json);
            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                await HandleErrorResponse(response);

            return response.IsSuccessStatusCode;
        }

        // ─── LIGNES ───────────────────────────────────────────────────────────────

        public async Task<VehiculePointageLine?> UpdateLineAsync(Guid id, VehiculePointageLine line, string projectNo)
        {
            // 1. Vérification appartenance via le champ marche de la ligne
            var (existingLine, _) = await GetLineAndEtagAsync(id);
            if (existingLine == null) return null;

            if (!existingLine.Marche!.Equals(projectNo, StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException(
                    "Accès refusé : cette ligne n'appartient pas à votre chantier.");

            // 2. Champs non modifiables via ce endpoint
            line.DocumentNo = null;
            line.Marche     = null;

            var json = JsonSerializer.Serialize(line, _writeOptions);
            _logger.LogInformation("[VehiculePointage] PATCH vehiculePointageLines({Id})", id);

            var request  = BuildPatchRequest($"vehiculePointageLines({id})", json);
            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                await HandleErrorResponse(response);

            return await response.Content.ReadFromJsonAsync<VehiculePointageLine>();
        }

        // ─── ALERTES ──────────────────────────────────────────────────────────────

        public async Task<IEnumerable<VehiculePointageHeader>> GetHeadersWithLinesAsync(string projectNo)
        {
            var url = $"vehiculePointageHeaders?$filter=jobNo eq '{ODataEncode(projectNo)}'&$expand=vehiculePointageLines";
            _logger.LogInformation("[VehiculePointage] GET (with lines) {Url}", url);

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                await HandleErrorResponse(response);

            var result = await response.Content.ReadFromJsonAsync<BCResponse<VehiculePointageHeader>>();
            return result?.Value ?? Enumerable.Empty<VehiculePointageHeader>();
        }

        // ─── HELPERS PRIVÉS ───────────────────────────────────────────────────────

        /// <summary>
        /// Récupère une ligne de pointage et son ETag.
        /// Retourne (null, null) si introuvable (404).
        /// </summary>
        private async Task<(VehiculePointageLine? dto, string? etag)> GetLineAndEtagAsync(Guid lineId)
        {
            var response = await _httpClient.GetAsync($"vehiculePointageLines({lineId})");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return (null, null);

            if (!response.IsSuccessStatusCode)
                await HandleErrorResponse(response);

            var dto  = await response.Content.ReadFromJsonAsync<VehiculePointageLine>();
            var etag = response.Headers.ETag?.ToString();
            return (dto, etag);
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