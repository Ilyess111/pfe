using System.Text.Json.Serialization;

namespace Soroubat.Api.Models
{
    /// <summary>
    /// Représente l'en-tête d'une demande d'achat Business Central.
    /// Les champs en lecture seule (id, no, statut, amount, jobDescription) sont ignorés
    /// lors de l'envoi vers BC grâce à JsonIgnoreCondition.WhenWritingNull.
    /// </summary>
    public class PurchaseRequestDto
    {
        // --- Identifiants (lecture seule) ---

        [JsonPropertyName("id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Guid? Id { get; set; }

        [JsonPropertyName("no")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? No { get; set; }

        // --- Projet (lecture seule côté API — forcé par le backend) ---

        [JsonPropertyName("jobNo")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? JobNo { get; set; }

        [JsonPropertyName("jobDescription")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? JobDescription { get; set; }

        // --- Informations générales ---

        [JsonPropertyName("observation")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Observation { get; set; }

        [JsonPropertyName("requesterId")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? RequesterId { get; set; }

        [JsonPropertyName("requestType")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? RequestType { get; set; }

        [JsonPropertyName("service")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Service { get; set; }

        // --- Engin ---

        [JsonPropertyName("engin")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Engin { get; set; }

        [JsonPropertyName("descriptionEngin")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? DescriptionEngin { get; set; }

        // --- Logistique ---

        [JsonPropertyName("locationCode")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? LocationCode { get; set; }

        // --- Dates ---

        [JsonPropertyName("orderDate")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? OrderDate { get; set; }

        [JsonPropertyName("dueDate")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? DueDate { get; set; }

        // --- Statut & Montant (lecture seule — jamais envoyés en PATCH/POST) ---

        [JsonPropertyName("statut")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Statut { get; set; }

        [JsonPropertyName("amount")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public decimal? Amount { get; set; }

        // --- Lignes (incluses uniquement sur GET par id avec $expand) ---

        [JsonPropertyName("purchaseRequestLines")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<PurchaseRequestLineDto>? PurchaseRequestLines { get; set; }
    }
}