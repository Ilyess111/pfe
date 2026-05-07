using System.Text.Json.Serialization;

namespace Soroubat.Api.Models
{
    public class VehiculePointageHeader
    {
        [JsonPropertyName("id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Guid? Id { get; set; }

        [JsonPropertyName("documentNo")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? DocumentNo { get; set; }

        [JsonPropertyName("jobNo")]
        public string? JobNo { get; set; }

        [JsonPropertyName("date")]
        public string? Date { get; set; }

        [JsonPropertyName("status")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Status { get; set; }

        [JsonPropertyName("vehiculePointageLines")]
        public List<VehiculePointageLine> Lines { get; set; } = new List<VehiculePointageLine>();
    }
}