using System.Text.Json.Serialization;

namespace Soroubat.Api.Models
{
    /// <summary>
    /// Corps de la requête PATCH pour la saisie de réception d'une ligne de transfert.
    /// Seul qtyToReceive est accepté — tous les autres champs de la ligne sont ignorés
    /// pour empêcher toute modification non autorisée via ce endpoint.
    /// </summary>
    public class TransferLineUpdateDto
    {
        /// <summary>
        /// Quantité réceptionnée par le chef de chantier.
        /// Doit être supérieure à 0 et inférieure ou égale à la quantité expédiée (quantityShipped).
        /// </summary>
        [JsonPropertyName("qtyToReceive")]
        public decimal QtyToReceive { get; set; }
    }
}