using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Soroubat.Api.Interfaces;
using Soroubat.Api.Models;
using Microsoft.AspNetCore.Authorization;


namespace Soroubat.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PurchaseRequestController : ControllerBase
    {
        private readonly IPurchaseRequestService _service;

        public PurchaseRequestController(IPurchaseRequestService service)
        {
            _service = service;
        }

        // Propriété d'aide pour extraire le projet du JWT
        private string UserProjectNo => User.FindFirst("projectNo")?.Value;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PurchaseRequestDto>>> GetAll()
        {
            try 
            {
                var projectNo = UserProjectNo;
                
                // Sécurité : si le token n'a pas de projet, on interdit l'accès
                if (string.IsNullOrEmpty(projectNo)) 
                    return BadRequest("Aucun projet n'est assigné à votre compte.");

                // On appelle le service avec le filtre automatique
                var requests = await _service.GetAllRequestsAsync(projectNo);
                return Ok(requests);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PurchaseRequestDto>> GetById(Guid id)
        {
            try 
            {
                var projectNo = UserProjectNo; // Extrait du JWT
                if (string.IsNullOrEmpty(projectNo)) return Unauthorized();

                // On passe l'ID et le projet autorisé au service
                var request = await _service.GetRequestByIdAsync(id, projectNo);
                
                if (request == null) return NotFound(new { message = "Demande introuvable." });
                
                return Ok(request);
            }
            catch (UnauthorizedAccessException ex)
            {
                // Retourne 403 si le chef de chantier essaie de voir un autre projet
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }


        [HttpPost]
        public async Task<ActionResult<PurchaseRequestDto>> CreateHeader([FromBody] PurchaseRequestDto requestDto)
        {
            try 
            {
                // On récupère le matricule du projet depuis le jeton JWT
                var projectNo = UserProjectNo;
                if (string.IsNullOrEmpty(projectNo)) 
                    return Unauthorized("Aucun projet n'est associé à votre compte.");

                // On appelle le service en lui passant le DTO et le projet forcé
                var createdHeader = await _service.CreateHeaderAsync(requestDto, projectNo);
                
                if (createdHeader == null) 
                    return BadRequest("Échec de la création de l'en-tête.");

                return CreatedAtAction(nameof(GetById), new { id = createdHeader.Id }, createdHeader);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpPost("lines")]
        public async Task<IActionResult> CreateLines([FromBody] List<PurchaseRequestLineDto> lines)
        {
            try 
            {
                var projectNo = UserProjectNo;
                if (string.IsNullOrEmpty(projectNo)) return Unauthorized();

                if (lines == null || !lines.Any()) return BadRequest("La liste des lignes est vide.");

                var success = await _service.CreateLinesAsync(lines, projectNo);
                
                if (success) return Ok(new { message = "Lignes créées avec succès." });
                
                return BadRequest("Erreur lors de la création de certaines lignes.");
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateHeader(Guid id, [FromBody] PurchaseRequestDto headerDto)
        {
            try 
            {
                var projectNo = UserProjectNo; // Extrait du JWT
                if (string.IsNullOrEmpty(projectNo)) return Unauthorized();

                var success = await _service.PatchHeaderAsync(id, headerDto, projectNo);
                
                if (success) return Ok(new { message = "En-tête mis à jour avec succès." });
                
                return NotFound("Demande d'achat introuvable.");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHeader(Guid id)
        {
            try 
            {
                var projectNo = UserProjectNo;
                if (string.IsNullOrEmpty(projectNo)) return Unauthorized();

                var success = await _service.DeleteRequestAsync(id, projectNo);
                
                if (success) return NoContent(); // Succès 204 (Pas de contenu)
                
                return BadRequest("Impossible de supprimer la demande.");
            }
            catch (UnauthorizedAccessException ex)
            {
                // Retourne 403 Forbidden si le chef tente de supprimer le projet d'un autre
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpPatch("lines/{id}")]
        public async Task<IActionResult> UpdateLine(Guid id, [FromBody] PurchaseRequestLineDto lineDto)
        {
            try 
            {
                var projectNo = UserProjectNo;
                if (string.IsNullOrEmpty(projectNo)) return Unauthorized();

                var success = await _service.PatchLineAsync(id, lineDto, projectNo);
                
                if (success) return Ok(new { message = "Ligne mise à jour avec succès." });
                
                return NotFound("Ligne introuvable.");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("lines/{id}")]
        public async Task<IActionResult> DeleteLine(Guid id)
        {
            try 
            {
                var projectNo = UserProjectNo; // Propriété privée qui lit le Claim "projectNo"
                if (string.IsNullOrEmpty(projectNo)) return Unauthorized();

                var success = await _service.DeleteLineAsync(id, projectNo);
                
                if (success) return NoContent(); // 204
                
                return BadRequest("Échec de la suppression de la ligne.");
            }
            catch (UnauthorizedAccessException ex)
            {
                // Si le chef de chantier essaie de supprimer la ligne d'un autre projet
                return StatusCode(403, new { message = ex.Message });            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}