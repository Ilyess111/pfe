using System.Text.Json.Serialization;

namespace Soroubat.Api.Models
{
    /// <summary>
    /// Représente une ligne d'une demande d'achat Business Central.
    /// lineAmount et id sont en lecture seule (ignorés à l'envoi).
    /// jobNo est forcé côté backend depuis le JWT.
    /// </summary>
    public class PurchaseRequestLineDto
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

        // --- Article ---

        [JsonPropertyName("type")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Type { get; set; }

        [JsonPropertyName("no")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? No { get; set; }

        [JsonPropertyName("description")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Description { get; set; }

        [JsonPropertyName("description2")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Description2 { get; set; }

        [JsonPropertyName("variantCode")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? VariantCode { get; set; }

        // --- Quantité & Unité ---

        [JsonPropertyName("quantity")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public decimal? Quantity { get; set; }

        [JsonPropertyName("unitOfMeasureCode")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? UnitOfMeasureCode { get; set; }

        // --- Localisation ---

        [JsonPropertyName("locationCode")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? LocationCode { get; set; }

        // --- Projet & Engin ---

        [JsonPropertyName("jobNo")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? JobNo { get; set; }

        [JsonPropertyName("jobTaskNo")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? JobTaskNo { get; set; }

        [JsonPropertyName("engin")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Engin { get; set; }

        // --- Options ---

        [JsonPropertyName("transferer")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? Transferer { get; set; }

        // --- Montant (lecture seule — calculé par BC) ---

        [JsonPropertyName("lineAmount")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public decimal? LineAmount { get; set; }
    }
}