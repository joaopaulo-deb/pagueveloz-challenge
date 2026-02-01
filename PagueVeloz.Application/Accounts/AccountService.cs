using PagueVeloz.Application.Common;
using PagueVeloz.Application.Contracts;
using PagueVeloz.Domain.Entities;

namespace PagueVeloz.Application.Accounts
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IClientRepository _clientRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AccountService(IUnitOfWork unitOfWork, IAccountRepository accountRepository, IClientRepository clientRepository)
        {
            _unitOfWork = unitOfWork;
            _accountRepository = accountRepository;
            _clientRepository = clientRepository;
        }
        
        public async Task<Response<AccountCreateOutputDto>> Create(AccountCreateInputDto input)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var client = await GetOrCreateClientAsync(input.ClientId);
                await _unitOfWork.SaveChangesAsync();
                        
                var account = CreateAccount(client.Id, input );
                await _unitOfWork.SaveChangesAsync();

                account.GenerateCode();
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitAsync();

                return Response<AccountCreateOutputDto>.Ok(ToOutputDto(account, client), "Conta criado com sucesso");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new ApplicationException("Erro ao criar conta", ex);
            }
        }

        private async Task<Client> GetOrCreateClientAsync(string clientId)
        {
            var client = await _clientRepository.GetByCodeAsync(clientId);

            if (client != null)
                return client;

            client = new Client(clientId);
            _clientRepository.Create(client);

            return client;
        }

        private Account CreateAccount(int clientId, AccountCreateInputDto input)
        {
            var account = new Account(
                clientId,
                input.CreditLimit,
                input.AvailableBalance
            );

            _accountRepository.Create(account);
            return account;
        }


        private AccountCreateOutputDto ToOutputDto(Account account, Client client)
        {
            return new AccountCreateOutputDto
            {
                AccountId = account.Code,
                ClientId = client.Code
            };
        }
    }
}
