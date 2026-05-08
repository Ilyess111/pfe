using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Soroubat.Api.Interfaces;
using Soroubat.Api.Models;
using System.Security.Claims;

namespace Soroubat.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AttendanceController : ControllerBase
    {
        private readonly IEmpAttendanceService _service;
        private readonly IEmployeeService _employeeService;
        private readonly IChefChantierService _chefChantierService;
        private string? UserProjectNo => User.FindFirst("projectNo")?.Value;

        public AttendanceController(IEmpAttendanceService service, IEmployeeService employeeService, IChefChantierService chefChantierService)
        {
            _service = service;
            _employeeService = employeeService;
            _chefChantierService = chefChantierService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmpAttendanceDto>>> GetAll()
        {
            if (string.IsNullOrEmpty(UserProjectNo)) return BadRequest(new { message = "Projet non assigné." });
            return Ok(await _service.GetAllHeadersAsync(UserProjectNo));
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<EmpAttendanceDto>> GetById(Guid id)
        {
            if (string.IsNullOrEmpty(UserProjectNo)) 
                return BadRequest(new { message = "Projet non assigné." });

            try 
            {
                var result = await _service.GetHeaderByIdAsync(id, UserProjectNo);
                
                if (result == null)
                    return NotFound(new { message = "Fiche de pointage introuvable." });
                    
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex) 
            { 
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message }); 
            }
            catch (Exception ex) 
            {
                return StatusCode(500, new { message = "Une erreur interne est survenue." });
            }
        }
        [HttpPost]
        public async Task<ActionResult<EmpAttendanceDto>> CreateHeader([FromBody] EmpAttendanceDto dto)
        {
            if (string.IsNullOrEmpty(UserProjectNo)) 
                return BadRequest(new { message = "Projet non assigné." });
            try
            {
                var created = await _service.CreateHeaderAsync(dto, UserProjectNo);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("Deja saisie"))
            {
                return Conflict(new { message = $"Un pointage existe déjà pour ce chantier sur cette période." });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Une erreur interne est survenue." });
            }
        }

        [HttpPatch("{id:guid}")]
        public async Task<IActionResult> UpdateHeader(Guid id, [FromBody] EmpAttendanceDto dto)
        {
            if (string.IsNullOrEmpty(UserProjectNo)) return BadRequest(new { message = "Projet non assigné." });
            await _service.PatchHeaderAsync(id, dto, UserProjectNo);
            return Ok(new { message = "En-tête mis à jour." });
        }

        [HttpPost("lines")]
        public async Task<IActionResult> CreateLines([FromBody] List<EmpAttendanceLineDto> lines)
        {
            if (string.IsNullOrEmpty(UserProjectNo)) return BadRequest(new { message = "Projet non assigné." });
            await _service.CreateLinesAsync(lines, UserProjectNo);
            return Ok(new { message = "Lignes créées." });
        }

        [HttpPatch("lines/{id:guid}")]
        public async Task<IActionResult> UpdateLine(Guid id, [FromBody] EmpAttendanceLineDto lineDto)
        {
            if (string.IsNullOrEmpty(UserProjectNo)) return BadRequest(new { message = "Projet non assigné." });
            await _service.PatchLineAsync(id, lineDto, UserProjectNo);
            return Ok(new { message = "Ligne mise à jour." });
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteHeader(Guid id)
        {
            if (string.IsNullOrEmpty(UserProjectNo)) return BadRequest(new { message = "Projet non assigné." });
            await _service.DeleteHeaderAsync(id, UserProjectNo);
            return NoContent();
        }

        [HttpDelete("lines/{id:guid}")]
        public async Task<IActionResult> DeleteLine(Guid id)
        {
            // 1. Vérification de la session utilisateur
            if (string.IsNullOrEmpty(UserProjectNo))
            {
                return BadRequest(new { message = "Projet non assigné." });
            }

            try
            {
                // 2. Appel au service pour suppression sécurisée
                var deleted = await _service.DeleteLineAsync(id, UserProjectNo);
                
                if (!deleted)
                {
                    return BadRequest(new { message = "Impossible de supprimer la ligne." });
                }

                return NoContent(); // Code 204 en cas de succès[cite: 7]
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Ligne de pointage introuvable." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Une erreur interne est survenue." });
            }
        }


        [HttpPost("scan-presence")]
        public async Task<IActionResult> MarkPresenceWithFace([FromBody] FaceAttendanceRequest request)
        {
            // 1. Récupérer le projet via le service (comme dans EmployeeController)
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email)) return Unauthorized("Email manquant");
            
            var numProjet = await _chefChantierService.GetJobNoByEmailAsync(email);

            // 2. Préparer la requête de vérification
            var faceRequest = new FaceVerificationRequest 
            { 
                Matricule = request.Matricule, 
                CapturedImageBase64 = request.CapturedImageBase64 
            };

            // 3. Vérifier avec le BON numProjet
            // var isVerified = await _employeeService.VerifyFaceAsync(faceRequest, numProjet);
            var isVerified = await _employeeService.VerifyFaceAsync(faceRequest, null);

            if (!isVerified)
            {
                return Unauthorized(new { message = "Reconnaissance faciale échouée." });
            }

            // ÉTAPE 2 : Marquage de la présence si identité confirmée
            try
            {
                var success = await _service.MarkPresenceAsync(
                    request.HeaderId, 
                    request.Matricule, 
                    request.Day, 
                    UserProjectNo);

                if (success)
                    return Ok(new { message = $"Présence marquée pour le jour {request.Day}" });
                
                return BadRequest("Erreur lors de la mise à jour du pointage.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}