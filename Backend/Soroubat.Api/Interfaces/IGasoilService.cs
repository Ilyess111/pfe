using Soroubat.Api.Models;

namespace Soroubat.Api.Interfaces
{
    /// <summary>
    /// Contrat du service de gestion des fiches gasoil.
    /// Toutes les opérations sont scopées au projet du chef de chantier connecté.
    /// La sécurité repose sur jobNo (header) et projectNo (ligne) comme pivots de vérification.
    /// </summary>
    public interface IGasoilService
    {
        // ─── HEADERS ─────────────────────────────────────────────────────────────

        /// <summary>Retourne toutes les fiches gasoil du projet.</summary>
        Task<IEnumerable<GasoilHeader>> GetHeadersByJobAsync(string projectNo);

        /// <summary>
        /// Retourne une fiche gasoil avec ses lignes ($expand).
        /// Retourne null si introuvable (404).
        /// Lève UnauthorizedAccessException si la fiche n'appartient pas au projet.
        /// </summary>
        Task<GasoilHeader?> GetHeaderByIdAsync(Guid id, string projectNo);

        /// <summary>
        /// Crée une fiche gasoil. jobNo est toujours forcé depuis le JWT.
        /// </summary>
        Task<GasoilHeader?> CreateHeaderAsync(GasoilHeader header, string projectNo);

        /// <summary>
        /// Met à jour les champs modifiables d'une fiche gasoil.
        /// Lève UnauthorizedAccessException si la fiche n'appartient pas au projet.
        /// </summary>
        Task<GasoilHeader?> UpdateHeaderAsync(Guid id, GasoilHeader header, string projectNo);

        /// <summary>
        /// Supprime une fiche gasoil.
        /// Lève UnauthorizedAccessException si la fiche n'appartient pas au projet.
        /// </summary>
        Task<bool> DeleteHeaderAsync(Guid id, string projectNo);

        /// <summary>
        /// Valide une fiche gasoil (En Cours → Validé).
        /// Lève InvalidOperationException si le statut actuel n'est pas 'En Cours'.
        /// Lève UnauthorizedAccessException si la fiche n'appartient pas au projet.
        /// </summary>
        Task<bool> ValiderFicheAsync(Guid id, string projectNo);

        // ─── LIGNES ───────────────────────────────────────────────────────────────

        /// <summary>
        /// Crée une ligne de distribution gasoil.
        /// Vérifie que le document cible appartient au projet avant création.
        /// projectNo est toujours forcé depuis le JWT.
        /// </summary>
        Task<GasoilLine?> CreateLineAsync(GasoilLine line, string projectNo);

        /// <summary>
        /// Met à jour une ligne de distribution gasoil.
        /// Lève UnauthorizedAccessException si la ligne n'appartient pas au projet.
        /// </summary>
        Task<GasoilLine?> UpdateLineAsync(Guid id, GasoilLine line, string projectNo);

        /// <summary>
        /// Supprime une ligne de distribution gasoil.
        /// Lève UnauthorizedAccessException si la ligne n'appartient pas au projet.
        /// </summary>
        Task<bool> DeleteLineAsync(Guid id, string projectNo);

        // ─── ALERTES (usage interne uniquement) ───────────────────────────────────

        /// <summary>
        /// Retourne toutes les fiches gasoil avec leurs lignes ($expand).
        /// Utilisé exclusivement par AlertService — non exposé dans le contrôleur.
        /// </summary>
        Task<IEnumerable<GasoilHeader>> GetHeadersWithLinesAsync(string projectNo);
    }
}