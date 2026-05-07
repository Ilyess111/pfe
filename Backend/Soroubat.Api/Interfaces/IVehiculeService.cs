using Soroubat.Api.Models;

namespace Soroubat.Api.Interfaces
{
    public interface IVehiculeService
    {
        Task<IEnumerable<VehiculePointageHeader>> GetHeadersByJobAsync(string projectNo);
        Task<VehiculePointageHeader> GetHeaderByIdAsync(Guid id, string projectNo);
        Task<VehiculePointageHeader?> CreateHeaderAsync(VehiculePointageHeader header, string projectNo);
        // Task<VehiculePointageHeader?> UpdateHeaderAsync(Guid id, VehiculePointageHeader header, string projectNo);
        Task<bool> DeleteHeaderAsync(Guid id, string projectNo);
        Task<bool> ValiderPointageAsync(Guid id, string projectNo);
        Task<VehiculePointageLine?> UpdateLineAsync(Guid id, VehiculePointageLine line, string projectNo);
        Task<IEnumerable<VehiculePointageHeader>> GetHeadersWithLinesAsync(string projectNo);
    }
}