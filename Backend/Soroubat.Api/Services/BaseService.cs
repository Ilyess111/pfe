using System.Net;
using System.Text.Json;
using Soroubat.Api.Models;

namespace Soroubat.Api.Services
{
    /// <summary>
    /// Classe de base pour tous les services qui communiquent avec Business Central.
    /// Centralise la gestion des erreurs HTTP et les utilitaires partagés.
    /// </summary>
    public abstract class BaseService
    {
        /// <summary>
        /// Lit la réponse d'erreur BC et lance une exception avec un message lisible.
        /// Gère les cas : body vide, JSON valide, JSON invalide.
        /// </summary>
        protected async Task HandleErrorResponse(HttpResponseMessage response)
        {
            var errorContent = await response.Content.ReadAsStringAsync();

            // Body vide → message générique selon le code HTTP
            if (string.IsNullOrWhiteSpace(errorContent))
            {
                throw new Exception(response.StatusCode switch
                {
                    HttpStatusCode.NotFound
                        => "Ressource introuvable dans Business Central (404). Vérifiez que l'extension est déployée.",
                    HttpStatusCode.Unauthorized
                        => "Authentification refusée par Business Central (401).",
                    HttpStatusCode.Forbidden
                        => "Accès refusé par Business Central (403).",
                    HttpStatusCode.UnprocessableEntity
                        => "Données invalides rejetées par Business Central (422).",
                    _
                        => $"Erreur Business Central inattendue. Code HTTP : {(int)response.StatusCode}."
                });
            }

            try
            {
                var bcError = JsonSerializer.Deserialize<BCResponseError>(errorContent);
                throw new Exception(bcError?.Error?.Message ?? errorContent);
            }
            catch (JsonException)
            {
                throw new Exception(
                    $"Réponse Business Central illisible (JSON invalide). " +
                    $"Code HTTP : {(int)response.StatusCode}. Contenu : {errorContent}");
            }
        }

        /// <summary>
        /// Échappe les apostrophes dans une valeur utilisée dans un filtre OData.
        /// Exemple : "L'Oréal" → "L''Oréal"
        /// </summary>
        protected static string ODataEncode(string value) =>
            value?.Replace("'", "''") ?? string.Empty;
    }
}