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
    /// Service de gestion des demandes d'achat.
    /// Toutes les opérations vérifient l'appartenance de la ressource au projet du chef connecté.
    /// </summary>
    public class PurchaseRequestService : BaseService, IPurchaseRequestService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PurchaseRequestService> _logger;

        private static readonly JsonSerializerOptions _writeOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public PurchaseRequestService(HttpClient httpClient, ILogger<PurchaseRequestService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        // ─── EN-TÊTES ─────────────────────────────────────────────────────────────

        public async Task<IEnumerable<PurchaseRequestDto>> GetAllRequestsAsync(string projectNo)
        {
            var url = $"purchaseRequests?$filter=jobNo eq '{ODataEncode(projectNo)}'";
            _logger.LogInformation("[PurchaseRequest] GET {Url}", url);

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                await HandleErrorResponse(response);

            var result = await response.Content.ReadFromJsonAsync<BCResponse<PurchaseRequestDto>>();
            return result?.Value ?? new List<PurchaseRequestDto>();
        }

        public async Task<PurchaseRequestDto?> GetRequestByIdAsync(Guid id, string projectNo)
        {
            var url = $"purchaseRequests({id})?$expand=purchaseRequestLines";
            _logger.LogInformation("[PurchaseRequest] GET {Url}", url);

            var response = await _httpClient.GetAsync(url);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            if (!response.IsSuccessStatusCode)
                await HandleErrorResponse(response);

            var request = await response.Content.ReadFromJsonAsync<PurchaseRequestDto>();

            if (request != null && !request.JobNo!.Equals(projectNo, StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException(
                    "Accès refusé : cette demande n'appartient pas à votre projet.");

            return request;
        }

        public async Task<PurchaseRequestDto> CreateHeaderAsync(PurchaseRequestDto header, string projectNo)
        {
            // Sécurité : le jobNo est toujours forcé depuis le JWT, jamais depuis le body client
            header.JobNo = projectNo;

            var json = JsonSerializer.Serialize(header, _writeOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogInformation("[PurchaseRequest] POST purchaseRequests — Body: {Json}", json);

            var response = await _httpClient.PostAsync("purchaseRequests", content);

            if (!response.IsSuccessStatusCode)
                await HandleErrorResponse(response);

            return await response.Content.ReadFromJsonAsync<PurchaseRequestDto>()
                ?? throw new Exception("Business Central n'a pas retourné la demande créée.");
        }

        public async Task<bool> UpdateHeaderAsync(Guid id, PurchaseRequestDto header, string projectNo)
        {
            // 1. Vérification appartenance + récupération ETag
            var (existing, etag) = await GetRequestAndEtagAsync(id, projectNo);
            if (existing == null) return false;

            // 2. Sécurité : on ne laisse pas modifier jobNo ou statut via ce endpoint
            header.JobNo   = null;
            header.Statut  = null;
            header.No      = null;
            header.Amount  = null;

            var json = JsonSerializer.Serialize(header, _writeOptions);
            _logger.LogInformation("[PurchaseRequest] PATCH purchaseRequests({Id}) — Body: {Json}", id, json);

            var request = BuildPatchRequest($"purchaseRequests({id})", json, etag);
            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                await HandleErrorResponse(response);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> SubmitForApprovalAsync(Guid id, string projectNo)
        {
            // 1. Vérification appartenance + statut actuel
            var (existing, etag) = await GetRequestAndEtagAsync(id, projectNo);
            if (existing == null) return false;

            if (!existing.Statut!.Equals("Open", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException(
                    $"Soumission impossible : statut actuel '{existing.Statut}', attendu 'Open'.");

            // 2. PATCH sur le statut uniquement
            var json = """{"statut": "To Approve"}""";
            _logger.LogInformation("[PurchaseRequest] PATCH submit purchaseRequests({Id})", id);

            var request = BuildPatchRequest($"purchaseRequests({id})", json, etag);
            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                await HandleErrorResponse(response);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteRequestAsync(Guid id, string projectNo)
        {
            // Vérification appartenance avant suppression
            var (existing, _) = await GetRequestAndEtagAsync(id, projectNo);
            if (existing == null) return false;

            _logger.LogInformation("[PurchaseRequest] DELETE purchaseRequests({Id})", id);

            var response = await _httpClient.DeleteAsync($"purchaseRequests({id})");

            if (!response.IsSuccessStatusCode)
                await HandleErrorResponse(response);

            return response.IsSuccessStatusCode;
        }

        // ─── LIGNES ───────────────────────────────────────────────────────────────

        public async Task<bool> CreateLinesAsync(List<PurchaseRequestLineDto> lines, string projectNo)
        {
            if (lines == null || !lines.Any()) return false;

            // Vérification : toutes les lignes doivent appartenir au même document
            var documentNo = lines.First().DocumentNo;
            if (lines.Any(l => l.DocumentNo != documentNo))
                throw new ArgumentException("Toutes les lignes doivent appartenir au même document.");

            // Vérification appartenance du document au projet
            await VerifyDocumentOwnershipAsync(documentNo!, projectNo);

            // Calcul du numéro de départ (dernier lineNo connu + incrément)
            int currentLineNo = await GetLastLineNoAsync(documentNo!);

            foreach (var line in lines)
            {
                currentLineNo += 10000;
                line.LineNo    = currentLineNo;
                line.JobNo     = projectNo;

                var json = JsonSerializer.Serialize(line, _writeOptions);
                _logger.LogInformation("[PurchaseRequest] POST purchaseRequestLines — Doc: {Doc}, Ligne: {LineNo}", documentNo, currentLineNo);

                var content  = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("purchaseRequestLines", content);

                if (!response.IsSuccessStatusCode)
                    await HandleErrorResponse(response);
            }

            return true;
        }

        public async Task<bool> UpdateLineAsync(Guid lineId, PurchaseRequestLineDto line, string projectNo)
        {
            // Vérification appartenance + ETag
            var (existingLine, etag) = await GetLineAndEtagAsync(lineId, projectNo);
            if (existingLine == null) return false;

            // Sécurité : champs non modifiables via ce endpoint
            line.JobNo      = null;
            line.DocumentNo = null;
            line.LineNo     = null;
            line.LineAmount = null;

            var json = JsonSerializer.Serialize(line, _writeOptions);
            _logger.LogInformation("[PurchaseRequest] PATCH purchaseRequestLines({Id})", lineId);

            var request  = BuildPatchRequest($"purchaseRequestLines({lineId})", json, etag);
            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                await HandleErrorResponse(response);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteLineAsync(Guid lineId, string projectNo)
        {
            // Vérification appartenance avant suppression
            var (existingLine, _) = await GetLineAndEtagAsync(lineId, projectNo);
            if (existingLine == null) return false;

            _logger.LogInformation("[PurchaseRequest] DELETE purchaseRequestLines({Id})", lineId);

            var response = await _httpClient.DeleteAsync($"purchaseRequestLines({lineId})");

            if (!response.IsSuccessStatusCode)
                await HandleErrorResponse(response);

            return response.IsSuccessStatusCode;
        }

        // ─── HELPERS PRIVÉS ───────────────────────────────────────────────────────

        /// <summary>
        /// Récupère un en-tête de demande d'achat et son ETag.
        /// Lève UnauthorizedAccessException si le projet ne correspond pas.
        /// Retourne (null, null) si la ressource est introuvable (404).
        /// </summary>
        private async Task<(PurchaseRequestDto? dto, string? etag)> GetRequestAndEtagAsync(Guid id, string projectNo)
        {
            var response = await _httpClient.GetAsync($"purchaseRequests({id})");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return (null, null);

            if (!response.IsSuccessStatusCode)
                await HandleErrorResponse(response);

            var dto  = await response.Content.ReadFromJsonAsync<PurchaseRequestDto>();
            var etag = response.Headers.ETag?.ToString();

            if (dto != null && !dto.JobNo!.Equals(projectNo, StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException(
                    "Accès refusé : cette demande n'appartient pas à votre projet.");

            return (dto, etag);
        }

        /// <summary>
        /// Récupère une ligne et son ETag.
        /// Lève UnauthorizedAccessException si le projet ne correspond pas.
        /// Retourne (null, null) si la ressource est introuvable (404).
        /// </summary>
        private async Task<(PurchaseRequestLineDto? dto, string? etag)> GetLineAndEtagAsync(Guid lineId, string projectNo)
        {
            var response = await _httpClient.GetAsync($"purchaseRequestLines({lineId})");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return (null, null);

            if (!response.IsSuccessStatusCode)
                await HandleErrorResponse(response);

            var dto  = await response.Content.ReadFromJsonAsync<PurchaseRequestLineDto>();
            var etag = response.Headers.ETag?.ToString();

            if (dto != null && !dto.JobNo!.Equals(projectNo, StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException(
                    "Accès refusé : cette ligne n'appartient pas à votre projet.");

            return (dto, etag);
        }

        /// <summary>
        /// Vérifie qu'un document (par son numéro) appartient au projet du chef connecté.
        /// Utilisé lors de la création de lignes pour sécuriser le document cible.
        /// </summary>
        private async Task VerifyDocumentOwnershipAsync(string documentNo, string projectNo)
        {
            var url = $"purchaseRequests?$filter=no eq '{ODataEncode(documentNo)}'";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                await HandleErrorResponse(response);

            var result = await response.Content.ReadFromJsonAsync<BCResponse<PurchaseRequestDto>>();
            var header = result?.Value?.FirstOrDefault();

            if (header == null)
                throw new KeyNotFoundException($"Document '{documentNo}' introuvable.");

            if (!header.JobNo!.Equals(projectNo, StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException(
                    "Accès refusé : ce document n'appartient pas à votre projet.");
        }

        /// <summary>
        /// Retourne le dernier numéro de ligne du document, ou 0 si aucune ligne n'existe.
        /// Utilisé pour calculer le prochain lineNo lors de la création.
        /// </summary>
        private async Task<int> GetLastLineNoAsync(string documentNo)
        {
            var url = $"purchaseRequestLines?$filter=documentNo eq '{ODataEncode(documentNo)}'&$orderby=lineNo desc&$top=1";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return 0;

            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);
            var values = doc.RootElement.GetProperty("value");

            return values.GetArrayLength() > 0
                ? values[0].GetProperty("lineNo").GetInt32()
                : 0;
        }

        /// <summary>
        /// Construit une requête PATCH avec le header If-Match requis par BC.
        /// </summary>
        private static HttpRequestMessage BuildPatchRequest(string url, string json, string? etag)
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