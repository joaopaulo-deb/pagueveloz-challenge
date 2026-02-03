
using PagueVeloz.Domain.Entities;

namespace PagueVeloz.Domain.Contracts
{
    public interface IAccountRepository : IBaseRepository<Account>
    {
        Task<Account?> GetByCodeAsync(string Code);
    }
}
