using PagueVeloz.Application.Common;
using PagueVeloz.Application.Contracts;
using PagueVeloz.Application.Customers;
using PagueVeloz.Domain.Entities;
using PagueVeloz.Repository.Contracts;
using PagueVeloz.Repository.Repositories;

namespace PagueVeloz.Application.Accounts
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AccountService(IUnitOfWork unitOfWork, IAccountRepository accountRepository, ICustomerRepository customerRepository)
        {
            _unitOfWork = unitOfWork;
            _accountRepository = accountRepository;
            _customerRepository = customerRepository;
        }
        
        public async Task<Response<AccountCreateOutputDto>> Create(AccountCreateInputDto input)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var customer = await GetOrCreateCustomerAsync(input.ClientId);
                await _unitOfWork.SaveChangesAsync();
                        
                var account = CreateAccount(customer.Id, input );
                await _unitOfWork.SaveChangesAsync();

                account.GenerateCode();
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitAsync();

                return Response<AccountCreateOutputDto>.Ok(ToOutputDto(account, customer), "Conta criado com sucesso");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new ApplicationException("Erro ao criar conta", ex);
            }
        }

        private async Task<Customer> GetOrCreateCustomerAsync(string clientId)
        {
            var customer = await _customerRepository.GetByCodeAsync(clientId);

            if (customer != null)
                return customer;

            customer = new Customer(clientId);
            _customerRepository.Create(customer);

            return customer;
        }

        private Account CreateAccount(int customerId, AccountCreateInputDto input)
        {
            var account = new Account(
                customerId,
                input.CreditLimit,
                input.AvailableBalance
            );

            _accountRepository.Create(account);
            return account;
        }


        private AccountCreateOutputDto ToOutputDto(Account account, Customer customer)
        {
            return new AccountCreateOutputDto
            {
                AccountId = account.Code,
                ClientId = customer.Code
            };
        }
    }
}
