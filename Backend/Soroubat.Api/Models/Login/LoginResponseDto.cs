    /// <summary>
    /// Réponse retournée après une authentification réussie.
    /// </summary>
    public class LoginResponseDto
    {
        /// <summary>JWT signé à transmettre dans le header Authorization.</summary>
        public string Token { get; set; } = string.Empty;
    }