
using PagueVeloz.Domain.Entities;

namespace PagueVeloz.Domain.Contracts
{
    public interface ITransactionRepository : IBaseRepository<Transaction>
    {
        Task<Transaction?> GetAsync(string referenceId, int? accountId);
    }
}
