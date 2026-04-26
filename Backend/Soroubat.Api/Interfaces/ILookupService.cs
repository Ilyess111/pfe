using System.Text.Json;

namespace Soroubat.Api.Interfaces
{
    public interface ILookupService
    {
        Task<JsonElement> GetLookupDataAsync(string entitySetName, string numProjet, string? additionalFilter = null);
    }
}