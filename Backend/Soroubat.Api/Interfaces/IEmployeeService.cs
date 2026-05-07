using Soroubat.Api.Models;
using System.Text.Json;

namespace Soroubat.Api.Interfaces
{
    public interface IEmployeeService
    {
        // Récupère les salariés (filtrés par projet ou non)
        // Task<JsonElement> GetEmployeesAsync(string? numProjet = null, string? filter = null);
        Task<JsonElement> GetEmployeesAsync(string? numProjet = null, string? filter = null, int? top = null);
        
        // Logique de comparaison faciale
        Task<bool> VerifyFaceAsync(FaceVerificationRequest request, string? numProjet);
    }
}