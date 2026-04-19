using Newtonsoft.Json;
using Soroubat.Api.Models;
using Soroubat.Api.Interfaces;
using System.Net.Http;
using System.Linq;
using System.Threading.Tasks;

namespace Soroubat.Api.Services
{
    public class ChefChantierService : IChefChantierService
    {
        private readonly HttpClient _httpClient;

        public ChefChantierService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GetJobNoByEmailAsync(string email)
        {
            // On utilise le chemin relatif vers votre API personnalisée
            // Notez l'absence de 'Company(...)' car elle est souvent incluse dans la BaseAddress de l'API
            var response = await _httpClient.GetAsync($"chefsChantier?$filter=email eq '{email}'");
            
            if (!response.IsSuccessStatusCode) return null;

            var content = await response.Content.ReadAsStringAsync();
            
            // Désérialisation avec votre BCResponse et ChefChantierDto
            var result = JsonConvert.DeserializeObject<BCResponse<ChefChantierDto>>(content);

            // Retourne 'numProjet' (nom défini dans votre fichier AL et DTO)
            return result?.Value?.FirstOrDefault()?.NumProjet;
        }
    }

}
