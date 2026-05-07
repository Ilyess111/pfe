using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Soroubat.Api.Interfaces;
using Soroubat.Api.Models;

namespace Soroubat.Api.Services
{
    /// <summary>
    /// Interagit avec les APIs Business Central PurchaseRequestAPI et PurchaseRequestLineAPI.
    /// </summary>
    public class PurchaseRequestService : BaseService, IPurchaseRequestService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PurchaseRequestService> _logger;

        // Options de sérialisation partagées : on n'envoie jamais de champs nuls à BC
        private static readonly JsonSerializerOptions _serializerOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public PurchaseRequestService(HttpClient httpClient, ILogger<PurchaseRequestService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        // ── HELPERS PRIVÉS ────────────────────────────────────────────────────

        /// <summary>
        /// Récupère un en-tête et vérifie qu'il appartient au projet du chef connecté.
        /// Lève <see cref="KeyNotFoundException"/> si introuvable.
        /// Lève <see cref="UnauthorizedAccessException"/> si le projet ne correspond pas.
        /// </summary>
        private async Task<(PurchaseRequestDto Header, string? ETag)> GetAndVerifyHeaderAsync(
            Guid id, string projectNo)
        {
            var response = await _httpClient.GetAsync($"purchaseRequests({id})");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                throw new KeyNotFoundException($"La demande d'achat '{id}' est introuvable dans Business Central.");

            if (!response.IsSuccessStatusCode)
                await HandleErrorResponseAsync(response);

            var header = await response.Content.ReadFromJsonAsync<PurchaseRequestDto>();

            if (header == null)
                throw new KeyNotFoundException($"La demande d'achat '{id}' est introuvable dans Business Central.");

            if (!string.Equals(header.JobNo, projectNo, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning(
                    "[PR] Accès refusé : demande {Id} appartient au projet {HeaderProject}, " +
                    "chef connecté au projet {UserProject}", id, header.JobNo, projectNo);
                throw new UnauthorizedAccessException(
                    "Accès refusé : cette demande n'appartient pas à votre projet.");
            }

            var etag = response.Headers.ETag?.ToString();
            return (header, etag);
        }

        /// <summary>
        /// Récupère une ligne et vérifie qu'elle appartient au projet du chef connecté.
        /// </summary>
        private async Task<(PurchaseRequestLineDto Line, string? ETag)> GetAndVerifyLineAsync(
            Guid lineId, string projectNo)
        {
            var response = await _httpClient.GetAsync($"purchaseRequestLines({lineId})");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                throw new KeyNotFoundException($"La ligne '{lineId}' est introuvable dans Business Central.");

            if (!response.IsSuccessStatusCode)
                await HandleErrorResponseAsync(response);

            var line = await response.Content.ReadFromJsonAsync<PurchaseRequestLineDto>();

            if (line == null)
                throw new KeyNotFoundException($"La ligne '{lineId}' est introuvable dans Business Central.");

            if (!string.Equals(line.JobNo, projectNo, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning(
                    "[PR] Accès refusé ligne {LineId} : projet {LineProject} ≠ {UserProject}",
                    lineId, line.JobNo, projectNo);
                throw new UnauthorizedAccessException(
                    "Accès refusé : cette ligne n'appartient pas à votre projet.");
            }

            var etag = response.Headers.ETag?.ToString();
            return (line, etag);
        }

        // ── EN-TÊTES ──────────────────────────────────────────────────────────

        public async Task<IEnumerable<PurchaseRequestDto>> GetAllRequestsAsync(string projectNo)
        {
            var url = $"purchaseRequests?$filter=jobNo eq '{projectNo}'";
            _logger.LogInformation("[PR] GetAll pour projet {ProjectNo}", projectNo);

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                await HandleErrorResponseAsync(response);

            var result = await response.Content.ReadFromJsonAsync<BCResponse<PurchaseRequestDto>>();
            return result?.Value ?? Enumerable.Empty<PurchaseRequestDto>();
        }

        public async Task<PurchaseRequestDto?> GetRequestByIdAsync(Guid id, string projectNo)
        {
            var url = $"purchaseRequests({id})?$expand=purchaseRequestLines";
            var response = await _httpClient.GetAsync(url);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            if (!response.IsSuccessStatusCode)
                await HandleErrorResponseAsync(response);

            var request = await response.Content.ReadFromJsonAsync<PurchaseRequestDto>();

            if (request != null &&
                !string.Equals(request.JobNo, projectNo, StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException("Accès refusé : cette demande n'appartient pas à votre projet.");
            }

            return request;
        }

        public async Task<PurchaseRequestDto> CreateHeaderAsync(PurchaseRequestDto header, string projectNo)
        {
            // SÉCURITÉ : forcer le projet depuis le JWT
            header.JobNo = projectNo;

            var json = JsonSerializer.Serialize(header, _serializerOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogInformation("[PR] Création en-tête pour projet {ProjectNo}", projectNo);

            var response = await _httpClient.PostAsync("purchaseRequests", content);

            if (!response.IsSuccessStatusCode)
                await HandleErrorResponseAsync(response);

            return (await response.Content.ReadFromJsonAsync<PurchaseRequestDto>())!;
        }

        public async Task<bool> CreateLinesAsync(List<PurchaseRequestLineDto> lines, string projectNo)
        {
            if (lines == null || !lines.Any()) return false;

            var firstDocNo = lines.First().DocumentNo;

            if (string.IsNullOrWhiteSpace(firstDocNo))
                throw new ArgumentException(
                    "Le numéro de document (DocumentNo) est obligatoire pour créer des lignes.");

            // Récupération du dernier LineNo existant une seule fois
            int currentMaxLineNo = await GetLastLineNoAsync(firstDocNo);

            foreach (var line in lines)
            {
                // SÉCURITÉ : forcer le projet depuis le JWT
                line.JobNo = projectNo;

                // Attribution du LineNo en incrémentant de 10 000 (convention BC standard)
                currentMaxLineNo += 10000;
                line.LineNo = currentMaxLineNo;

                _logger.LogInformation("[PR] Création ligne {DocNo} / LineNo {LineNo}",
                    line.DocumentNo, line.LineNo);

                var json = JsonSerializer.Serialize(line, _serializerOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("purchaseRequestLines", content);

                if (!response.IsSuccessStatusCode)
                    await HandleErrorResponseAsync(response);
            }

            return true;
        }

        public async Task<bool> PatchHeaderAsync(Guid id, PurchaseRequestDto header, string projectNo)
        {
            // SÉCURITÉ : vérifier appartenance + récupérer ETag
            var (_, etag) = await GetAndVerifyHeaderAsync(id, projectNo);

            if (!string.IsNullOrEmpty(header.JobNo) && !string.Equals(header.JobNo, projectNo, StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException("La modification du numéro de projet est interdite.");
            }

            // On ne permet pas de changer le jobNo via un PATCH standard
            header.JobNo = null;

            var json = JsonSerializer.Serialize(header, _serializerOptions);
            _logger.LogInformation("[PR] PATCH en-tête {Id} — Body: {Json}", id, json);

            var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"purchaseRequests({id})")
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            request.Headers.TryAddWithoutValidation("If-Match", etag ?? "*");

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                await HandleErrorResponseAsync(response);

            _logger.LogInformation("[PR] PATCH en-tête {Id} — Réponse: {Status}",
                id, (int)response.StatusCode);

            return true;
        }

        public async Task<bool> SubmitForApprovalAsync(Guid id, string projectNo)
        {
            // SÉCURITÉ : vérifier appartenance + récupérer ETag + validation métier
            var (existing, etag) = await GetAndVerifyHeaderAsync(id, projectNo);

            // Vérification du statut — null-safe
            var currentStatut = existing.Statut ?? string.Empty;
            if (!string.Equals(currentStatut, "Open", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException(
                    $"Statut actuel '{currentStatut}' — seule une demande 'Open' peut être soumise.");

            var json = """{"statut": "To Approve"}""";
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"purchaseRequests({id})")
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            request.Headers.TryAddWithoutValidation("If-Match", etag ?? "*");

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                await HandleErrorResponseAsync(response);

            _logger.LogInformation("[PR] Demande {Id} soumise pour approbation.", id);
            return true;
        }

        public async Task<bool> DeleteRequestAsync(Guid id, string projectNo)
        {
            // SÉCURITÉ : vérifier appartenance avant suppression
            await GetAndVerifyHeaderAsync(id, projectNo);

            var response = await _httpClient.DeleteAsync($"purchaseRequests({id})");

            if (!response.IsSuccessStatusCode)
                await HandleErrorResponseAsync(response);

            _logger.LogInformation("[PR] Demande {Id} supprimée.", id);
            return true;
        }

        // ── LIGNES ────────────────────────────────────────────────────────────

        public async Task<bool> PatchLineAsync(Guid lineId, PurchaseRequestLineDto lineDto, string projectNo)
        {
            // SÉCURITÉ : vérifier appartenance + récupérer ETag
            var (_, etag) = await GetAndVerifyLineAsync(lineId, projectNo);

            // Forcer le jobNo pour éviter toute manipulation côté client
            lineDto.JobNo = projectNo;

            var json = JsonSerializer.Serialize(lineDto, _serializerOptions);
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"purchaseRequestLines({lineId})")
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            request.Headers.TryAddWithoutValidation("If-Match", etag ?? "*");

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                await HandleErrorResponseAsync(response);

            return true;
        }

        public async Task<bool> DeleteLineAsync(Guid lineId, string projectNo)
        {
            // SÉCURITÉ : vérifier appartenance avant suppression
            await GetAndVerifyLineAsync(lineId, projectNo);

            var response = await _httpClient.DeleteAsync($"purchaseRequestLines({lineId})");

            if (!response.IsSuccessStatusCode)
                await HandleErrorResponseAsync(response);

            _logger.LogInformation("[PR] Ligne {LineId} supprimée.", lineId);
            return true;
        }

        // ── UTILITAIRES PRIVÉS ────────────────────────────────────────────────

        /// <summary>
        /// Retourne le dernier numéro de ligne existant pour un document donné.
        /// Retourne 0 si le document n'a aucune ligne (première ligne = 10 000).
        /// </summary>
        private async Task<int> GetLastLineNoAsync(string documentNo)
        {
            var url = $"purchaseRequestLines?$filter=documentNo eq '{documentNo}'" +
                      "&$orderby=lineNo desc&$top=1";

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return 0; // En cas d'erreur, on commence à 0 — BC refusera les doublons éventuels

            var content = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement.GetProperty("value");

            if (root.GetArrayLength() > 0)
                return root[0].GetProperty("lineNo").GetInt32();

            return 0;
        }
    }
}