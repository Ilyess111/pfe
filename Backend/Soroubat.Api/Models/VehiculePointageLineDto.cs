using Newtonsoft.Json;

namespace Soroubat.Api.Models
{
    public class VehiculePointageLine
    {
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public Guid? Id { get; set; }

        [JsonProperty("documentNo", NullValueHandling = NullValueHandling.Ignore)]
        public string? DocumentNo { get; set; }

        [JsonProperty("vehiculeNo", NullValueHandling = NullValueHandling.Ignore)]
        public string? VehiculeNo { get; set; }

        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string? Description { get; set; }

        [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
        public string? Status { get; set; }

        [JsonProperty("hoursWorked")]
        public decimal HoursWorked { get; set; }

        [JsonProperty("startIndex")] 
        public decimal StartIndex { get; set; } // Index Depart

        [JsonProperty("endIndex")]
        public decimal EndIndex { get; set; }   // Index Final

        [JsonProperty("fuelConsumed", NullValueHandling = NullValueHandling.Ignore)]
        public decimal? FuelConsumed { get; set; }

        [JsonProperty("breakdownMotiv", NullValueHandling = NullValueHandling.Ignore)]
        public string? BreakdownMotiv { get; set; }
    }
}