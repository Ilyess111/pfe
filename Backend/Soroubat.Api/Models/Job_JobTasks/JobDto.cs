using System.Text.Json.Serialization;

namespace Soroubat.Api.Models
{
    /// <summary>
    /// Représente un projet (Job) Business Central retourné par l'API JobAPI.
    /// Ce DTO est en lecture seule : le chef de chantier ne modifie pas les données projet.
    /// </summary>
    public class JobDto
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("no")]
        public string No { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("startingDate")]
        public DateTime? StartingDate { get; set; }

        [JsonPropertyName("endingDate")]
        public DateTime? EndingDate { get; set; }

        [JsonPropertyName("personResponsible")]
        public string PersonResponsible { get; set; } = string.Empty;

        [JsonPropertyName("projectManager")]
        public string ProjectManager { get; set; } = string.Empty;

        [JsonPropertyName("affectationMagasin")]
        public string AffectationMagasin { get; set; } = string.Empty;
    }
}