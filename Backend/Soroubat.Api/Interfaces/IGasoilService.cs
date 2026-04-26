using Soroubat.Api.Models;

namespace Soroubat.Api.Interfaces
{
    public interface IGasoilService
    {
        Task<IEnumerable<GasoilHeader>> GetHeadersByJobAsync(string projectNo);
        Task<GasoilHeader?> GetHeaderByIdAsync(Guid id, string projectNo);
        Task<GasoilHeader?> CreateHeaderAsync(GasoilHeader header, string projectNo);
        Task<GasoilHeader?> UpdateHeaderAsync(Guid id, GasoilHeader header, string projectNo);
        Task<bool> DeleteHeaderAsync(Guid id, string projectNo);
        Task<bool> ValiderFicheAsync(Guid id, string projectNo);
        Task<GasoilLine?> CreateLineAsync(GasoilLine line, string projectNo);
        Task<GasoilLine?> UpdateLineAsync(Guid id, GasoilLine line, string projectNo);
        Task<bool> DeleteLineAsync(Guid id, string projectNo);
        
        // pour le service d'alertes uniquement
        Task<IEnumerable<GasoilHeader>> GetHeadersWithLinesAsync(string projectNo);

    }
}