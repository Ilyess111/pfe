using System.Text.Json.Serialization;

namespace Soroubat.Api.Models
{
    /// <summary>
    /// Représente une ligne de pointage véhicule.
    /// Les lignes sont créées automatiquement avec l'en-tête (via BC) —
    /// le chef de chantier les modifie uniquement (hoursWorked, index, fuelConsumed, status, breakdownMotiv).
    /// marche (jobNo de la ligne) est utilisé par le backend pour la vérification de sécurité.
    /// </summary>
    public class VehiculePointageLine
    {
        // --- Identifiants (lecture seule) ---

        [JsonPropertyName("id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Guid? Id { get; set; }

        [JsonPropertyName("documentNo")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? DocumentNo { get; set; }

        // --- Véhicule ---

        [JsonPropertyName("vehiculeNo")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? VehiculeNo { get; set; }

        [JsonPropertyName("description")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Description { get; set; }

        // --- Statut ---

        [JsonPropertyName("status")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Status { get; set; }

        // --- Données de travail ---

        [JsonPropertyName("hoursWorked")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public decimal? HoursWorked { get; set; }

        // --- Index (kilométrique ou horaire) ---

        [JsonPropertyName("startIndex")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public decimal? StartIndex { get; set; }

        [JsonPropertyName("endIndex")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public decimal? EndIndex { get; set; }

        // --- Consommation ---

        [JsonPropertyName("fuelConsumed")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public decimal? FuelConsumed { get; set; }

        // --- Maintenance ---

        [JsonPropertyName("breakdownMotiv")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? BreakdownMotiv { get; set; }

        // --- Chantier (lecture seule — utilisé par le backend pour la vérification sécurité) ---

        [JsonPropertyName("marche")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Marche { get; set; }
    }
}