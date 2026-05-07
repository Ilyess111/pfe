using System.Text.Json.Serialization;

namespace Soroubat.Api.Models
{
    // DTO utilisé UNIQUEMENT pour le POST vers BC — sans "no" ni "id"
    public class CreateEmpAttendanceDto
    {
        [JsonPropertyName("jobNo")]
        public string? JobNo { get; set; }

        [JsonPropertyName("month")]
        public string Month { get; set; }

        [JsonPropertyName("year")]
        public int Year { get; set; }
    }
}