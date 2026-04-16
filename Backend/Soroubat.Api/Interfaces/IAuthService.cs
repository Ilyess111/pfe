namespace Soroubat.Api.Interfaces
{
    public interface IAuthService
    {
        string GenerateJwtToken(string email);
        Task<string> AuthenticateAsync(string email, string password);
    }
}