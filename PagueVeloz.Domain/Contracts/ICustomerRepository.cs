
using PagueVeloz.Domain.Entities;
using PagueVeloz.Repository.Contracts;

namespace PagueVeloz.Application.Contracts
{
    public interface ICustomerRepository : IBaseRepository<Customer>
    {
        Task<Customer> GetByCodeAsync(string Code);
    }
}
