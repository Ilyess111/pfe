using Microsoft.AspNetCore.Mvc;
using Soroubat.Api.Interfaces;
using Soroubat.Api.Models;
using System.Security.Claims;

namespace Soroubat.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;
        private readonly IChefChantierService _chefChantierService;

        public EmployeeController(IEmployeeService employeeService, IChefChantierService chefChantierService)
        {
            _employeeService = employeeService;
            _chefChantierService = chefChantierService;
        }

[HttpGet]
public async Task<IActionResult> GetAllEmployees(
    [FromQuery] string? filter = null, 
    [FromQuery] bool useProjectFilter = true, 
    [FromQuery] int top = 10) // Tout doit être dans ces parenthèses
{
    try
    {
        string? numProjet = null;
        if (useProjectFilter)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            // Correction CS8604 : On vérifie si l'email n'est pas null avant l'appel
            if (string.IsNullOrEmpty(email)) return Unauthorized("Email manquant");
            
            numProjet = await _chefChantierService.GetJobNoByEmailAsync(email);
        }

        // Ajoutez l'argument 'top' ici pour correspondre à la signature de l'interface
        var data = await _employeeService.GetEmployeesAsync(numProjet, filter, top);
        return Ok(data);
    }
    catch (Exception ex)
    {
        return BadRequest(new { message = ex.Message });
    }
}

        [HttpPost("scan")]
        public async Task<IActionResult> ScanFace([FromBody] FaceVerificationRequest request)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var numProjet = await _chefChantierService.GetJobNoByEmailAsync(email);

            // var isVerified = await _employeeService.VerifyFaceAsync(request, numProjet);
            var isVerified = await _employeeService.VerifyFaceAsync(request, null);

            if (isVerified)
                return Ok(new { status = "Success", message = "Salarié identifié" });

            return Unauthorized(new { status = "Failed", message = "Échec de reconnaissance" });
        }
    }
}