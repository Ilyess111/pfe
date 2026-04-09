using System.Text.Json;
using Soroubat.Api.Models;

namespace Soroubat.Api.Interfaces
{
    public interface IPurchaseRequestService
    {
        // En-tête (Header)
        Task<IEnumerable<PurchaseRequestDto>> GetAllRequestsAsync();
        Task<PurchaseRequestDto> GetRequestByIdAsync(Guid id);
        // on a divisé la création d'une demande d'achat complète en deux étapes : d'abord on crée le header pour obtenir son ID, puis on crée les lignes en associant l'ID du header. Cela permet de mieux gérer les erreurs et de s'assurer que le header est créé avant de tenter de créer les lignes.
        Task<PurchaseRequestDto> CreateHeaderAsync(PurchaseRequestDto header);
        Task<bool> CreateLinesAsync(List<PurchaseRequestLineDto> lines);
        Task<bool> UpdateHeaderAsync(Guid id, object partialUpdate); 
        Task<bool> DeleteRequestAsync(Guid id);

        // Lignes (Lines)
        Task<bool> UpdateLineAsync(Guid lineId, object partialUpdate);
        Task<bool> DeleteLineAsync(Guid lineId); 

    }
}