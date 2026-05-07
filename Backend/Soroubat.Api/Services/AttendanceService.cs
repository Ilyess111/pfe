using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Soroubat.Api.Interfaces;
using Soroubat.Api.Models;

namespace Soroubat.Api.Services
{
    public class AttendanceService : BaseService, IEmpAttendanceService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AttendanceService> _logger;

        private static readonly JsonSerializerOptions _serializerOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public AttendanceService(HttpClient httpClient, ILogger<AttendanceService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        // ── HELPERS PRIVÉS (Inspirés de PurchaseRequest) ─────────────────────

        private async Task<(EmpAttendanceDto Header, string? ETag)> GetAndVerifyHeaderAsync(Guid id, string projectNo)
        {
            var response = await _httpClient.GetAsync($"employeeAttendanceHeaders({id})");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                throw new KeyNotFoundException($"Fiche de pointage '{id}' introuvable.");

            if (!response.IsSuccessStatusCode) await HandleErrorResponseAsync(response);

            var header = await response.Content.ReadFromJsonAsync<EmpAttendanceDto>();
            if (header == null) throw new KeyNotFoundException("Fiche introuvable.");

            if (!string.Equals(header.JobNo, projectNo, StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException("Accès refusé : cette fiche n'appartient pas à votre projet.");

            return (header, response.Headers.ETag?.ToString());
        }

        private async Task<(EmpAttendanceLineDto Line, string? ETag)> GetAndVerifyLineAsync(Guid lineId, string projectNo)
        {
            // 1. Récupérer la ligne
            var response = await _httpClient.GetAsync($"employeeAttendanceLines({lineId})");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                throw new KeyNotFoundException($"Ligne de pointage introuvable.");

            if (!response.IsSuccessStatusCode) await HandleErrorResponseAsync(response);

            var line = await response.Content.ReadFromJsonAsync<EmpAttendanceLineDto>();
            if (line == null) throw new KeyNotFoundException("Ligne introuvable.");

            // 2. Récupérer l'en-tête associé pour vérifier le projet (sécurité)
            // On utilise le DocumentNo de la ligne pour trouver l'en-tête
            var headerResponse = await _httpClient.GetAsync($"employeeAttendanceHeaders?$filter=no eq '{line.DocumentNo}'");
            var headerResult = await headerResponse.Content.ReadFromJsonAsync<BCResponse<EmpAttendanceDto>>();
            var header = headerResult?.Value?.FirstOrDefault();

            if (header == null || !string.Equals(header.JobNo, projectNo, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("[Pointage] Accès refusé à la ligne {Id} : projet invalide.", lineId);
                throw new UnauthorizedAccessException("Accès refusé : cette ligne n'appartient pas à votre projet.");
            }

            return (line, response.Headers.ETag?.ToString());
        }

        // ── EN-TÊTES ──────────────────────────────────────────────────────────

        public async Task<IEnumerable<EmpAttendanceDto>> GetAllHeadersAsync(string projectNo)
        {
            var url = $"employeeAttendanceHeaders?$filter=jobNo eq '{projectNo}'";
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) await HandleErrorResponseAsync(response);

            var result = await response.Content.ReadFromJsonAsync<BCResponse<EmpAttendanceDto>>();
            return result?.Value ?? Enumerable.Empty<EmpAttendanceDto>();
        }

        public async Task<EmpAttendanceDto?> GetHeaderByIdAsync(Guid id, string projectNo)
        {
            // 1. URL propre sans caractères parasites
            var url = $"employeeAttendanceHeaders({id})?$expand=employeeAttendanceLines";
            
            var response = await _httpClient.GetAsync(url);
            
            // 2. Gestion d'erreur inspirée de PurchaseRequest
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound) 
                return null;

            if (!response.IsSuccessStatusCode) 
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogError("BC a renvoyé une erreur 400. Détails : {Error}", errorBody);
                await HandleErrorResponseAsync(response);
            }

            var request = await response.Content.ReadFromJsonAsync<EmpAttendanceDto>();
            
            if (request == null) return null;

            // 3. Log de diagnostic (indispensable car vous recevez (null) actuellement)
            _logger.LogInformation("Vérification Projet - BC: '{BC}', Token: '{Token}'", 
                request.JobNo ?? "NULL", projectNo);

            // 4. Comparaison sécurisée (Trim + Case Insensitive)
            if (!string.Equals(request.JobNo?.Trim(), projectNo?.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException($"Accès refusé : Le projet de la fiche ({request.JobNo}) ne correspond pas à votre projet ({projectNo}).");
            }

            return request;
        }

public async Task<EmpAttendanceDto> CreateHeaderAsync(EmpAttendanceDto dto, string projectNo)
{
    // Construire un objet minimal SANS "no" ni "id" — BC génère le N° automatiquement
    var payload = new CreateEmpAttendanceDto
    {
        JobNo = projectNo,
        Month = dto.Month,
        Year  = dto.Year
    };

    var json = JsonSerializer.Serialize(payload);
    _logger.LogInformation("Payload envoyé à BC: {Json}", json);

    var content = new StringContent(json, Encoding.UTF8, "application/json");
    var response = await _httpClient.PostAsync("employeeAttendanceHeaders", content);

    if (!response.IsSuccessStatusCode)
    {
        var errorDetail = await response.Content.ReadAsStringAsync();
        _logger.LogError("Détails erreur POST BC: {Error}", errorDetail);

        // Détecter le doublon BC et lever une exception métier explicite
        if (errorDetail.Contains("Deja saisie") || errorDetail.Contains("DialogException"))
            throw new InvalidOperationException(errorDetail);

        await HandleErrorResponseAsync(response);
    }

    return (await response.Content.ReadFromJsonAsync<EmpAttendanceDto>())!;
}

        public async Task<bool> PatchHeaderAsync(Guid id, EmpAttendanceDto dto, string projectNo)
        {
            var (_, etag) = await GetAndVerifyHeaderAsync(id, projectNo);
            dto.JobNo = null; // Protection : on ne change pas le projet en patch

            var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"employeeAttendanceHeaders({id})")
            {
                Content = new StringContent(JsonSerializer.Serialize(dto, _serializerOptions), Encoding.UTF8, "application/json")
            };
            request.Headers.TryAddWithoutValidation("If-Match", etag ?? "*");

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode) await HandleErrorResponseAsync(response);
            return true;
        }

        public async Task<bool> DeleteHeaderAsync(Guid id, string projectNo)
        {
            await GetAndVerifyHeaderAsync(id, projectNo);
            var response = await _httpClient.DeleteAsync($"employeeAttendanceHeaders({id})");
            return response.IsSuccessStatusCode;
        }

        // ── LIGNES ────────────────────────────────────────────────────────────

        public async Task<bool> CreateLinesAsync(List<EmpAttendanceLineDto> lines, string projectNo)
        {
            if (lines == null || !lines.Any()) return false;

            foreach (var line in lines)
            {
                // On ne force plus JobNo car il n'existe pas sur la ligne
                var response = await _httpClient.PostAsJsonAsync("employeeAttendanceLines", line, _serializerOptions);
                if (!response.IsSuccessStatusCode) await HandleErrorResponseAsync(response);
            }
            return true;
        }

        public async Task<bool> PatchLineAsync(Guid lineId, EmpAttendanceLineDto lineDto, string projectNo)
        {
            // On récupère l'ETag en utilisant l'ID GUID
            var (_, etag) = await GetAndVerifyLineAsync(lineId, projectNo);

            var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"employeeAttendanceLines({lineId})")
            {
                Content = new StringContent(JsonSerializer.Serialize(lineDto, _serializerOptions), Encoding.UTF8, "application/json")
            };
            
            request.Headers.TryAddWithoutValidation("If-Match", etag ?? "*");
            
            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteLineAsync(Guid lineId, string projectNo)
        {
            await GetAndVerifyLineAsync(lineId, projectNo);
            var response = await _httpClient.DeleteAsync($"employeeAttendanceLines({lineId})");
            return response.IsSuccessStatusCode;
        }
    }
}