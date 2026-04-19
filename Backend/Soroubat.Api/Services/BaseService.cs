using System.Text.Json;
using System.Net.Http.Json;
using Soroubat.Api.Models;

public abstract class BaseService 
{
    protected async Task HandleErrorResponse(HttpResponseMessage response)
    {
        var statusCode = (int)response.StatusCode;
        var errorContent = await response.Content.ReadAsStringAsync();

        // Cas 404 — Ressource introuvable (GUID inexistant, EntitySet mal nommé, etc.)
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            throw new Exception($"404 - Ressource introuvable dans Business Central. URL : {response.RequestMessage?.RequestUri}");
        }

        // Cas général — on tente de parser le message d'erreur OData de BC
        try 
        {
            var bcError = JsonSerializer.Deserialize<BCResponseError>(errorContent);
            var message = bcError?.Error?.Message ?? errorContent;
            throw new Exception($"{statusCode} - {message}");
        } 
        catch (JsonException) 
        {
            // Le corps n'est pas du JSON (HTML, texte brut, vide...)
            throw new Exception($"{statusCode} - Réponse BC illisible. Contenu brut : {errorContent}");
        }
    }
}