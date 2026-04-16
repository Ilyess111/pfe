using System;
using System.Text.Json.Serialization;

namespace Soroubat.Api.Models
{
    public class ChefChantierDto
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("nomEtPrenom")]
        public string NomEtPrenom { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("numProjet")]
        public string NumProjet { get; set; }

        [JsonPropertyName("actif")]
        public bool Actif { get; set; }
    }
}