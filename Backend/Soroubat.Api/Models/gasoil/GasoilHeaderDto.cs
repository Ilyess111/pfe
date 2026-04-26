using System.Text.Json.Serialization;

namespace Soroubat.Api.Models
{
    public class GasoilHeader
    {
        [JsonPropertyName("id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Guid? Id { get; set; }

        [JsonPropertyName("documentNo")]
        public string? DocumentNo { get; set; }

        [JsonPropertyName("date")]
        public string? Date { get; set; }

        [JsonPropertyName("locationCode")]
        public string? LocationCode { get; set; }

        [JsonPropertyName("jobNo")]
        public string? JobNo { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("fileNo")]
        public string? FileNo { get; set; }

        [JsonPropertyName("startIndex")]
        public decimal? StartIndex { get; set; }

        [JsonPropertyName("endIndex")]
        public decimal? EndIndex { get; set; }

        [JsonPropertyName("gasoilLines")]
        public List<GasoilLine> Lines { get; set; } = new List<GasoilLine>();
    }
}