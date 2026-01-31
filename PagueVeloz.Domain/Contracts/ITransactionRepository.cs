
using PagueVeloz.Domain.Entities;
using PagueVeloz.Repository.Contracts;

namespace PagueVeloz.Application.Contracts
{
    public interface ITransactionRepository : IBaseRepository<Transaction>
    {
        //Task<Transaction> GetByCodeAsync(string Code);
    }
}
