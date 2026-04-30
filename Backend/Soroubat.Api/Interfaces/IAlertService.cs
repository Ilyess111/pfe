using Soroubat.Api.Models;

namespace Soroubat.Api.Interfaces
{
    /// <summary>
    /// Contrat du service d'alertes intelligentes.
    /// Chaque méthode analyse les données d'un domaine métier et retourne
    /// la liste des anomalies détectées pour le projet donné.
    /// Les méthodes ne lèvent jamais d'exception — elles retournent une liste vide
    /// en cas d'erreur d'accès aux données afin de ne pas bloquer l'agrégateur global.
    /// </summary>
    public interface IAlertService
    {
        /// <summary>Alertes sur les tâches projet (retard, blocage, non démarrage, dépassement budget).</summary>
        Task<List<AlertDto>> GetSiteManagementAlertsAsync(string projectNo);

        /// <summary>Alertes sur les demandes d'achat (rejet, attente approbation, échéance dépassée, demande vide).</summary>
        Task<List<AlertDto>> GetPurchaseRequestAlertsAsync(string projectNo);

        /// <summary>Alertes sur les ordres de transfert (transit bloqué, non expédié, réception partielle, véhicule manquant).</summary>
        Task<List<AlertDto>> GetTransferAlertsAsync(string projectNo);

        /// <summary>Alertes sur le stock chantier (stock négatif, stock critique, stock dormant).</summary>
        Task<List<AlertDto>> GetStockAlertsAsync(string projectNo);

        /// <summary>Alertes sur les pointages véhicules (non validé, surutilisation, index incohérent, panne sans motif, consommation anormale).</summary>
        Task<List<AlertDto>> GetVehiculeAlertsAsync(string projectNo);

        /// <summary>Alertes sur les fiches gasoil (non validée, index incohérent, consommation totale anormale, ligne sans véhicule, quantité anormale).</summary>
        Task<List<AlertDto>> GetGasoilAlertsAsync(string projectNo);
    }
}