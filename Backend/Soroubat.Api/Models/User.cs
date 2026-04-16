public class User {
    public int Id { get; set; }
    public string Email { get; set; } // Sert de lien avec BC
    public string PasswordHash { get; set; } // Mot de passe crypté
}