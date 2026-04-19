namespace Soroubat.Api.Interfaces
{
    public interface IAuthService
    {
        string GenerateJwtToken(string email, string projectNo);
        Task<string> AuthenticateAsync(string email, string password);
    }
}