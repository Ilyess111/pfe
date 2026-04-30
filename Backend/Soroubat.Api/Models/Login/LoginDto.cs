using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Soroubat.Api.Models
{
    /// <summary>
    /// Corps de la requête POST /api/auth/login.
    /// </summary>
    public class LoginDto
    {
        /// <summary>Adresse e-mail du chef de chantier.</summary>
        [Required(ErrorMessage = "L'adresse e-mail est obligatoire.")]
        [EmailAddress(ErrorMessage = "Format d'adresse e-mail invalide.")]
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        /// <summary>Mot de passe en clair — comparé au hash BCrypt stocké en base.</summary>
        [Required(ErrorMessage = "Le mot de passe est obligatoire.")]
        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;
    }
}