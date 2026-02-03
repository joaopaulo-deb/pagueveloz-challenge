
using PagueVeloz.Domain.Entities;

namespace PagueVeloz.Domain.Contracts
{
    public interface IClientRepository : IBaseRepository<Client>
    {
        Task<Client> GetByCodeAsync(string Code);
    }
}
