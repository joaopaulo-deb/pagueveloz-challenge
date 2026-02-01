
using PagueVeloz.Domain.Entities;

namespace PagueVeloz.Application.Contracts
{
    public interface IClientRepository : IBaseRepository<Client>
    {
        Task<Client> GetByCodeAsync(string Code);
    }
}
