using Soroubat.Api.Models;

namespace Soroubat.Api.Interfaces
{
    public interface ISiteManagementService 
    { 
        // Récupère le projet assigné basé sur le numéro du token
        Task<JobDto> GetAssignedJobAsync(string projectNo); 

        // Récupère les tâches en utilisant directement le numéro de projet
        Task<List<JobTaskDto>> GetMyTasksAsync(string projectNo);
        
        Task<bool> UpdateTaskProgressAsync(Guid taskId, decimal progress, string authorizedProjectNo);    }
}