
using PagueVeloz.Domain.Entities;

namespace PagueVeloz.Application.Contracts
{
    public interface ITransactionRepository : IBaseRepository<Transaction>
    {
        Task<Transaction?> GetByAccountAndReferenceIdAsync(int accountId, string referenceId);
    }
}
