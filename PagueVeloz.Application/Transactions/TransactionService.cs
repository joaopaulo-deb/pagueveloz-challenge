using PagueVeloz.Application.Common;
using PagueVeloz.Application.Contracts;
using PagueVeloz.Application.Customers;
using PagueVeloz.Application.Transactions.Operations;
using PagueVeloz.Domain.Entities;
using PagueVeloz.Domain.Enums;
using PagueVeloz.Repository.Repositories;
using System.Security.Principal;

namespace PagueVeloz.Application.Transactions
{
    public class TransactionService : ITransactionService
    {
        private readonly IEnumerable<IOperation> _operations;
        private readonly IAccountRepository _accountRepository;

        public TransactionService(IEnumerable<IOperation> operations, IAccountRepository accountRepository)
        {
            _operations = operations;
            _accountRepository = accountRepository;
        }

        public async Task<TransactionOutputDto> Handle(TransactionInputDto input)
        {
            try
            {
                ValidadeEntry(input);
                var operation = ValidateAndGetOperation(input.Operation);
                var account = await GetAndValidateAccount(input.Account_id);

                return await operation!.ExecuteAsync(account!, input);
            }
            catch (Exception ex)
            {
                return new TransactionOutputDto
                {
                    transaction_id = input.Reference_id,
                    status = TransactionStatus.failed,
                    timestamp = DateTime.UtcNow,
                    error_message = ex.Message
                };
            }
        }

        private void ValidadeEntry(TransactionInputDto dto)
        {
            if (dto.Amount <= 0)
            {
                throw new Exception("Amount deve ser maior que zero");
            }

            if (string.IsNullOrWhiteSpace(dto.Reference_id))
            {
                throw new Exception("Reference_id é obrigatório");
            }

            if (string.IsNullOrWhiteSpace(dto.Currency))
            {
                throw new Exception("Currency é obrigatório");
            }
        }

        private async Task<Account?> GetAndValidateAccount(string accountId)
        {
            var account = await _accountRepository.GetByCodeAsync(accountId);

            if (account is null || account.Status != StatusAccount.Active)
            {
                throw new Exception("Conta inválida ou inativa");
            }

            return account;
        }

        private IOperation? ValidateAndGetOperation(string operation)
        {
            if (!Enum.TryParse<OperationType>(operation, true, out var operationType))
            {
                throw new Exception("Operação inválida");
            }

            return _operations.FirstOrDefault(_ => _.Type == operationType);
        }
    }
}
