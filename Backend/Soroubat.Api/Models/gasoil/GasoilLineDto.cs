using System.Text.Json.Serialization;

namespace Soroubat.Api.Models
{
    /// <summary>
    /// Représente une ligne de distribution gasoil.
    /// projectNo est le pivot de sécurité — toujours forcé depuis le JWT côté backend.
    /// id, documentNo, lineNo et vehiclePlate sont en lecture seule.
    /// </summary>
    public class GasoilLine
    {
        // --- Identifiants (lecture seule) ---

        [JsonPropertyName("id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Guid? Id { get; set; }

        [JsonPropertyName("documentNo")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? DocumentNo { get; set; }

        [JsonPropertyName("lineNo")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? LineNo { get; set; }

        // --- Véhicule ---

        [JsonPropertyName("vehicleNo")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? VehicleNo { get; set; }

        [JsonPropertyName("vehiclePlate")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? VehiclePlate { get; set; }

        [JsonPropertyName("driver")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Driver { get; set; }

        // --- Distribution ---

        [JsonPropertyName("quantity")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public decimal? Quantity { get; set; }

        [JsonPropertyName("time")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Time { get; set; }

        // --- Index ---

        [JsonPropertyName("indexType")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? IndexType { get; set; }

        [JsonPropertyName("hourIndex")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public decimal? HourIndex { get; set; }

        [JsonPropertyName("kmIndex")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public decimal? KmIndex { get; set; }

        // --- Projet (pivot de sécurité — forcé depuis le JWT) ---

        [JsonPropertyName("projectNo")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ProjectNo { get; set; }
    }
}