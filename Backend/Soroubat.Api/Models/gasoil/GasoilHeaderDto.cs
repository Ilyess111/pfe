using System.Text.Json.Serialization;

namespace Soroubat.Api.Models
{
    /// <summary>
    /// Représente l'en-tête d'une fiche gasoil journalière.
    /// jobNo est toujours forcé depuis le JWT côté backend — jamais depuis le body client.
    /// status est en lecture seule depuis l'API — modifiable uniquement via l'action /valider.
    /// </summary>
    public class GasoilHeader
    {
        // --- Identifiants (lecture seule) ---

        [JsonPropertyName("id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Guid? Id { get; set; }

        [JsonPropertyName("documentNo")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? DocumentNo { get; set; }

        // --- Chantier (forcé depuis le JWT — non modifiable via l'API) ---

        [JsonPropertyName("jobNo")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? JobNo { get; set; }

        // --- Informations fiche ---

        [JsonPropertyName("date")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Date { get; set; }

        [JsonPropertyName("fileNo")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? FileNo { get; set; }

        [JsonPropertyName("locationCode")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? LocationCode { get; set; }

        // --- Index cuve ---

        [JsonPropertyName("startIndex")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public decimal? StartIndex { get; set; }

        [JsonPropertyName("endIndex")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public decimal? EndIndex { get; set; }

        // --- Statut (lecture seule — modifié uniquement via /valider) ---

        [JsonPropertyName("status")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Status { get; set; }

        // --- Lignes (incluses uniquement sur GET par id avec $expand) ---

        [JsonPropertyName("gasoilLines")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<GasoilLine>? Lines { get; set; }
    }
}