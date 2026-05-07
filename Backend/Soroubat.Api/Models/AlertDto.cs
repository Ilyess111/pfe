namespace Soroubat.Api.Models
{
    /// <summary>
    /// Représente une alerte générée par le système à partir des données BC.
    /// </summary>
    public class AlertDto
    {
        /// <summary>Identifiant unique de l'alerte (généré côté backend).</summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Catégorie de l'alerte.
        /// Valeurs possibles : "TaskDelay" | "TaskBlocked" | "TaskNotStarted" | "BudgetOverrun"
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Niveau de sévérité.
        /// Valeurs possibles : "Info" | "Warning" | "Critical"
        /// </summary>
        public string Severity { get; set; } = string.Empty;

        /// <summary>Titre court affiché dans le badge de notification Angular.</summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>Message détaillé affiché dans le panneau d'alertes.</summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Identifiant de l'entité concernée (TaskNo, JobNo…).
        /// Permet au frontend de naviguer vers la ressource en question.
        /// </summary>
        public string RelatedEntityNo { get; set; } = string.Empty;

        /// <summary>SystemId BC de la tâche concernée (utile pour PATCH direct).</summary>
        public Guid? RelatedEntityId { get; set; }

        /// <summary>Horodatage de la détection (UTC).</summary>
        public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
    }
}