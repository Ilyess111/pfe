using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Soroubat.Api.Interfaces;
using Soroubat.Api.Models;

namespace Soroubat.Api.Services
{
    /// <summary>
    /// Service de gestion de chantier.
    /// Toutes les opérations sont filtrées et vérifiées par rapport au projectNo du JWT.
    /// </summary>
    public class SiteManagementService : BaseService, ISiteManagementService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<SiteManagementService> _logger;

        public SiteManagementService(HttpClient httpClient, ILogger<SiteManagementService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }


        public async Task<JobDto> GetAssignedJobAsync(string projectNo)
        {
            var url = $"jobs?$filter=no eq '{ODataEncode(projectNo)}'";
            _logger.LogInformation("[SiteManagement] GET {Url}", url);

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                await HandleErrorResponse(response);

            var data = await response.Content.ReadFromJsonAsync<BCResponse<JobDto>>();
            return data?.Value?.FirstOrDefault()
                ?? throw new KeyNotFoundException($"Projet '{projectNo}' introuvable dans Business Central.");
        }


        public async Task<List<JobTaskDto>> GetTasksByProjectAsync(string projectNo)
        {
            var url = $"jobTasks?$filter=jobNo eq '{ODataEncode(projectNo)}'";
            _logger.LogInformation("[SiteManagement] GET {Url}", url);

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                await HandleErrorResponse(response);

            var data = await response.Content.ReadFromJsonAsync<BCResponse<JobTaskDto>>();
            return data?.Value ?? new List<JobTaskDto>();
        }

        public async Task<bool> UpdateTaskProgressAsync(Guid taskId, decimal progressPct, string projectNo)
        {
            // 1. Vérification : la tâche doit appartenir au projet du chef connecté
            _logger.LogInformation("[SiteManagement] Vérification appartenance tâche {TaskId} → projet {ProjectNo}", taskId, projectNo);

            var getResponse = await _httpClient.GetAsync($"jobTasks({taskId})");
            if (!getResponse.IsSuccessStatusCode)
                return false;

            var task = await getResponse.Content.ReadFromJsonAsync<JobTaskDto>();

            if (task == null || !task.JobNo.Equals(projectNo, StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException(
                    "Modification refusée : cette tâche n'appartient pas à votre projet.");

            // 2. Validation métier : la progression doit être entre 0 et 100
            if (progressPct < 0 || progressPct > 100)
                throw new ArgumentOutOfRangeException(nameof(progressPct),
                    "Le pourcentage d'avancement doit être compris entre 0 et 100.");

            // 3. Le patch
            var patchBody = new { progressPct };
            var json = JsonSerializer.Serialize(patchBody); // serialize sert à convertir un objet C# en une chaîne JSON
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"jobTasks({taskId})")
            {
                Content = content
            };
            request.Headers.TryAddWithoutValidation("If-Match", "*"); // ignore les problèmes d'ETag — on force la mise à jour même si la ressource a été modifiée depuis la dernière lecture

            _logger.LogInformation("[SiteManagement] PATCH jobTasks({TaskId}) — progressPct: {Progress}", taskId, progressPct);

            var patchResponse = await _httpClient.SendAsync(request);

            if (!patchResponse.IsSuccessStatusCode)
                await HandleErrorResponse(patchResponse);

            return patchResponse.IsSuccessStatusCode;
        }



    }
}