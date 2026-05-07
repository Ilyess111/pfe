using System.Text.Json.Serialization;

namespace Soroubat.Api.Models
{
    public class EmpAttendanceLineDto
    {
        [JsonPropertyName("id")]
    public Guid Id { get; set; }
        public string? JobNo { get; set; }
        public string? DocumentNo { get; set; }
        public string EmployeeNo { get; set; }
        public string? EmployeeName { get; set; }
        public string? Assignment { get; set; }
        public string? Qualification { get; set; }
        
        // Jours 1 à 31
        public string? Day1 { get; set; }
        public string? Day2 { get; set; }
        public string? Day3 { get; set; }
        public string? Day4 { get; set; }
        public string? Day5 { get; set; }
        public string? Day6 { get; set; }
        public string? Day7 { get; set; }
        public string? Day8 { get; set; }
        public string? Day9 { get; set; }
        public string? Day10 { get; set; }
        public string? Day11 { get; set; }
        public string? Day12 { get; set; }
        public string? Day13 { get; set; }
        public string? Day14 { get; set; }
        public string? Day15 { get; set; }
        public string? Day16 { get; set; }
        public string? Day17 { get; set; }
        public string? Day18 { get; set; }
        public string? Day19 { get; set; }
        public string? Day20 { get; set; }
        public string? Day21 { get; set; }
        public string? Day22 { get; set; }
        public string? Day23 { get; set; }
        public string? Day24 { get; set; }
        public string? Day25 { get; set; }
        public string? Day26 { get; set; }
        public string? Day27 { get; set; }
        public string? Day28 { get; set; }
        public string? Day29 { get; set; }
        public string? Day30 { get; set; }
        public string? Day31 { get; set; }

        public decimal? TotalPresentDays { get; set; }
        public int? TotalAbsentDays { get; set; }
        public decimal? TotalHours { get; set; }
    }
}