using Soroubat.Api.Models;

namespace Soroubat.Api.Interfaces
{
    /// <summary>
    /// Contrat du service de gestion des demandes d'achat.
    /// La création est divisée en deux étapes (header puis lignes) pour garantir
    /// la cohérence des données et permettre une gestion d'erreur granulaire.
    /// Toutes les opérations sont scopées au projet du chef de chantier connecté.
    /// </summary>
    public interface IPurchaseRequestService
    {
        // ─── EN-TÊTES ─────────────────────────────────────────────────────────────

        /// <summary>Retourne toutes les demandes d'achat du projet.</summary>
        Task<IEnumerable<PurchaseRequestDto>> GetAllRequestsAsync(string projectNo);

        /// <summary>
        /// Retourne une demande d'achat avec ses lignes ($expand).
        /// Lève UnauthorizedAccessException si la demande n'appartient pas au projet.
        /// </summary>
        Task<PurchaseRequestDto?> GetRequestByIdAsync(Guid id, string projectNo);

        /// <summary>
        /// Crée l'en-tête d'une demande d'achat.
        /// Le jobNo est toujours forcé depuis le projectNo du JWT.
        /// </summary>
        Task<PurchaseRequestDto> CreateHeaderAsync(PurchaseRequestDto header, string projectNo);

        /// <summary>
        /// Met à jour les champs modifiables de l'en-tête.
        /// Lève UnauthorizedAccessException si la demande n'appartient pas au projet.
        /// </summary>
        Task<bool> UpdateHeaderAsync(Guid id, PurchaseRequestDto header, string projectNo);

        /// <summary>
        /// Soumet la demande pour approbation (Open → To Approve).
        /// Lève InvalidOperationException si le statut actuel n'est pas 'Open'.
        /// Lève UnauthorizedAccessException si la demande n'appartient pas au projet.
        /// </summary>
        Task<bool> SubmitForApprovalAsync(Guid id, string projectNo);

        /// <summary>
        /// Supprime une demande d'achat.
        /// Lève UnauthorizedAccessException si la demande n'appartient pas au projet.
        /// </summary>
        Task<bool> DeleteRequestAsync(Guid id, string projectNo);

        // ─── LIGNES ───────────────────────────────────────────────────────────────

        /// <summary>
        /// Crée toutes les lignes d'un document en une seule opération séquentielle.
        /// Les numéros de ligne sont calculés automatiquement (dernier + 10000).
        /// Lève UnauthorizedAccessException si le document n'appartient pas au projet.
        /// </summary>
        Task<bool> CreateLinesAsync(List<PurchaseRequestLineDto> lines, string projectNo);

        /// <summary>
        /// Met à jour une ligne.
        /// Lève UnauthorizedAccessException si la ligne n'appartient pas au projet.
        /// </summary>
        Task<bool> UpdateLineAsync(Guid lineId, PurchaseRequestLineDto line, string projectNo);

        /// <summary>
        /// Supprime une ligne.
        /// Lève UnauthorizedAccessException si la ligne n'appartient pas au projet.
        /// </summary>
        Task<bool> DeleteLineAsync(Guid lineId, string projectNo);
    }
}