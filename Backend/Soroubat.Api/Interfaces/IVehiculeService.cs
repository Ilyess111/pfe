using Soroubat.Api.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Soroubat.Api.Interfaces
{
    public interface IVehiculeService
    {
        // Méthodes pour le Header (En-tête)
        Task<IEnumerable<VehiculePointageHeader>> GetHeadersByJobAsync(string jobNo);
        Task<VehiculePointageHeader> GetHeaderByIdAsync(Guid id); 
        Task<VehiculePointageHeader> CreateHeaderAsync(VehiculePointageHeader header);
        Task<VehiculePointageHeader> UpdateHeaderAsync(Guid id, VehiculePointageHeader header);
        Task<bool> DeleteHeaderAsync(Guid id);

        // Méthodes pour les Lines (Lignes)
        Task<VehiculePointageLine> AddLineAsync(VehiculePointageLine line);
        Task<VehiculePointageLine> UpdateLineAsync(Guid id, VehiculePointageLine line);
        Task<bool> DeleteLineAsync(Guid id);
    }
}