using System.Text.Json;
using System.Net.Http.Json;
using Soroubat.Api.Models;

public abstract class BaseService 
{
    protected new async Task HandleErrorResponse(HttpResponseMessage response)
    {
        var errorContent = await response.Content.ReadAsStringAsync();

        // ✅ Body vide → message générique selon le code HTTP
        if (string.IsNullOrWhiteSpace(errorContent))
        {
            throw new Exception(response.StatusCode switch
            {
                System.Net.HttpStatusCode.NotFound => "Ressource introuvable dans Business Central (404). Vérifiez l'URL de l'action ou que l'extension est déployée.",
                System.Net.HttpStatusCode.Unauthorized => "Authentification refusée par Business Central (401).",
                System.Net.HttpStatusCode.Forbidden => "Accès refusé par Business Central (403).",
                _ => $"Erreur Business Central inattendue. Code HTTP : {(int)response.StatusCode}."
            });
        }

        try
        {
            var bcError = JsonSerializer.Deserialize<BCResponseError>(errorContent);
            throw new Exception(bcError?.Error?.Message ?? errorContent);
        }
        catch (JsonException)
        {
            throw new Exception($"Réponse de Business Central illisible (Format JSON invalide). Code HTTP {(int)response.StatusCode}. Contenu brut : {errorContent}");
        }
    }
}