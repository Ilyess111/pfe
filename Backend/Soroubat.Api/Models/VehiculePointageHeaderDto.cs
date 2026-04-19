using Newtonsoft.Json; 


namespace Soroubat.Api.Models;
public class VehiculePointageHeader
{

    [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
    public Guid? Id { get; set; }

    [JsonProperty("documentNo", NullValueHandling = NullValueHandling.Ignore)]
    public string? DocumentNo { get; set; }

    [JsonProperty("jobNo", NullValueHandling = NullValueHandling.Ignore)]
    public string? JobNo { get; set; }

    [JsonProperty("date", NullValueHandling = NullValueHandling.Ignore)]
    public DateTime? Date { get; set; }

    [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]  
    public string? Status { get; set; }

    [JsonProperty("vehiculePointageLines", NullValueHandling = NullValueHandling.Ignore)]
    public List<VehiculePointageLine>? Lines { get; set; } = new List<VehiculePointageLine>();
}