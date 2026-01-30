using PagueVeloz.Application.Common;
using PagueVeloz.Application.Customers;

namespace PagueVeloz.Application.Contracts
{
    public interface ICustomerService
    {
        Task<Response<CustomerCreateOutputDto>> Create(CustomerCreateInputDto input);
    }
}
