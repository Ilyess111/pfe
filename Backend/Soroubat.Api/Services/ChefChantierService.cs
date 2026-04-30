using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Soroubat.Api.Interfaces;
using Soroubat.Api.Models;

namespace Soroubat.Api.Services
{
    /// <summary>
    /// Service d'accès aux données Chef Chantier dans Business Central.
    /// Utilisé exclusivement lors de l'authentification pour résoudre le numéro de projet.
    /// </summary>
    public class ChefChantierService : BaseService, IChefChantierService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ChefChantierService> _logger;

        public ChefChantierService(HttpClient httpClient, ILogger<ChefChantierService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<string?> GetJobNoByEmailAsync(string email)
        {
            // Filtre sur l'e-mail ET sur le flag actif — un compte désactivé ne doit pas pouvoir se connecter
            var url = $"chefsChantier?$filter=email eq '{ODataEncode(email)}' and actif eq true";
            _logger.LogInformation("[ChefChantier] GET {Url}", url); // ca sert à afficher dans les logs la requête exacte envoyée à BC

            var response = await _httpClient.GetAsync(url); // httpclient appelle l'url de base concaténée avec le paramétre de la méthode getasync

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("[ChefChantier] Erreur BC lors de la résolution du projet pour '{Email}' — HTTP {Status}",
                    email, (int)response.StatusCode);
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<BCResponse<ChefChantierDto>>();
            var chef   = result?.Value?.FirstOrDefault();

            if (chef == null)
            {
                _logger.LogWarning("[ChefChantier] Aucun chef de chantier actif trouvé pour '{Email}'.", email);
                return null;
            }

            _logger.LogInformation("[ChefChantier] Projet '{ProjectNo}' résolu pour '{Email}'.", chef.NumProjet, email);
            return chef.NumProjet;
        }
    }
}