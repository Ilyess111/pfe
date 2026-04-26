namespace Soroubat.Api.Models
{
    public class StockChantierDto
    {
        public string ItemNo { get; set; }
        public string ItemDescription { get; set; }
        public string LocationCode { get; set; }
        public decimal Quantity { get; set; }
        public string JobNo { get; set; }
        public DateTime? LastPostingDate { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("postingDate")]
        public DateTime? PostingDate { get; set; }

    }
}