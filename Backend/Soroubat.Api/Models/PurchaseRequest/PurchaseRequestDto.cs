using System.Text.Json.Serialization;

namespace Soroubat.Api.Models
{
    /// <summary>
    /// Représente l'en-tête d'une demande d'achat Business Central.
    /// Les champs en lecture seule (id, no, amount, statut, jobDescription) sont ignorés
    /// lors de l'envoi vers BC (WhenWritingNull) pour éviter tout rejet par l'API.
    /// </summary>
    public class PurchaseRequestDto
    {
        /// <summary>SystemId BC — ignoré à l'envoi (assigné par BC).</summary>
        [JsonPropertyName("id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Guid? Id { get; set; }

        /// <summary>Numéro de document BC — ignoré à l'envoi (auto-incrémenté par BC).</summary>
        [JsonPropertyName("no")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? No { get; set; }

        [JsonPropertyName("observation")]
        public string? Observation { get; set; }

        /// <summary>Numéro de projet — forcé par le backend depuis le JWT, ignoré à l'envoi si null.</summary>
        [JsonPropertyName("jobNo")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? JobNo { get; set; }

        /// <summary>Libellé du projet — calculé par BC, ignoré à l'envoi.</summary>
        [JsonPropertyName("jobDescription")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? JobDescription { get; set; }

        [JsonPropertyName("requesterId")]
        public string? RequesterId { get; set; }

        [JsonPropertyName("requestType")]
        public string? RequestType { get; set; }

        [JsonPropertyName("engin")]
        public string? Engin { get; set; }

        /// <summary>Désignation de l'engin — calculée par BC, ignorée à l'envoi.</summary>
        [JsonPropertyName("descriptionEngin")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? DescriptionEngin { get; set; }

        [JsonPropertyName("locationCode")]
        public string? LocationCode { get; set; }

        [JsonPropertyName("orderDate")]
        public string? OrderDate { get; set; }

        [JsonPropertyName("dueDate")]
        public string? DueDate { get; set; }

        /// <summary>
        /// Statut BC — ignoré à l'envoi sur les opérations normales (Create/Update).
        /// Seule l'action /submit peut modifier ce champ via un PATCH dédié.
        /// </summary>
        [JsonPropertyName("statut")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Statut { get; set; }

        /// <summary>Montant total — calculé par BC, ignoré à l'envoi.</summary>
        [JsonPropertyName("amount")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public decimal? Amount { get; set; }

        [JsonPropertyName("service")]
        public string? Service { get; set; }

        /// <summary>Lignes de la demande — ignorées à l'envoi (gérées séparément).</summary>
        [JsonPropertyName("purchaseRequestLines")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<PurchaseRequestLineDto>? PurchaseRequestLines { get; set; }
    }
}