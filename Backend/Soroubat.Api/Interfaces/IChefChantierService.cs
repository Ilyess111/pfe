namespace Soroubat.Api.Interfaces
{
    public interface IChefChantierService
    {
        Task<string> GetJobNoByEmailAsync(string email);
    }
}