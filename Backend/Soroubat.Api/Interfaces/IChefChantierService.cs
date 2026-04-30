namespace Soroubat.Api.Interfaces
{
    /// <summary>
    /// Contrat du service d'accès aux données Chef Chantier dans Business Central.
    /// Utilisé lors de l'authentification pour résoudre le numéro de projet à partir de l'e-mail.
    /// </summary>
    public interface IChefChantierService
    {
        /// <summary>
        /// Retourne le numéro de projet BC associé à l'adresse e-mail du chef de chantier.
        /// Retourne null si aucun enregistrement ne correspond.
        /// </summary>
        Task<string?> GetJobNoByEmailAsync(string email);
    }
}