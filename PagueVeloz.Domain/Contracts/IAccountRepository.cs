
using PagueVeloz.Domain.Entities;

namespace PagueVeloz.Application.Contracts
{
    public interface IAccountRepository : IBaseRepository<Account>
    {
        Task<Account?> GetByCodeAsync(string Code);
    }
}
