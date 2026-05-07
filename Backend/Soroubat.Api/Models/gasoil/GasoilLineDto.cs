using System.Text.Json.Serialization;

namespace Soroubat.Api.Models
{
    public class GasoilLine
    {
        [JsonPropertyName("id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Guid? Id { get; set; }

        [JsonPropertyName("documentNo")]
        public string? DocumentNo { get; set; }

        [JsonPropertyName("lineNo")]
        public int? LineNo { get; set; }

        [JsonPropertyName("vehicleNo")]
        public string? VehicleNo { get; set; }

        [JsonPropertyName("vehiclePlate")]
        public string? VehiclePlate { get; set; }

        [JsonPropertyName("quantity")]
        public decimal? Quantity { get; set; }

        [JsonPropertyName("indexType")]
        public string? IndexType { get; set; }

        [JsonPropertyName("hourIndex")]
        public decimal? HourIndex { get; set; }

        [JsonPropertyName("kmIndex")]
        public decimal? KmIndex { get; set; }

        [JsonPropertyName("projectNo")]
        public string? ProjectNo { get; set; }
    }
}