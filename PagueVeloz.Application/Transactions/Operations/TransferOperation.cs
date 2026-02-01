using PagueVeloz.Application.Contracts;
using PagueVeloz.Domain.Entities;
using PagueVeloz.Domain.Enums;
using PagueVeloz.Repository.Repositories;

namespace PagueVeloz.Application.Transactions.Operations
{
    public class TransferOperation : ITransferOperation
    {
        private readonly IAccountRepository _accountRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IUnitOfWork _unitOfWork;
        private string DESCRIPTION = "Transferência aprovada";

        public TransferOperation(
            IUnitOfWork unitOfWork,
            IAccountRepository accountRepository,
            ITransactionRepository transactionRepository)
        {
            _unitOfWork = unitOfWork;
            _accountRepository = accountRepository;
            _transactionRepository = transactionRepository;

        }

        public OperationType Type => OperationType.transfer;

        public async Task<TransactionOutputDto> ExecuteAsync(Account sourceAccount, Account destinationAccount, TransactionInputDto dto)
        {
            var referenceId = dto.Reference_id.Trim().ToUpper();

            var idempot = await _transactionRepository.GetByAccountAndReferenceIdAsync(sourceAccount.Id, referenceId);

            if (idempot != null)
            {
                return Fail(dto, "Operação já executada", sourceAccount);
            }

            var transaction = await CreatePendingTransactionAsync(sourceAccount, destinationAccount, dto, dto.Operation, referenceId);

            try
            {
                await _unitOfWork.BeginTransactionAsync();
                sourceAccount.TransferTo(destinationAccount, dto.Amount);
                _accountRepository.Update(sourceAccount);
                _accountRepository.Update(destinationAccount);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                await UpdateTransactionStatusAsync(transaction.Id, TransactionStatus.success);

                return new TransactionOutputDto
                {
                    transaction_id = dto.Reference_id + "-PROCESSED",
                    status = TransactionStatus.success,
                    balance = sourceAccount.AvailableBalance,
                    reserved_balance = sourceAccount.ReservedBalance,
                    available_balance = sourceAccount.AvailableBalance,
                    timestamp = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                await UpdateTransactionStatusAsync(transaction.Id, TransactionStatus.failed, ex.Message);
                return Fail(dto, ex.Message, sourceAccount);
            }
        }

        private async Task<Transaction> CreatePendingTransactionAsync(Account sourceAccount, Account destinationAccount, TransactionInputDto dto, OperationType operation, string referenceId)
        {
            var transaction = new Transaction(
                operation,
                sourceAccount.Id,
                dto.Amount,
                TransactionStatus.pending,
                dto.Currency,
                referenceId,
                DESCRIPTION
            );

            // se você guarda conta destino na transaction
            //transaction.SetDestinationAccount(destinationAccount.Id);

            _transactionRepository.Create(transaction);
            await _unitOfWork.SaveChangesAsync();

            return transaction;
        }

        private async Task UpdateTransactionStatusAsync( int transactionId, TransactionStatus status, string? errorMessage = null)
        {
            var transaction = await _transactionRepository.Get(transactionId);
            if (transaction == null) return;

            transaction.Status = status;

            _transactionRepository.Update(transaction);
            await _unitOfWork.SaveChangesAsync();
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
