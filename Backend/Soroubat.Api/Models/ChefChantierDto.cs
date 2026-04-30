using System.Text.Json.Serialization;

namespace Soroubat.Api.Models
{
    /// <summary>
    /// Représente un Chef Chantier retourné par l'API ChefChantierAPI de Business Central.
    /// Utilisé uniquement lors de l'authentification pour récupérer le numéro de projet associé.
    /// </summary>
    public class ChefChantierDto
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("nomEtPrenom")]
        public string NomEtPrenom { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("numProjet")]
        public string NumProjet { get; set; } = string.Empty;

        [JsonPropertyName("actif")]
        public bool Actif { get; set; }

        [JsonPropertyName("idApprobateur")]
        public string IdApprobateur { get; set; } = string.Empty;
    }
}