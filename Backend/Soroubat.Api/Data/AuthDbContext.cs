using Microsoft.EntityFrameworkCore;
using Soroubat.Api.Models; // Assurez-vous d'avoir un dossier Models avec la classe User

namespace Soroubat.Api.Data
{
    public class AuthDbContext : DbContext
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; } // Votre table de comptes utilisateurs
    }
}