using System.Text.Json.Serialization;

namespace Soroubat.Api.Models
{
    /// <summary>
    /// Représente une ligne d'un ordre de transfert Business Central.
    /// Seul qtyToReceive est modifiable par le chef de chantier (saisie de réception).
    /// Tous les autres champs sont en lecture seule.
    /// </summary>
    public class TransferLineDto
    {
        // --- Identifiants ---

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

        [JsonPropertyName("itemNo")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ItemNo { get; set; }

        [JsonPropertyName("description")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Description { get; set; }

        [JsonPropertyName("descriptionSoroubat")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? DescriptionSoroubat { get; set; }

        // --- Quantités ---

        [JsonPropertyName("quantity")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public decimal? Quantity { get; set; }

        [JsonPropertyName("quantityShipped")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public decimal? QuantityShipped { get; set; }

        [JsonPropertyName("quantityReceived")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public decimal? QuantityReceived { get; set; }

        /// <summary>
        /// Seul champ modifiable par le chef de chantier — saisie de la quantité à réceptionner.
        /// </summary>
        [JsonPropertyName("qtyToReceive")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public decimal? QtyToReceive { get; set; }

        // --- Unité & Stock ---

        [JsonPropertyName("unitOfMeasure")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? UnitOfMeasure { get; set; }

        [JsonPropertyName("stock")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public decimal? Stock { get; set; }

        // --- Logistique & Analytique ---

        [JsonPropertyName("numVehicule")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? NumVehicule { get; set; }

        [JsonPropertyName("affaire")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Affaire { get; set; }
    }
}