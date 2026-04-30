using Soroubat.Api.Models;

namespace Soroubat.Api.Interfaces
{
    /// <summary>
    /// Contrat du service de gestion des ordres de transfert.
    /// Les transferts sont créés dans BC par le magasinier — le chef de chantier
    /// consulte les transferts à destination de son chantier et saisit les réceptions.
    /// Toutes les opérations sont scopées au chantierDestination du chef connecté.
    /// </summary>
    public interface ITransferService
    {
        /// <summary>
        /// Retourne tous les ordres de transfert dont le chantier destination
        /// correspond au projet du chef de chantier connecté.
        /// </summary>
        Task<IEnumerable<TransferHeaderDto>> GetAllTransfersAsync(string projectNo);

        /// <summary>
        /// Retourne un ordre de transfert avec ses lignes ($expand).
        /// Retourne null si introuvable ou si le chantier destination ne correspond pas.
        /// Lève UnauthorizedAccessException si l'accès est refusé.
        /// </summary>
        Task<TransferHeaderDto?> GetTransferByIdAsync(Guid id, string projectNo);

        /// <summary>
        /// Met à jour la quantité à réceptionner (qtyToReceive) d'une ligne de transfert.
        /// Vérifie que la ligne appartient bien à un transfert destiné au projet du chef.
        /// Lève UnauthorizedAccessException si la ligne n'appartient pas au projet.
        /// </summary>
        Task<bool> UpdateLineAsync(Guid lineId, TransferLineUpdateDto updateDto, string projectNo);

        /// <summary>
        /// Retourne tous les ordres de transfert avec leurs lignes ($expand).
        /// Utilisé exclusivement par AlertService — non exposé dans le contrôleur.
        /// </summary>
        Task<IEnumerable<TransferHeaderDto>> GetAllTransfersWithLinesAsync(string projectNo);
    }
}