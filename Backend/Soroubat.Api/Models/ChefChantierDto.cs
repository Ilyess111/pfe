using System;
using Newtonsoft.Json; 

namespace Soroubat.Api.Models
{
    public class ChefChantierDto
    {
        [JsonProperty("id")] 
        public Guid Id { get; set; }

        [JsonProperty("nomEtPrenom")]
        public string NomEtPrenom { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("numProjet")]
        public string NumProjet { get; set; }

        [JsonProperty("actif")]
        public bool Actif { get; set; }
    }
}