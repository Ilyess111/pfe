using System.Text.Json.Serialization;

namespace Soroubat.Api.Models
{
    /// <summary>
    /// Enveloppe générique des réponses OData de Business Central.
    /// Utilisée pour désérialiser les collections retournées par BC (propriété "value").
    /// </summary>
    public class BCResponse<T>
    {
        [JsonPropertyName("@odata.context")]
        public string Context { get; set; } = string.Empty;

        [JsonPropertyName("value")]
        public List<T> Value { get; set; } = new List<T>();
    }

    /// <summary>
    /// Enveloppe des réponses d'erreur Business Central.
    /// BC retourne les erreurs sous la forme : { "error": { "code": "...", "message": "..." } }
    /// </summary>
    public class BCResponseError
    {
        [JsonPropertyName("error")]
        public BCResponseErrorDetail? Error { get; set; }
    }

    /// <summary>
    /// Détail d'une erreur Business Central.
    /// </summary>
    public class BCResponseErrorDetail
    {
        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;
    }
}