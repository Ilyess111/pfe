using System.Text.Json.Serialization;

namespace Soroubat.Api.Models
{
    /// <summary>
    /// Représente une tâche projet (Job Task) Business Central.
    /// Seul progressPct est modifiable par le chef de chantier via PATCH tasks/{id}/progress.
    /// Tous les autres champs sont en lecture seule.
    /// </summary>
    public class JobTaskDto
    {
        // --- Identifiants ---

        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("jobNo")]
        public string JobNo { get; set; } = string.Empty;

        [JsonPropertyName("taskNo")]
        public string TaskNo { get; set; } = string.Empty;

        // --- Informations générales ---

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("isBlocked")]
        public bool IsBlocked { get; set; }

        // --- Planification (lecture seule — définie dans BC par le chef de projet) ---

        [JsonPropertyName("dateDebut")]
        public DateTime? DateDebut { get; set; }

        [JsonPropertyName("dateFin")]
        public DateTime? DateFin { get; set; }

        // --- Avancement ---

        /// <summary>Seul champ modifiable depuis l'API — via PATCH tasks/{id}/progress.</summary>
        [JsonPropertyName("progressPct")]
        public decimal ProgressPct { get; set; }

        /// <summary>Calculé automatiquement par BC depuis les quantités réalisées — lecture seule.</summary>
        [JsonPropertyName("taskProgressPct")]
        public decimal TaskProgressPct { get; set; }

        // --- Réalisé ---

        [JsonPropertyName("quantityShipped")]
        public decimal QuantityShipped { get; set; }

        [JsonPropertyName("usageTotalCost")]
        public decimal UsageTotalCost { get; set; }

        // --- Budget initial ---

        [JsonPropertyName("initialQuantity")]
        public decimal InitialQuantity { get; set; }

        [JsonPropertyName("initialUoM")]
        public string InitialUoM { get; set; } = string.Empty;

        [JsonPropertyName("initialAmount")]
        public decimal InitialAmount { get; set; }
    }
}