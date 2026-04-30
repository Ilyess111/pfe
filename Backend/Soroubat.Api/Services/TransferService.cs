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
    /// Service de gestion des ordres de transfert.
    /// Le filtrage par chantierDestination et la vérification par remontée au header
    /// constituent le double mécanisme de sécurité de cette partie.
    /// </summary>
    public class TransferService : BaseService, ITransferService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<TransferService> _logger;

        private static readonly JsonSerializerOptions _writeOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public TransferService(HttpClient httpClient, ILogger<TransferService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        // ─── HEADERS ──────────────────────────────────────────────────────────────

        public async Task<IEnumerable<TransferHeaderDto>> GetAllTransfersAsync(string projectNo)
        {
            var url = $"transferHeaders?$filter=chantierDestination eq '{ODataEncode(projectNo)}'";
            _logger.LogInformation("[Transfer] GET {Url}", url);

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                await HandleErrorResponse(response);

            var result = await response.Content.ReadFromJsonAsync<BCResponse<TransferHeaderDto>>();
            return result?.Value ?? Enumerable.Empty<TransferHeaderDto>();
        }

        public async Task<TransferHeaderDto?> GetTransferByIdAsync(Guid id, string projectNo)
        {
            var url = $"transferHeaders({id})?$expand=transferLines";
            _logger.LogInformation("[Transfer] GET {Url}", url);

            var response = await _httpClient.GetAsync(url);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            if (!response.IsSuccessStatusCode)
                await HandleErrorResponse(response);

            var transfer = await response.Content.ReadFromJsonAsync<TransferHeaderDto>();

            if (transfer == null)
                return null;

            // Vérification sécurité : le chantier destination doit correspondre au projet du chef
            if (!transfer.ChantierDestination!.Equals(projectNo, StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException(
                    "Accès refusé : cet ordre de transfert n'est pas destiné à votre chantier.");

            return transfer;
        }

        public async Task<IEnumerable<TransferHeaderDto>> GetAllTransfersWithLinesAsync(string projectNo)
        {
            var url = $"transferHeaders?$filter=chantierDestination eq '{ODataEncode(projectNo)}'&$expand=transferLines";
            _logger.LogInformation("[Transfer] GET (with lines) {Url}", url);

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                await HandleErrorResponse(response);

            var result = await response.Content.ReadFromJsonAsync<BCResponse<TransferHeaderDto>>();
            return result?.Value ?? Enumerable.Empty<TransferHeaderDto>();
        }

        // ─── LIGNES ───────────────────────────────────────────────────────────────

        public async Task<bool> UpdateLineAsync(Guid lineId, TransferLineUpdateDto updateDto, string projectNo)
        {
            // 1. Récupération de la ligne et de son ETag
            var (line, etag) = await GetLineAndEtagAsync(lineId);
            if (line == null) return false;

            // 2. Vérification sécurité : remontée au header pour contrôler le chantierDestination
            // La ligne seule ne contient pas le chantier — on remonte au header via documentNo
            await VerifyLineOwnershipAsync(line.DocumentNo!, projectNo);

            // 3. Validation métier : qtyToReceive doit être > 0
            if (updateDto.QtyToReceive <= 0)
                throw new ArgumentOutOfRangeException(nameof(updateDto.QtyToReceive),
                    "La quantité à réceptionner doit être supérieure à 0.");

            // 4. PATCH uniquement sur qtyToReceive — aucun autre champ ne peut être modifié
            var json = JsonSerializer.Serialize(updateDto, _writeOptions);
            _logger.LogInformation("[Transfer] PATCH transferLines({LineId}) — qtyToReceive: {Qty}",
                lineId, updateDto.QtyToReceive);

            var request = BuildPatchRequest($"transferLines({lineId})", json, etag);
            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                await HandleErrorResponse(response);

            return response.IsSuccessStatusCode;
        }

        // ─── HELPERS PRIVÉS ───────────────────────────────────────────────────────

        /// <summary>
        /// Récupère une ligne de transfert et son ETag.
        /// Retourne (null, null) si introuvable (404).
        /// </summary>
        private async Task<(TransferLineDto? dto, string? etag)> GetLineAndEtagAsync(Guid lineId)
        {
            var response = await _httpClient.GetAsync($"transferLines({lineId})");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return (null, null);

            if (!response.IsSuccessStatusCode)
                await HandleErrorResponse(response);

            var dto  = await response.Content.ReadFromJsonAsync<TransferLineDto>();
            var etag = response.Headers.ETag?.ToString();

            return (dto, etag);
        }

        /// <summary>
        /// Remonte au header d'un transfert via son numéro de document et vérifie
        /// que le chantierDestination correspond au projet du chef connecté.
        /// Lève UnauthorizedAccessException si le projet ne correspond pas.
        /// Lève KeyNotFoundException si le header est introuvable.
        /// </summary>
        private async Task VerifyLineOwnershipAsync(string documentNo, string projectNo)
        {
            var url = $"transferHeaders?$filter=no eq '{ODataEncode(documentNo)}'";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                await HandleErrorResponse(response);

            var result = await response.Content.ReadFromJsonAsync<BCResponse<TransferHeaderDto>>();
            var header = result?.Value?.FirstOrDefault();

            if (header == null)
                throw new KeyNotFoundException($"En-tête de transfert '{documentNo}' introuvable.");

            if (!header.ChantierDestination!.Equals(projectNo, StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException(
                    "Accès refusé : cette ligne appartient à un transfert non destiné à votre chantier.");
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