using Moq;
using PagueVeloz.Domain.Entities;
using PagueVeloz.Domain.Enums;
using PagueVeloz.Application.Publisher;
using PagueVeloz.Domain.Contracts;
using PagueVeloz.Application.Transactions.Operations;
using PagueVeloz.Application.Transactions;
using PagueVeloz.Application.Common;

namespace Tests.Unit.Application.Operations
{
    public class ReserveOperationTests
    {
        private readonly Mock<IUnitOfWork> _uow = new();
        private readonly Mock<IAccountRepository> _accountRepo = new();
        private readonly Mock<ITransactionRepository> _txRepo = new();
        private readonly Mock<IEventPublisher> _publisher = new();

        private ReserveOperation CreateSut()
            => new ReserveOperation(
                _uow.Object,
                _accountRepo.Object,
                _publisher.Object,
                _txRepo.Object
            );

        private static Account CreateAccount(int id = 1, int availableBalance = 100)
        {
            var acc = new Account(clientId: 10, creditLimit: 0, availableBalance: availableBalance)
            {
                Id = id,
                Code = $"ACC-{id:D3}"
            };
            return acc;
        }

        private static TransactionInputDto CreateDto(int amount = 30, string reference = " ref-123 ")
            => new TransactionInputDto
            {
                Operation = OperationType.reserve,
                Amount = amount,
                Reference_id = reference,
                Currency = Currency.BRL,
                Account_id = "ACC-001",
                Destination_account_id = "ACC-002"
            };

        [Fact]
        public async Task ExecuteAsync_WhenIdempotencyHit_ReturnsFail_DoesNotCreatePending_DoesNotPublish()
        {
            var account = CreateAccount(id: 1);
            var dto = CreateDto(reference: "abc");

            _txRepo.Setup(r => r.GetAsync("ABC", account.Id))
                  .ReturnsAsync(new Transaction());

            var sut = CreateSut();

            var result = await sut.ExecuteAsync(account, dto);

            Assert.Equal(TransactionStatus.failed, result.status);
            Assert.Equal("Operação já executada", result.error_message);
        }

        [Fact]
        public async Task ExecuteAsync_HappyPath_ReservesFunds_Commits_Publishes_UpdatesTxSuccess()
        {
            var account = CreateAccount(id: 1, availableBalance: 100);
            var dto = CreateDto(amount: 30, reference: " ref-123 ");

            _txRepo.Setup(r => r.GetAsync("REF-123", account.Id))
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

            var result = await sut.ExecuteAsync(account, dto);

            Assert.Equal(TransactionStatus.success, result.status);
            Assert.Equal(dto.Reference_id + "-PROCESSED", result.transaction_id);

            Assert.Equal(70, result.available_balance);
            Assert.Equal(70, result.balance);
            Assert.Equal(30, result.reserved_balance);
            Assert.NotNull(pending);
            Assert.Equal("Reserva para pagamento", pending!.Description);
        }

        [Fact]
        public async Task ExecuteAsync_WhenConcurrencyConflict_Rollbacks_ClearsTracking_UpdatesTxFailed_ReturnsFailConcurrencyMessage()
        {
            var account = CreateAccount(id: 1);
            var dto = CreateDto(amount: 10, reference: "ref-1");

            _txRepo.Setup(r => r.GetAsync("REF-1", account.Id))
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

            var result = await sut.ExecuteAsync(account, dto);

            Assert.Equal(TransactionStatus.failed, result.status);
            Assert.Equal("Conflito de concorrência: conta foi alterada por outra operação", result.error_message);
        }

        [Fact]
        public async Task ExecuteAsync_WhenGenericException_Rollbacks_ClearsTracking_UpdatesTxFailed_ReturnsFailWithExceptionMessage()
        {
            var account = CreateAccount(id: 1);
            var dto = CreateDto(amount: 10, reference: "ref-2");

            _txRepo.Setup(r => r.GetAsync("REF-9", account.Id))
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

            var result = await sut.ExecuteAsync(account, dto);

            Assert.Equal(TransactionStatus.failed, result.status);
            Assert.Equal("falhou commit", result.error_message);
        }

        [Fact]
        public async Task ExecuteAsync_NormalizesReferenceId_TrimAndUpper_ForIdempotencyCheck()
        {
            var account = CreateAccount(id: 1);
            var dto = CreateDto(reference: "  aBc-9  ");

            _txRepo.Setup(r => r.GetAsync("ABC-9", account.Id))
                  .ReturnsAsync(new Transaction());

            var sut = CreateSut();

            _ = await sut.ExecuteAsync(account, dto);

            _txRepo.Verify(r => r.GetAsync("ABC-9", account.Id), Times.Once);
        }
    }
}
