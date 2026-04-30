using Microsoft.EntityFrameworkCore;
using Soroubat.Api.Models; 

namespace Soroubat.Api.Data
{
    public class AuthDbContext : DbContext
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) // base(options) renvoie les options de configuration à la classe de base DbContext pour l'initialisation
        // DbContextOptions<AuthDbContext> est une classe générique qui contient les paramètres de configuration spécifiques au contexte authdbcontext, comme la chaîne de connexion, le fournisseur de base de données, etc.
        // c'est défini dans Program.cs lors de l'ajout du service : builder.Services.AddDbContext<AuthDbContext>(options => options.UseSqlite(...))
        {
        }

        public DbSet<User> Users { get; set; } // la table de comptes utilisateurs
    }
}