using System.Text.Json.Serialization;

namespace Soroubat.Api.Models
{
    public class TransferHeaderDto
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("id")]
        public Guid? Id { get; set; }

        [JsonPropertyName("no")]
        public string? No { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("transferFromCode")]
        public string? TransferFromCode { get; set; }

        [JsonPropertyName("transferToCode")]
        public string? TransferToCode { get; set; }

        [JsonPropertyName("inTransitCode")]
        public string? InTransitCode { get; set; }

        [JsonPropertyName("postingDate")]
        public string? PostingDate { get; set; }

        [JsonPropertyName("observation")]
        public string? Observation { get; set; }

        [JsonPropertyName("chantierOrigine")]
        public string? ChantierOrigine { get; set; }

        [JsonPropertyName("chantierDestination")]
        public string? ChantierDestination { get; set; }

        [JsonPropertyName("idExpediteur")]
        public string? IdExpediteur { get; set; }

        [JsonPropertyName("idReceptionneur")]
        public string? IdReceptionneur { get; set; }

        [JsonPropertyName("numMateriel")]
        public string? NumMateriel { get; set; }

        [JsonPropertyName("numDemandeAchat")]
        public string? NumDemandeAchat { get; set; }

        [JsonPropertyName("transferLines")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<TransferLineDto>? TransferLines { get; set; }
    }
}