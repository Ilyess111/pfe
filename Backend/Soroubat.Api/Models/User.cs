namespace Soroubat.Api.Models
{
    /// <summary>
    /// Représente un utilisateur stocké dans la base SQLite locale.
    /// L'email sert de clé de liaison avec le compte Chef Chantier dans Business Central.
    /// Le mot de passe est systématiquement stocké sous forme de hash BCrypt — jamais en clair.
    /// </summary>
    public class User
    {
        public int Id { get; set; }

        /// <summary>Adresse e-mail — sert de clé de liaison avec BC et d'identifiant de connexion.</summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>Hash BCrypt du mot de passe. Jamais exposé dans les réponses API.</summary>
        public string PasswordHash { get; set; } = string.Empty;
    }
}