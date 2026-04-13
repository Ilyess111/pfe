using System.Text.Json.Serialization;

namespace Soroubat.Api.Models
{
    public class TransferLineDto
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("id")]
        public Guid? Id { get; set; }

        [JsonPropertyName("documentNo")]
        public string? DocumentNo { get; set; }

        [JsonPropertyName("lineNo")]
        public int? LineNo { get; set; }

        [JsonPropertyName("itemNo")]
        public string? ItemNo { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("quantity")]
        public decimal Quantity { get; set; }
        [JsonPropertyName("quantityShipped")]
        public decimal? QuantityShipped { get; set; }

        [JsonPropertyName("quantityReceived")]
        public decimal? QuantityReceived { get; set; }

        [JsonPropertyName("qtyToReceive")]
        public decimal? QtyToReceive { get; set; }

        [JsonPropertyName("unitOfMeasure")]
        public string? UnitOfMeasure { get; set; }

        [JsonPropertyName("stock")]
        public decimal? Stock { get; set; }

        [JsonPropertyName("numVehicule")]
        public string? NumVehicule { get; set; }

        [JsonPropertyName("affaire")]
        public string? Affaire { get; set; }

        [JsonPropertyName("descriptionSoroubat")]
        public string? DescriptionSoroubat { get; set; }
    }
}