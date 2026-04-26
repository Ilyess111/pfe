using Soroubat.Api.Models;

namespace Soroubat.Api.Interfaces
{

    public interface IAlertService
    {

        Task<List<AlertDto>> GetSiteManagementAlertsAsync(string projectNo);
        Task<List<AlertDto>> GetPurchaseRequestAlertsAsync(string projectNo);
        Task<List<AlertDto>> GetTransferAlertsAsync(string projectNo);
        Task<List<AlertDto>> GetStockAlertsAsync(string projectNo);
        Task<List<AlertDto>> GetVehiculeAlertsAsync(string projectNo);
        Task<List<AlertDto>> GetGasoilAlertsAsync(string projectNo);

    }
}