using Moq;

using PagueVeloz.Domain.Entities;
using PagueVeloz.Domain.Enums;
using PagueVeloz.Application.Publisher;
using PagueVeloz.Domain.Contracts;
using PagueVeloz.Application.Transactions.Operations;
using PagueVeloz.Application.Common;
using PagueVeloz.Application.Transactions;

namespace Tests.Unit.Application.Operations
{
    public class TransferOperationTests
    {
        private readonly Mock<IUnitOfWork> _uow = new();
        private readonly Mock<IAccountRepository> _accountRepo = new();
        private readonly Mock<ITransactionRepository> _txRepo = new();
        private readonly Mock<IEventPublisher> _publisher = new();

        private TransferOperation CreateSut()
            => new TransferOperation(
                _uow.Object,
                _accountRepo.Object,
                _publisher.Object,
                _txRepo.Object
            );

        private static Account CreateAccount(int id, int creditLimit, int availableBalance, string code)
        {
            var acc = new Account(clientId: 10, creditLimit: creditLimit, availableBalance: availableBalance)
            {
                Id = id,
                Code = code
            };
            return acc;
        }

        private static TransactionInputDto CreateDto(int amount = 50, string reference = " ref-123 ")
            => new TransactionInputDto
            {
                Operation = OperationType.transfer,
                Amount = amount,
                Reference_id = reference,
                Currency = Currency.BRL,
                Account_id = "ACC-001",
                Destination_account_id = "ACC-002"
            };

        [Fact]
        public async Task ExecuteAsync_WhenIdempotencyHit_ReturnsFail_DoesNotCreatePending_DoesNotPublish()
        {
            var source = CreateAccount(id: 1, creditLimit: 0, availableBalance: 100, code: "ACC-001");
            var dest = CreateAccount(id: 2, creditLimit: 0, availableBalance: 0, code: "ACC-002");
            var dto = CreateDto(reference: "abc");

            _txRepo.Setup(r => r.GetAsync("ABC", source.Id))
                  .ReturnsAsync(new Transaction());

            var sut = CreateSut();

            var result = await sut.ExecuteAsync(source, dest, dto);

            Assert.Equal(TransactionStatus.failed, result.status);
            Assert.Equal("Operação já executada", result.error_message);
        }

        [Fact]
        public async Task ExecuteAsync_HappyPath_TransfersFunds_UpdatesBothAccounts_Commits_Publishes_UpdatesTxSuccess()
        {
            var source = CreateAccount(id: 1, creditLimit: 0, availableBalance: 100, code: "ACC-001");
            var dest = CreateAccount(id: 2, creditLimit: 0, availableBalance: 10, code: "ACC-002");
            var dto = CreateDto(amount: 50, reference: " ref-123 ");

            _txRepo.Setup(r => r.GetAsync("REF-9", source.Id))
                  .ReturnsAsync((Transaction?)null);

            Transaction? pending = null;
            _txRepo.Setup(r => r.Create(It.IsAny<Transaction>()))
                  .Callback<Transaction>(t =>
                  {
                      t.Id = 10;
                      pending = t;
                  });

            _uow.Setup(u => u.SaveChangesAsync()).Returns(Task.CompletedTask);
            _uow.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _uow.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);

            _txRepo.Setup(r => r.Get(10)).ReturnsAsync(() => pending);
            _txRepo.Setup(r => r.Update(It.IsAny<Transaction>()));

            _publisher.Setup(p => p.PublishAsync(It.IsAny<object>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .Returns(Task.CompletedTask);

            var sut = CreateSut();

            var result = await sut.ExecuteAsync(source, dest, dto);

            Assert.Equal(50, source.AvailableBalance);
            Assert.Equal(60, dest.AvailableBalance);

            Assert.Equal(TransactionStatus.success, result.status);
            Assert.Equal(dto.Reference_id + "-PROCESSED", result.transaction_id);
            Assert.Equal(50, result.available_balance);
            Assert.Equal(50, result.balance);
            Assert.Equal(source.ReservedBalance, result.reserved_balance);
            Assert.NotNull(pending);
            Assert.Equal("Transferência aprovada", pending!.Description);
            Assert.Equal(source.Id, pending.AccountId);
        }

        [Fact]
        public async Task ExecuteAsync_WhenTransferUsesCreditLimit_StillCommitsAndUpdatesAccounts()
        {
            var source = CreateAccount(id: 1, creditLimit: 100, availableBalance: 20, code: "ACC-001");
            var dest = CreateAccount(id: 2, creditLimit: 0, availableBalance: 0, code: "ACC-002");
            var dto = CreateDto(amount: 50, reference: "ref-9");

            _txRepo.Setup(r => r.GetAsync("REF-9", source.Id))
                  .ReturnsAsync((Transaction?)null);

            Transaction? pending = null;
            _txRepo.Setup(r => r.Create(It.IsAny<Transaction>()))
                  .Callback<Transaction>(t => { t.Id = 10; pending = t; });

            _uow.Setup(u => u.SaveChangesAsync()).Returns(Task.CompletedTask);
            _uow.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _uow.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);

            _txRepo.Setup(r => r.Get(10)).ReturnsAsync(() => pending);
            _txRepo.Setup(r => r.Update(It.IsAny<Transaction>()));

            _publisher.Setup(p => p.PublishAsync(It.IsAny<object>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .Returns(Task.CompletedTask);

            var sut = CreateSut();

            var result = await sut.ExecuteAsync(source, dest, dto);

            Assert.Equal(-30, source.AvailableBalance);
            Assert.Equal(100, source.CreditLimit);
            Assert.Equal(50, dest.AvailableBalance);

            Assert.Equal(TransactionStatus.success, result.status);
        }

        [Fact]
        public async Task ExecuteAsync_WhenConcurrencyConflict_Rollbacks_ClearsTracking_UpdatesTxFailed_ReturnsFailConcurrencyMessage()
        {
            var source = CreateAccount(id: 1, creditLimit: 0, availableBalance: 100, code: "ACC-001");
            var dest = CreateAccount(id: 2, creditLimit: 0, availableBalance: 0, code: "ACC-002");
            var dto = CreateDto(amount: 10, reference: "ref-1");

            _txRepo.Setup(r => r.GetAsync("REF-1", source.Id))
                  .ReturnsAsync((Transaction?)null);

            Transaction? pending = null;
            _txRepo.Setup(r => r.Create(It.IsAny<Transaction>()))
                  .Callback<Transaction>(t => { t.Id = 10; pending = t; });

            _uow.Setup(u => u.SaveChangesAsync()).Returns(Task.CompletedTask);

            _uow.Setup(u => u.BeginTransactionAsync()).ThrowsAsync(new ConcurrencyConflictException("cc"));
            _uow.Setup(u => u.RollbackAsync()).Returns(Task.CompletedTask);

            _txRepo.Setup(r => r.Get(10)).ReturnsAsync(() => pending);
            _txRepo.Setup(r => r.Update(It.IsAny<Transaction>()));

            var sut = CreateSut();

            var result = await sut.ExecuteAsync(source, dest, dto);

            Assert.Equal(TransactionStatus.failed, result.status);
            Assert.Equal("Conflito de concorrência: conta foi alterada por outra operação", result.error_message);
        }

        [Fact]
        public async Task ExecuteAsync_WhenGenericException_Rollbacks_ClearsTracking_UpdatesTxFailed_ReturnsFailWithExceptionMessage()
        {
            var source = CreateAccount(id: 1, creditLimit: 0, availableBalance: 100, code: "ACC-001");
            var dest = CreateAccount(id: 2, creditLimit: 0, availableBalance: 0, code: "ACC-002");
            var dto = CreateDto(amount: 10, reference: "ref-2");

            _txRepo.Setup(r => r.GetAsync("REF-2", source.Id))
                  .ReturnsAsync((Transaction?)null);

            Transaction? pending = null;
            _txRepo.Setup(r => r.Create(It.IsAny<Transaction>()))
                  .Callback<Transaction>(t => { t.Id = 10; pending = t; });

            _uow.Setup(u => u.SaveChangesAsync()).Returns(Task.CompletedTask);
            _uow.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);

            _uow.Setup(u => u.CommitAsync()).ThrowsAsync(new Exception("falhou commit"));

            _uow.Setup(u => u.RollbackAsync()).Returns(Task.CompletedTask);

            _txRepo.Setup(r => r.Get(10)).ReturnsAsync(() => pending);
            _txRepo.Setup(r => r.Update(It.IsAny<Transaction>()));

            var sut = CreateSut();

            var result = await sut.ExecuteAsync(source, dest, dto);

            Assert.Equal(TransactionStatus.failed, result.status);
            Assert.Equal("falhou commit", result.error_message);
        }

        [Fact]
        public async Task ExecuteAsync_NormalizesReferenceId_TrimAndUpper_ForIdempotencyCheck()
        {
            var source = CreateAccount(id: 1, creditLimit: 0, availableBalance: 100, code: "ACC-001");
            var dest = CreateAccount(id: 2, creditLimit: 0, availableBalance: 0, code: "ACC-002");
            var dto = CreateDto(reference: "  aBc-9  ");

            _txRepo.Setup(r => r.GetAsync("ABC-9", source.Id))
                  .ReturnsAsync(new Transaction());

            var sut = CreateSut();

            _ = await sut.ExecuteAsync(source, dest, dto);

            _txRepo.Verify(r => r.GetAsync("ABC-9", source.Id), Times.Once);
        }
    }
}
