using PagueVeloz.Application.Common;
using PagueVeloz.Application.Publisher;
using PagueVeloz.Domain.Contracts;
using PagueVeloz.Domain.Entities;
using PagueVeloz.Domain.Enums;

namespace PagueVeloz.Application.Transactions.Operations
{
    public class ReversalOperation : IReversalOperation
    {
        private readonly IAccountRepository _accountRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEventPublisher _publisher;
        private string DESCRIPTION = "Transação invertida";

        public ReversalOperation(
            IUnitOfWork unitOfWork,
            IAccountRepository accountRepository,
            IEventPublisher publisher,
            ITransactionRepository transactionRepository)
        {
            _unitOfWork = unitOfWork;
            _accountRepository = accountRepository;
            _transactionRepository = transactionRepository;
            _publisher = publisher;

        }

        public OperationType Type => OperationType.reversal;

        public async Task<TransactionOutputDto> ExecuteAsync(string referenceId)
        {
            var reference_id = referenceId.Trim().ToUpper();

            var transaction = await _transactionRepository.GetAsync(referenceId, null);

            if (transaction == null)
            {
                return Fail(referenceId, "Operação não encontrada");
            }

            var account = await _accountRepository.Get(transaction.AccountId);

            if (account == null)
            {
                return Fail(referenceId, "Conta da transação não encontrada");
            }

            var dto = BuildDto(account, transaction, reference_id);

            try
            {
                await _unitOfWork.BeginTransactionAsync();
                

                var newTransaction = await CreatePendingTransactionAsync(account, dto, OperationType.reversal, referenceId);
                ApplyReversal(account, transaction);

                _accountRepository.Update(account);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                await PublishTransactionProcessedEventAsync(dto, account);
                await UpdateTransactionStatusAsync(newTransaction.Id, TransactionStatus.success);

                return new TransactionOutputDto
                {
                    transaction_id = referenceId + "-PROCESSED",
                    status = TransactionStatus.success,
                    balance = account.AvailableBalance,
                    reserved_balance = account.ReservedBalance,
                    available_balance = account.AvailableBalance,
                    timestamp = DateTime.UtcNow
                };
            }
            catch (ConcurrencyConflictException ex)
            {
                await _unitOfWork.RollbackAsync();
                _unitOfWork.ClearTracking();
                await UpdateTransactionStatusAsync(transaction.Id, TransactionStatus.failed, "Conflito de concorrência");

                return Fail(referenceId, "Conflito de concorrência: conta foi alterada por outra operação", account);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _unitOfWork.ClearTracking();
                await UpdateTransactionStatusAsync(transaction.Id, TransactionStatus.failed, ex.Message);
                return Fail(referenceId, ex.Message, account);
            }
        }

        private async Task<Transaction> CreatePendingTransactionAsync(Account account,TransactionInputDto dto,OperationType operation, string referenceId)
        {
            var transaction = new Transaction(
                operation,
                account.Id,
                dto.Amount,
                TransactionStatus.pending,
                dto.Currency,
                $"{referenceId}-R",
                DESCRIPTION
            );

            _transactionRepository.Create(transaction);
            await _unitOfWork.SaveChangesAsync();

            return transaction;
        }

        private async Task UpdateTransactionStatusAsync(int transactionId, TransactionStatus status, string? errorMessage = null)
        {
            var transaction = await _transactionRepository.Get(transactionId);
            if (transaction == null) return;

            transaction.Status = status;

            if (errorMessage is not null)
            {
                transaction.Description = errorMessage;
            }

            _transactionRepository.Update(transaction);
            await _unitOfWork.SaveChangesAsync();
        }

        private static TransactionInputDto BuildDto(Account account, Transaction transaction, string referenceId)
        {
            return new TransactionInputDto
            {
                Operation = OperationType.reversal,
                Amount = transaction.Amount,
                Account_id = account.Code,
                Currency = Currency.BRL,
                Reference_id = referenceId
            };
        }

        private static void ApplyReversal(Account account, Transaction originalTransaction)
        {
            switch (originalTransaction.Operation)
            {
                case OperationType.credit:
                    account.Debit(originalTransaction.Amount);
                    break;

                case OperationType.debit:
                    account.Credit(originalTransaction.Amount);
                    break;

                case OperationType.reserve:
                    account.RevertReserve(originalTransaction.Amount);
                    break;

                case OperationType.capture:
                    account.RevertCapture(originalTransaction.Amount);
                    break;

                case OperationType.transfer:
                    // TODO
                    throw new NotImplementedException("Reversão de transferência ainda não implementada.");

                default:
                    throw new InvalidOperationException($"Operação não suportada para reversão: {originalTransaction.Operation}");
            }
        }

        private TransactionOutputDto Fail(string referenceId, string message, Account? account = null)
        {
            return new TransactionOutputDto
            {
                transaction_id = $"{referenceId}-PROCESSED",
                status = TransactionStatus.failed,
                balance = account?.AvailableBalance ?? 0,
                reserved_balance = account?.ReservedBalance ?? 0,
                available_balance = account?.AvailableBalance ?? 0,
                timestamp = DateTime.UtcNow,
                error_message = message
            };
        }

        private async Task PublishTransactionProcessedEventAsync(TransactionInputDto dto, Account account, string queueOrExchange = "transactions.processed")
        {
            var evt = new TransactionProcessedEvent(
                TransactionId: dto.Reference_id,
                AccountId: account.Id,
                Operation: dto.Operation,
                Status: "success",
                Amount: dto.Amount,
                Currency: dto.Currency,
                Timestamp: DateTime.UtcNow
            );

            await _publisher.PublishAsync(evt, queueOrExchange);
        }
    }
}
