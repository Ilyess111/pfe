using Soroubat.Api.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Soroubat.Api.Models;

namespace Soroubat.Api.Controllers
{
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SiteManagementController : ControllerBase
{
    private readonly ISiteManagementService _siteService;

    public SiteManagementController(ISiteManagementService service) 
    {
        _siteService = service;
    }

    // Propriété privée pour extraire le numéro de projet du JWT
    private string UserProjectNo => User.FindFirst("projectNo")?.Value;

    [HttpGet("my-project")]
    public async Task<ActionResult<JobDto>> GetMyProject()
    {
        var projectNo = UserProjectNo;
        if (string.IsNullOrEmpty(projectNo)) return BadRequest("Aucun projet assigné dans votre profil.");

        try 
        {
            var job = await _siteService.GetAssignedJobAsync(projectNo);
            return Ok(job);
        }
        catch (Exception ex) { return StatusCode(500, ex.Message); }
    }

    [HttpGet("my-tasks")]
    public async Task<ActionResult<IEnumerable<JobTaskDto>>> GetMyTasks()
    {
        var projectNo = UserProjectNo;
        if (string.IsNullOrEmpty(projectNo)) return BadRequest("Accès refusé : Aucun projet assigné.");

        try 
        {
            // Le backend décide lui-même quel projet charger
            var tasks = await _siteService.GetMyTasksAsync(projectNo);
            return Ok(tasks);
        }
        catch (Exception ex) { return StatusCode(500, ex.Message); }
    }



        [HttpPatch("update-progress")]
        public async Task<IActionResult> UpdateProgress([FromBody] UpdateProgressRequest request) 
        {
            try 
            {
                var projectNo = User.FindFirst("projectNo")?.Value;
                if (string.IsNullOrEmpty(projectNo)) return Unauthorized();

                var success = await _siteService.UpdateTaskProgressAsync(request.Id, request.Progress, projectNo);
                
                if (success) return Ok(new { message = "Avancement mis à jour avec succès" });
                return BadRequest("Erreur lors de la mise à jour dans Business Central");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message); // Retourne une erreur 403
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    public class UpdateProgressRequest // c'est la classe qui génére le formulaire de données attendu dans le corps de la requête PATCH pour mettre à jour le progrès d'une tâche
    {
        public Guid Id { get; set; }
        public decimal Progress { get; set; }
    }
}