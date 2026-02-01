
using PagueVeloz.Domain.Entities;
using PagueVeloz.Repository.Contracts;

namespace PagueVeloz.Application.Contracts
{
    public interface IClientRepository : IBaseRepository<Client>
    {
        Task<Client> GetByCodeAsync(string Code);
    }
}
