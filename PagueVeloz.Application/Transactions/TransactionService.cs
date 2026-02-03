using PagueVeloz.Application.Contracts;
using PagueVeloz.Application.Transactions.Operations;
using PagueVeloz.Domain.Contracts;
using PagueVeloz.Domain.Entities;
using PagueVeloz.Domain.Enums;

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
                var operation = ValidateAndGetOperation(input.Operation);
                ValidadeReference_id(input.Reference_id);

                if (operation is IReversalOperation reversalOp)
                {
                    return await reversalOp.ExecuteAsync(input.Reference_id);
                }

                ValidadeEntry(input);

                var account = await GetAndValidateAccount(input.Account_id);

                if (operation is ISingleAccountOperation singleOp)
                {
                    return await singleOp.ExecuteAsync(account, input);
                }

                if (operation is ITransferOperation transferOp)
                {
                    var destination = await GetAndValidateAccount(input.Destination_account_id);

                    return await transferOp.ExecuteAsync(account, destination, input);
                }

                throw new InvalidOperationException("Operação inválida");
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

        private void ValidadeEntry(TransactionInputDto input)
        {
            if (input is null)
                throw new Exception("Input não pode ser nulo");

            if (string.IsNullOrWhiteSpace(input.Account_id))
                throw new Exception("Account_id é obrigatório");

            if (input.Amount <= 0)
                throw new Exception("Amount deve ser maior que zero");

            if (input.Currency == default)
                throw new Exception("Currency é obrigatório");

            if (string.IsNullOrWhiteSpace(input.Reference_id))
                throw new Exception("Reference_id é obrigatório");
        }

        private void ValidadeReference_id(string reference_Id)
        {
            if (string.IsNullOrWhiteSpace(reference_Id))
            {
                throw new Exception("Reference_id é obrigatório");
            }
        }

        private async Task<Account?> GetAndValidateAccount(string accountId)
        {
            var account = await _accountRepository.GetByCodeAsync(accountId);

            if (account is null || account.Status != AccountStatus.Active)
            {
                throw new Exception("Conta inválida ou inativa");
            }

            return account;
        }

        private IOperation? ValidateAndGetOperation(OperationType operation)
        {
            var op = _operations.FirstOrDefault(_ => _.Type == operation); 
            if (op == null)
            {
                throw new Exception("Operação inválida");
            }
            return op;
        }
    }
}
