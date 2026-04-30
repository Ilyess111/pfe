using Soroubat.Api.Models;

namespace Soroubat.Api.Interfaces
{
    /// <summary>
    /// Contrat du service de gestion de chantier.
    /// Toutes les opérations sont scopées au projet du chef de chantier connecté.
    /// </summary>
    public interface ISiteManagementService
    {
        /// <summary>
        /// Récupère le projet BC assigné au chef de chantier.
        /// </summary>
        Task<JobDto> GetAssignedJobAsync(string projectNo);

        /// <summary>
        /// Récupère la liste des tâches du projet du chef de chantier.
        /// </summary>
        Task<List<JobTaskDto>> GetTasksByProjectAsync(string projectNo);

        /// <summary>
        /// Met à jour le pourcentage d'avancement d'une tâche.
        /// Lève UnauthorizedAccessException si la tâche n'appartient pas au projet autorisé.
        /// </summary>
        Task<bool> UpdateTaskProgressAsync(Guid taskId, decimal progressPct, string projectNo);
    }
}