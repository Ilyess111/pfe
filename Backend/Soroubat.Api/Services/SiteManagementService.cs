using Soroubat.Api.Interfaces;
using Soroubat.Api.Models;
using System.Text;
using System.Text.Json;
using System.Net.Http.Json;

// le namespace sert à organiser le code et à éviter les conflits de noms entre différentes parties de l'application. Ici, Soroubat.Api.Services indique que ce fichier fait partie des services de l'API Soroubat.
namespace Soroubat.Api.Services
{
public class SiteManagementService : BaseService, ISiteManagementService 
{
    private readonly HttpClient _httpClient;

    public SiteManagementService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // Récupère uniquement le projet spécifique du chef
    public async Task<JobDto> GetAssignedJobAsync(string projectNo)
    {
        // On filtre directement par le No de projet issu du token
        var response = await _httpClient.GetAsync($"jobs?$filter=no eq '{projectNo}'"); 
        
        if (!response.IsSuccessStatusCode) await HandleErrorResponse(response);

        var data = await response.Content.ReadFromJsonAsync<BCResponse<JobDto>>();
        return data?.Value?.FirstOrDefault() ?? throw new Exception("Projet non trouvé ou non assigné.");
    }

        public async Task<List<JobTaskDto>> GetMyTasksAsync(string projectNo)
        {
            // On filtre les tâches directement par le numéro de projet (JobNo)
            // Cela correspond au champ 'jobNo' dans votre API jobTasks
            var response = await _httpClient.GetAsync($"jobTasks?$filter=jobNo eq '{projectNo}'"); 
            
            if (!response.IsSuccessStatusCode) await HandleErrorResponse(response);

            var data = await response.Content.ReadFromJsonAsync<BCResponse<JobTaskDto>>();
            return data?.Value ?? new List<JobTaskDto>();
        }
        public async Task<bool> UpdateTaskProgressAsync(Guid taskId, decimal progress, string authorizedProjectNo)
        {
            // 1. VERIFICATION : On vérifie que la tâche appartient bien au projet du chef
            var taskResponse = await _httpClient.GetAsync($"jobTasks({taskId})");
            if (!taskResponse.IsSuccessStatusCode) return false;

            var task = await taskResponse.Content.ReadFromJsonAsync<JobTaskDto>();
            
            // Si le JobNo de la tâche ne correspond pas au projet du token JWT -> REFUS
            if (task == null || task.JobNo != authorizedProjectNo)
            {
                throw new UnauthorizedAccessException("Vous n'avez pas le droit de modifier une tâche d'un autre chantier.");
            }

            // 2. EXECUTION : Si c'est validé, on fait la mise à jour
            var patchData = new { progressPct = progress };
            var json = JsonSerializer.Serialize(patchData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"jobTasks({taskId})") { Content = content };
            request.Headers.Add("If-Match", "*"); 

            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

    
    }
}