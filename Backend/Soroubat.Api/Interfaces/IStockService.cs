using Soroubat.Api.Models;

namespace Soroubat.Api.Interfaces
{
    /// <summary>
    /// Contrat du service de consultation du stock chantier.
    /// Le stock est calculé en agrégeant les écritures comptables articles (Item Ledger Entries)
    /// filtrées par projet. Ce service est en lecture seule.
    /// </summary>
    public interface IStockService
    {
        /// <summary>
        /// Retourne le stock agrégé par article et magasin pour le projet donné.
        /// Les articles avec une quantité nulle sont exclus du résultat.
        /// Retourne une liste vide si aucun mouvement n'est enregistré.
        /// </summary>
        Task<List<StockChantierDto>> GetStockByProjectAsync(string projectNo);
    }
}