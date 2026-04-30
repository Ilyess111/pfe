using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Soroubat.Api.Interfaces;
using Soroubat.Api.Models;

namespace Soroubat.Api.Controllers
{
    /// <summary>
    /// Gestion du chantier : projet assigné et tâches projet.
    /// Toutes les routes sont scopées au projet du chef de chantier extrait du JWT.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SiteManagementController : ControllerBase
    {
        private readonly ISiteManagementService _siteManagementService;

        // Numéro de projet extrait du claim JWT — injecté à chaque requête
        private string UserProjectNo => User.FindFirst("projectNo")?.Value ?? string.Empty;

        public SiteManagementController(ISiteManagementService siteManagementService)
        {
            _siteManagementService = siteManagementService;
        }

        // ─── PROJET ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Retourne le projet BC assigné au chef de chantier connecté.
        /// </summary>
        [HttpGet("my-project")]
        [ProducesResponseType(typeof(JobDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<JobDto>> GetMyProject()
        {
            if (string.IsNullOrEmpty(UserProjectNo))
                return BadRequest("Aucun projet assigné dans votre profil.");

            try
            {
                var job = await _siteManagementService.GetAssignedJobAsync(UserProjectNo);
                return Ok(job);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        // ─── TÂCHES ───────────────────────────────────────────────────────────────

        /// <summary>
        /// Retourne la liste des tâches du projet du chef de chantier connecté.
        /// </summary>
        [HttpGet("my-tasks")]
        [ProducesResponseType(typeof(IEnumerable<JobTaskDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<JobTaskDto>>> GetMyTasks()
        {
            if (string.IsNullOrEmpty(UserProjectNo))
                return BadRequest("Aucun projet assigné dans votre profil.");

            try
            {
                var tasks = await _siteManagementService.GetTasksByProjectAsync(UserProjectNo);
                return Ok(tasks);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Met à jour le pourcentage d'avancement d'une tâche du projet du chef de chantier.
        /// </summary>
        [HttpPatch("tasks/{id:guid}/progress")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdateTaskProgress(Guid id, [FromBody] UpdateProgressRequest request)
        {
            if (string.IsNullOrEmpty(UserProjectNo))
                return Unauthorized(new { message = "Token invalide : aucun projet associé." });

            try
            {
                var success = await _siteManagementService.UpdateTaskProgressAsync(id, request.ProgressPct, UserProjectNo);

                if (success)
                    return Ok(new { message = "Avancement mis à jour avec succès." });

                return BadRequest(new { message = "Échec de la mise à jour dans Business Central." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }
    }

    /// <summary>
    /// Corps de la requête PATCH pour la mise à jour de l'avancement d'une tâche.
    /// </summary>
    public class UpdateProgressRequest
    {
        /// <summary>Pourcentage d'avancement entre 0 et 100.</summary>
        public decimal ProgressPct { get; set; }
    }
}