using System.Text.Json.Serialization;

namespace Soroubat.Api.Models
{
    /// <summary>
    /// Représente une alerte générée par le système à partir des données BC.
    /// Chaque alerte est scopée au projet du chef de chantier connecté.
    /// </summary>
    public class AlertDto
    {
        /// <summary>Identifiant unique de l'alerte — généré côté backend à la détection.</summary>
        [JsonPropertyName("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Catégorie de l'alerte. Valeurs possibles :
        /// SiteManagement  : "TaskBlocked" | "TaskDelay" | "TaskNotStarted" | "BudgetOverrun"
        /// PurchaseRequest : "PurchaseRequestRejected" | "PurchaseRequestPendingTooLong" | "PurchaseRequestOverdue" | "PurchaseRequestEmpty"
        /// Transfer        : "TransferStuckInTransit" | "TransferNotShipped" | "TransferPartialReceipt" | "TransferNoVehicle"
        /// Stock           : "StockNegatif" | "StockCritique" | "StockDormant"
        /// Vehicule        : "PointageNonValide" | "VehiculeSuprutilise" | "IndexIncohérent" | "PanneSansMotif" | "ConsommationAnormale"
        /// Gasoil          : "GasoilFicheNonValidee" | "GasoilIndexIncohérent" | "GasoilConsommationTotaleAnormale" | "GasoilLigneSansVehicule" | "GasoilQuantiteLigneAnormale"
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Niveau de sévérité.
        /// Valeurs possibles : "Warning" | "Critical"
        /// </summary>
        [JsonPropertyName("severity")]
        public string Severity { get; set; } = string.Empty;

        /// <summary>Titre court affiché dans le badge de notification côté frontend.</summary>
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        /// <summary>Message détaillé affiché dans le panneau d'alertes.</summary>
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Numéro lisible de l'entité concernée (TaskNo, N° demande, N° transfert…).
        /// Permet au frontend de naviguer vers la ressource en question.
        /// </summary>
        [JsonPropertyName("relatedEntityNo")]
        public string RelatedEntityNo { get; set; } = string.Empty;

        /// <summary>SystemId BC de l'entité concernée — utile pour les actions directes depuis le frontend.</summary>
        [JsonPropertyName("relatedEntityId")]
        public Guid? RelatedEntityId { get; set; }

        /// <summary>Horodatage UTC de la détection de l'alerte.</summary>
        [JsonPropertyName("detectedAt")]
        public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
    }
}