using Soroubat.Api.Models;
using System.Text.Json;

namespace Soroubat.Api.Interfaces
{
    public interface ITransferService
    {
        Task<IEnumerable<TransferHeaderDto>> GetAllTransfersAsync();
        Task<TransferHeaderDto?> GetTransferByIdAsync(Guid id);        
        Task<bool> UpdateLineAsync(Guid id, JsonElement body);
    }
}