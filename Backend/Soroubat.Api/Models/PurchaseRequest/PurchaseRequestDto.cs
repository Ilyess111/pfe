using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Soroubat.Api.Models
{
    public class PurchaseRequestDto
    {
        [JsonPropertyName("id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Guid? Id { get; set; }

        [JsonPropertyName("no")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? No { get; set; }

        [JsonPropertyName("observation")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Observation { get; set; }

        [JsonPropertyName("jobNo")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? JobNo { get; set; }

        [JsonPropertyName("jobDescription")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? JobDescription { get; set; }

        [JsonPropertyName("requesterId")]
        public string? RequesterId { get; set; }

        [JsonPropertyName("requestType")]
        public string? RequestType { get; set; }

        [JsonPropertyName("engin")]
        public string? Engin { get; set; }

        [JsonPropertyName("descriptionEngin")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? DescriptionEngin { get; set; }

        [JsonPropertyName("locationCode")]
        public string? LocationCode { get; set; }

        [JsonPropertyName("orderDate")]
        public string? OrderDate { get; set; }

        [JsonPropertyName("dueDate")]
        public string? DueDate { get; set; }

        [JsonPropertyName("statut")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Statut { get; set; }

        [JsonPropertyName("amount")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public decimal? Amount { get; set; }

        [JsonPropertyName("service")]
        public string? Service { get; set; }

        [JsonPropertyName("purchaseRequestLines")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<PurchaseRequestLineDto>? PurchaseRequestLines { get; set; }
    }
}