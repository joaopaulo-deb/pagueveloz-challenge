using PagueVeloz.Application.Common;
using PagueVeloz.Application.Publisher;
using PagueVeloz.Domain.Contracts;
using PagueVeloz.Domain.Entities;
using PagueVeloz.Domain.Enums;

namespace PagueVeloz.Application.Transactions.Operations
{
    public class CreditOperation : ISingleAccountOperation
    {
        private readonly IAccountRepository _accountRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEventPublisher _publisher;
        public IEventRepository _eventRepository;
        private string DESCRIPTION = "Depósito Inicial";

        public CreditOperation(
            IUnitOfWork unitOfWork,
            IAccountRepository accountRepository,
            ITransactionRepository transactionRepository,
            IEventPublisher publisher,
            IEventRepository eventRepository)
        {
            _unitOfWork = unitOfWork;
            _accountRepository = accountRepository;
            _transactionRepository = transactionRepository;
            _publisher = publisher;
            _eventRepository = eventRepository;

        }

        public OperationType Type => OperationType.credit;

        public async Task<TransactionOutputDto> ExecuteAsync(Account account, TransactionInputDto dto)
        {
            var referenceId = dto.Reference_id.Trim().ToUpper();

            var idempot = await _transactionRepository.GetAsync(referenceId, account.Id);

            if (idempot != null)
            {
                return Fail(dto, "Operação já executada", account);
            }

            var transaction = await CreatePendingTransactionAsync(account, dto, dto.Operation, referenceId);

            try
            {
                await _unitOfWork.BeginTransactionAsync();
                account.Credit(dto.Amount);
                _accountRepository.Update(account);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                await PublishTransactionProcessedEventAsync(dto, account);
                await UpdateTransactionStatusAsync(transaction.Id, TransactionStatus.success);

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
            catch (ConcurrencyConflictException ex)
            {
                await _unitOfWork.RollbackAsync();
                _unitOfWork.ClearTracking();
                await UpdateTransactionStatusAsync(transaction.Id, TransactionStatus.failed, "Conflito de concorrência");

                return Fail(dto, "Conflito de concorrência: conta foi alterada por outra operação", account);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _unitOfWork.ClearTracking();
                await UpdateTransactionStatusAsync(transaction.Id, TransactionStatus.failed, ex.Message);
                return Fail(dto, ex.Message, account);
            }
        }

        private async Task<Transaction> CreatePendingTransactionAsync(Account account,TransactionInputDto dto, OperationType operation, string referenceId)
        {
            var transaction = new Transaction(
                operation,
                account.Id,
                dto.Amount,
                TransactionStatus.pending,
                dto.Currency,
                referenceId,
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
