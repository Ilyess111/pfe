using Soroubat.Api.Models;
using System.Text.Json;

namespace Soroubat.Api.Interfaces
{
    public interface ITransferService
    {
        Task<IEnumerable<TransferHeaderDto>> GetAllTransfersAsync(string projectNo);
        Task<TransferHeaderDto?> GetTransferByIdAsync(Guid id, string projectNo);
        Task<bool> UpdateLineAsync(Guid id, JsonElement body, string projectNo);
        //uniquement pour le service d'alertes
        Task<IEnumerable<TransferHeaderDto>> GetAllTransfersWithLinesAsync(string projectNo);
    }
}