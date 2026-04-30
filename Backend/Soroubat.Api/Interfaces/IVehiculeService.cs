using Soroubat.Api.Models;

namespace Soroubat.Api.Interfaces
{
    /// <summary>
    /// Contrat du service de gestion des pointages véhicules.
    /// Toutes les opérations sont scopées au projet du chef de chantier connecté.
    /// Les lignes sont créées automatiquement par BC à la création de l'en-tête —
    /// seule leur modification est exposée depuis l'API.
    /// </summary>
    public interface IVehiculeService
    {
        // ─── HEADERS ─────────────────────────────────────────────────────────────

        /// <summary>Retourne tous les pointages du projet.</summary>
        Task<IEnumerable<VehiculePointageHeader>> GetHeadersByJobAsync(string projectNo);

        /// <summary>
        /// Retourne un pointage avec ses lignes ($expand).
        /// Retourne null si introuvable (404).
        /// Lève UnauthorizedAccessException si le pointage n'appartient pas au projet.
        /// </summary>
        Task<VehiculePointageHeader?> GetHeaderByIdAsync(Guid id, string projectNo);

        /// <summary>
        /// Crée un en-tête de pointage.
        /// jobNo est toujours forcé depuis le JWT.
        /// </summary>
        Task<VehiculePointageHeader?> CreateHeaderAsync(VehiculePointageHeader header, string projectNo);

        /// <summary>
        /// Met à jour les champs modifiables d'un en-tête.
        /// Vérifie l'appartenance au projet avant la mise à jour.
        /// Lève UnauthorizedAccessException si le pointage n'appartient pas au projet.
        /// </summary>
        Task<VehiculePointageHeader?> UpdateHeaderAsync(Guid id, VehiculePointageHeader header, string projectNo);

        /// <summary>
        /// Supprime un en-tête de pointage.
        /// Lève UnauthorizedAccessException si le pointage n'appartient pas au projet.
        /// </summary>
        Task<bool> DeleteHeaderAsync(Guid id, string projectNo);

        /// <summary>
        /// Valide un pointage (Ouvert → Validé).
        /// Lève InvalidOperationException si le statut actuel n'est pas 'Ouvert'.
        /// Lève UnauthorizedAccessException si le pointage n'appartient pas au projet.
        /// </summary>
        Task<bool> ValiderPointageAsync(Guid id, string projectNo);

        // ─── LIGNES ───────────────────────────────────────────────────────────────

        /// <summary>
        /// Met à jour une ligne de pointage.
        /// Vérifie l'appartenance au projet via le champ marche de la ligne.
        /// Lève UnauthorizedAccessException si la ligne n'appartient pas au projet.
        /// </summary>
        Task<VehiculePointageLine?> UpdateLineAsync(Guid id, VehiculePointageLine line, string projectNo);

        // ─── ALERTES (usage interne uniquement) ───────────────────────────────────

        /// <summary>
        /// Retourne tous les pointages avec leurs lignes ($expand).
        /// Utilisé exclusivement par AlertService — non exposé dans le contrôleur.
        /// </summary>
        Task<IEnumerable<VehiculePointageHeader>> GetHeadersWithLinesAsync(string projectNo);
    }
}