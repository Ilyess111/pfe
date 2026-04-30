using System.Text.Json.Serialization;

namespace Soroubat.Api.Models
{
    /// <summary>
    /// Représente le stock agrégé d'un article pour un chantier donné.
    /// Calculé côté backend à partir des écritures comptables articles (Item Ledger Entries) :
    /// les quantités sont sommées par article + magasin, les quantités nulles sont exclues.
    /// </summary>
    public class StockChantierDto
    {
        // --- Identification de l'article ---

        [JsonPropertyName("itemNo")]
        public string ItemNo { get; set; } = string.Empty;

        [JsonPropertyName("itemDescription")]
        public string ItemDescription { get; set; } = string.Empty;

        // --- Localisation ---

        [JsonPropertyName("locationCode")]
        public string LocationCode { get; set; } = string.Empty;

        // --- Projet ---

        [JsonPropertyName("jobNo")]
        public string JobNo { get; set; } = string.Empty;

        // --- Stock ---

        [JsonPropertyName("quantity")]
        public decimal Quantity { get; set; }

        // --- Dernier mouvement ---
        // Calculé côté backend comme le Max(postingDate) des écritures du groupe.
        // Utilisé par AlertService pour détecter les stocks dormants.

        [JsonPropertyName("lastPostingDate")]
        public DateTime? LastPostingDate { get; set; }
    }
}