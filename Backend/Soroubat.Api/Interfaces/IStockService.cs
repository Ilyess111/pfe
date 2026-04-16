using Soroubat.Api.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Soroubat.Api.Services
{
    public interface IStockService
    {
        Task<List<StockChantierDto>> GetStockByChefEmailAsync(string email);
    }
}