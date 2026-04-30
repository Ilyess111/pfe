namespace Soroubat.Api.Interfaces
{
    /// <summary>
    /// Contrat du service d'authentification.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Vérifie les identifiants et retourne un JWT signé si valides.
        /// Retourne null si l'e-mail est inconnu, le mot de passe incorrect,
        /// ou si le compte n'a pas de projet BC associé actif.
        /// </summary>
        Task<string?> AuthenticateAsync(string email, string password);

        /// <summary>
        /// Génère un JWT signé contenant les claims email et projectNo.
        /// Méthode exposée dans l'interface pour permettre les tests unitaires.
        /// </summary>
        string GenerateJwtToken(string email, string projectNo);
    }
}