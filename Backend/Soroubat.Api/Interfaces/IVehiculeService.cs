using Soroubat.Api.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Soroubat.Api.Interfaces
{
    public interface IVehiculeService
    {
        // Méthodes pour le Header (En-tête)
        Task<IEnumerable<VehiculePointageHeader>> GetHeadersByJobAsync(string projectNo);
        Task<VehiculePointageHeader> GetHeaderByIdAsync(Guid id, string projectNo);
        Task<VehiculePointageHeader?> CreateHeaderAsync(VehiculePointageHeader header, string projectNo);
        Task<VehiculePointageHeader?> UpdateHeaderAsync(Guid id, VehiculePointageHeader header, string projectNo);
        Task<bool> DeleteHeaderAsync(Guid id, string projectNo);
        // Task<bool> DeleteHeaderAsync(Guid id);


        Task<VehiculePointageLine?> UpdateLineAsync(Guid id, VehiculePointageLine line, string projectNo);
    }
}