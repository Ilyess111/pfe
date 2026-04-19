using System.Text.Json;
using Soroubat.Api.Models;

namespace Soroubat.Api.Interfaces
{
    public interface IPurchaseRequestService
    {
        // En-tête (Header)
        Task<IEnumerable<PurchaseRequestDto>> GetAllRequestsAsync(string projectNo);
        Task<PurchaseRequestDto> GetRequestByIdAsync(Guid id, string projectNo);        
        // on a divisé la création d'une demande d'achat complète en deux étapes : d'abord on crée le header pour obtenir son ID, puis on crée les lignes en associant l'ID du header. Cela permet de mieux gérer les erreurs et de s'assurer que le header est créé avant de tenter de créer les lignes.
        Task<PurchaseRequestDto> CreateHeaderAsync(PurchaseRequestDto header, string projectNo);
        Task<bool> CreateLinesAsync(List<PurchaseRequestLineDto> lines, string projectNo);
        Task<bool> PatchHeaderAsync(Guid id, PurchaseRequestDto header, string projectNo);
        Task<bool> DeleteRequestAsync(Guid id, string projectNo);
        Task<bool> PatchLineAsync(Guid lineId, PurchaseRequestLineDto lineDto, string projectNo);        
        Task<bool> DeleteLineAsync(Guid lineId, string projectNo);
    }
}