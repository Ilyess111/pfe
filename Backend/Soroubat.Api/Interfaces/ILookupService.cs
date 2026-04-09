using System.Text.Json;

namespace Soroubat.Api.Services
{
    public interface ILookupService
    {
        Task<JsonElement> GetLookupDataAsync(string entitySetName, string? filter = null);
    }
}