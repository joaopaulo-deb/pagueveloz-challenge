using PagueVeloz.Application.Contracts;
using PagueVeloz.Domain.Entities;
using PagueVeloz.Domain.Enums;
using PagueVeloz.Repository.Repositories;

namespace PagueVeloz.Application.Transactions.Operations
{
    public class DebitOperation : IOperation
    {
        public OperationType Type => OperationType.debit;

        private readonly IAccountRepository _accountRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DebitOperation(
            IUnitOfWork unitOfWork,
            IAccountRepository accountRepository,
            ITransactionRepository transactionRepository)
        {
            _unitOfWork = unitOfWork;
            _accountRepository = accountRepository;
            _transactionRepository = transactionRepository;
        }
        public async Task<TransactionOutputDto> ExecuteAsync(Account account, TransactionInputDto dto)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                account.Debit(dto.Amount);

                var transaction = new Transaction(
                    Enum.Parse<OperationType>(dto.Operation, true),
                    account.Id,
                    dto.Amount,
                    dto.Currency,
                    dto.Reference_id,
                    dto.Metadata?.Description ?? string.Empty);

                _transactionRepository.Create(transaction);
                _accountRepository.Update(account);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                return new TransactionOutputDto
                {
                    transaction_id = dto.Reference_id + "-PROCESSED",
                    status = TransactionStatus.success,
                    balance = account.AvailableBalance,
                    reserved_balance = account.ReservedBalance,
                    available_balance = account.AvailableBalance,
                    timestamp = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                return Fail(dto, ex.Message, account);
            }
        }
        private TransactionOutputDto Fail(TransactionInputDto dto, string message, Account? account = null)
        {
            return new TransactionOutputDto
            {
                transaction_id = $"{dto.Reference_id}-PROCESSED",
                status = TransactionStatus.failed,
                balance = account?.AvailableBalance ?? 0,
                reserved_balance = account?.ReservedBalance ?? 0,
                available_balance = account?.AvailableBalance ?? 0,
                timestamp = DateTime.UtcNow,
                error_message = message
            };
        }
    }
}
