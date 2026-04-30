using System.Text.Json.Serialization;

namespace Soroubat.Api.Models
{
    /// <summary>
    /// Représente l'en-tête d'un ordre de transfert Business Central.
    /// Ce DTO est en lecture seule depuis l'API — seules les lignes (qtyToReceive) sont modifiables.
    /// Le filtrage par chantierDestination est le mécanisme de sécurité principal.
    /// </summary>
    public class TransferHeaderDto
    {
        // --- Identifiants ---

        [JsonPropertyName("id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Guid? Id { get; set; }

        [JsonPropertyName("no")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? No { get; set; }

        // --- Statut & Date ---

        [JsonPropertyName("status")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Status { get; set; }

        [JsonPropertyName("postingDate")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? PostingDate { get; set; }

        // --- Localisation ---

        [JsonPropertyName("transferFromCode")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? TransferFromCode { get; set; }

        [JsonPropertyName("transferToCode")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? TransferToCode { get; set; }

        [JsonPropertyName("inTransitCode")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? InTransitCode { get; set; }

        // --- Chantiers Soroubat ---

        [JsonPropertyName("chantierOrigine")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ChantierOrigine { get; set; }

        /// <summary>
        /// Chantier de destination — pivot de sécurité utilisé par le backend
        /// pour scoper les transferts au projet du chef de chantier connecté.
        /// </summary>
        [JsonPropertyName("chantierDestination")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ChantierDestination { get; set; }

        // --- Intervenants ---

        [JsonPropertyName("idExpediteur")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? IdExpediteur { get; set; }

        [JsonPropertyName("idReceptionneur")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? IdReceptionneur { get; set; }

        // --- Références ---

        [JsonPropertyName("observation")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Observation { get; set; }

        [JsonPropertyName("numMateriel")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? NumMateriel { get; set; }

        [JsonPropertyName("numDemandeAchat")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? NumDemandeAchat { get; set; }

        // --- Lignes (incluses uniquement sur GET par id avec $expand) ---

        [JsonPropertyName("transferLines")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<TransferLineDto>? TransferLines { get; set; }
    }
}