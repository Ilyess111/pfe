using System.Text.Json.Serialization;

namespace Soroubat.Api.Models
{
    /// <summary>
    /// Représente l'en-tête d'un pointage véhicule journalier.
    /// jobNo est toujours forcé depuis le JWT côté backend — jamais depuis le body client.
    /// status est en lecture seule depuis l'API — modifiable uniquement via l'action /valider.
    /// </summary>
    public class VehiculePointageHeader
    {
        // --- Identifiants (lecture seule) ---

        [JsonPropertyName("id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Guid? Id { get; set; }

        [JsonPropertyName("documentNo")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? DocumentNo { get; set; }

        // --- Chantier (forcé depuis le JWT — ignoré à l'envoi) ---

        [JsonPropertyName("jobNo")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? JobNo { get; set; }

        // --- Informations pointage ---

        [JsonPropertyName("date")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Date { get; set; }

        // --- Statut (lecture seule — modifié uniquement via /valider) ---

        [JsonPropertyName("status")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Status { get; set; }

        // --- Lignes (incluses uniquement sur GET par id avec $expand) ---

        [JsonPropertyName("vehiculePointageLines")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<VehiculePointageLine>? Lines { get; set; }
    }
}