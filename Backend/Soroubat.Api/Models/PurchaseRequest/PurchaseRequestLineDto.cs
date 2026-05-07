using System.Text.Json.Serialization;

namespace Soroubat.Api.Models
{
    /// <summary>
    /// Représente une ligne d'une demande d'achat Business Central.
    /// </summary>
    public class PurchaseRequestLineDto
    {
        /// <summary>SystemId BC — ignoré à l'envoi (assigné par BC).</summary>
        [JsonPropertyName("id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Guid? Id { get; set; }

        /// <summary>Numéro du document parent — obligatoire pour lier la ligne à son en-tête.</summary>
        [JsonPropertyName("documentNo")]
        public string? DocumentNo { get; set; }

        /// <summary>
        /// Numéro de ligne — assigné par le backend avant l'envoi à BC.
        /// Doit être envoyé en insertion (lineNo Editable = true côté AL).
        /// Ignoré à l'envoi uniquement sur les PATCH (WhenWritingNull).
        /// </summary>
        [JsonPropertyName("lineNo")]
        public int? LineNo { get; set; }

        [JsonPropertyName("transferer")]
        public bool? Transferer { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        /// <summary>Numéro d'article ou de compte G/L.</summary>
        [JsonPropertyName("no")]
        public string? No { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("description2")]
        public string? Description2 { get; set; }

        [JsonPropertyName("quantity")]
        public decimal? Quantity { get; set; }

        [JsonPropertyName("unitOfMeasureCode")]
        public string? UnitOfMeasureCode { get; set; }

        [JsonPropertyName("locationCode")]
        public string? LocationCode { get; set; }

        [JsonPropertyName("variantCode")]
        public string? VariantCode { get; set; }

        /// <summary>Numéro de projet — forcé par le backend depuis le JWT, ignoré à l'envoi si null.</summary>
        [JsonPropertyName("jobNo")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? JobNo { get; set; }

        [JsonPropertyName("jobTaskNo")]
        public string? JobTaskNo { get; set; }

        [JsonPropertyName("engin")]
        public string? Engin { get; set; }

        /// <summary>Montant de la ligne — calculé par BC, ignoré à l'envoi.</summary>
        [JsonPropertyName("lineAmount")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public decimal? LineAmount { get; set; }
    }
}