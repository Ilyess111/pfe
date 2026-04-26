using System.Text.Json.Serialization;

namespace Soroubat.Api.Models
{
    public class GasoilLine
    {
        [JsonPropertyName("id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Guid? Id { get; set; }

        [JsonPropertyName("documentNo")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? DocumentNo { get; set; }

        [JsonPropertyName("lineNo")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? LineNo { get; set; }

        [JsonPropertyName("vehicleNo")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? VehicleNo { get; set; }

        [JsonPropertyName("vehiclePlate")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? VehiclePlate { get; set; }

        [JsonPropertyName("quantity")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public decimal? Quantity { get; set; }

        [JsonPropertyName("indexType")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? IndexType { get; set; }

        [JsonPropertyName("hourIndex")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public decimal? HourIndex { get; set; }

        [JsonPropertyName("kmIndex")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public decimal? KmIndex { get; set; }

        [JsonPropertyName("projectNo")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ProjectNo { get; set; }
    }
}