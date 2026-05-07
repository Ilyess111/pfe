using System.Text.Json.Serialization;

namespace Soroubat.Api.Models
{
    public class StockChantierDto
    {
        [JsonPropertyName("itemNo")]
        public string ItemNo { get; set; } = string.Empty;

        [JsonPropertyName("itemDescription")]
        public string ItemDescription { get; set; } = string.Empty;

        [JsonPropertyName("locationCode")]
        public string LocationCode { get; set; } = string.Empty;

        [JsonPropertyName("quantity")]
        public decimal Quantity { get; set; }

        [JsonPropertyName("jobNo")]
        public string JobNo { get; set; } = string.Empty;

        [JsonPropertyName("lastPostingDate")]
        public DateTime? LastPostingDate { get; set; }

        [JsonPropertyName("postingDate")] // Utilisé pour mapper le champ BC lors de la lecture
        public DateTime? PostingDate { get; set; }
    }
}