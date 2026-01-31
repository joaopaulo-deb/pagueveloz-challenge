using PagueVeloz.Application.Accounts;
using PagueVeloz.Application.Common;
using PagueVeloz.Application.Contracts;
using PagueVeloz.Domain.Entities;
using System.Security.Principal;

namespace PagueVeloz.Application.Customers
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;

        public CustomerService(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<Response<CustomerCreateOutputDto>> Create(CustomerCreateInputDto input)
        {
           try
           {
                var customer = new Customer(input.ClientId);

                _customerRepository.Create(customer);
                await _customerRepository.SaveChangesAsync();

                return Response<CustomerCreateOutputDto>.Ok(ToOutputDto(customer), "Cliente criado com sucesso");
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Erro ao criar cliente", ex);
            }
        }

        private CustomerCreateOutputDto ToOutputDto(Customer customer)
        {
            return new CustomerCreateOutputDto
            {
                ClientId = customer.Code
            };
        }
    }
}
