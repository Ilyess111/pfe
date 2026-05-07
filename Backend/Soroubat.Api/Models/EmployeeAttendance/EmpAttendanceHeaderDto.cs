using System.Text.Json.Serialization;

namespace Soroubat.Api.Models
{
    public class EmpAttendanceDto
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("id")]
        public Guid? Id { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("no")]
        public string? No { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("jobNo")]
        public string? JobNo { get; set; }

        [JsonPropertyName("month")]
        public string Month { get; set; }

        [JsonPropertyName("year")]
        public int Year { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("totalStaff")]
        public int? TotalStaff { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("attendanceRate")]
        public decimal? AttendanceRate { get; set; }

        [JsonPropertyName("employeeAttendanceLines")]
        public List<EmpAttendanceLineDto> Lines { get; set; } = new();
    }
}