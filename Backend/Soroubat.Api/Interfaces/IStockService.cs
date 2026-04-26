using Soroubat.Api.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Soroubat.Api.Interfaces
{
    public interface IStockService
    {
      Task<List<StockChantierDto>> GetStockByProjectAsync(string projectNo);  
    }
}