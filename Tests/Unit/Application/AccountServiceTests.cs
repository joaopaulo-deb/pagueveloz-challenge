
using Moq;
using PagueVeloz.Domain.Entities;
using PagueVeloz.Domain.Contracts;
using PagueVeloz.Application.Accounts;

namespace Tests.Unit.Application
{
    public class AccountServiceTests
    {
        private readonly Mock<IUnitOfWork> _uow = new();
        private readonly Mock<IAccountRepository> _accountRepo = new();
        private readonly Mock<IClientRepository> _clientRepo = new();

        private AccountService CreateSut()
            => new AccountService(_uow.Object, _accountRepo.Object, _clientRepo.Object);

        private static AccountCreateInputDto ValidInput() => new AccountCreateInputDto 
        {
            ClientId = "CLI-001",
            CreditLimit = 50000,
            AvailableBalance = 0
        };
        
        [Fact]
        public async Task Create_ClientExists_ReturnsOkAndCommits()
        {
            var input = ValidInput();
            var existingClient = new Client(input.ClientId);

            _uow.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _uow.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);
            _uow.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);

            _clientRepo.Setup(x => x.GetByCodeAsync(input.ClientId))
                       .ReturnsAsync(existingClient);

            Account? createdAccount = null;
            _accountRepo.Setup(x => x.Create(It.IsAny<Account>()))
                        .Callback<Account>(acc => createdAccount = acc);

            var sut = CreateSut();

            var result = await sut.Create(input);

            Assert.NotNull(result.Data);
            Assert.Equal(existingClient.Code, result.Data!.ClientId);

            Assert.NotNull(createdAccount);
            Assert.False(string.IsNullOrWhiteSpace(createdAccount!.Code));
            Assert.Equal(createdAccount.Code, result.Data!.AccountId);
        }
 
        [Fact]
        public async Task Create_ClientDoesNotExist_CreatesClient_ReturnsOkAndCommits()
        {
            var input = ValidInput();

            _uow.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _uow.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);
            _uow.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);

            _clientRepo.Setup(x => x.GetByCodeAsync(input.ClientId))
                       .ReturnsAsync((Client?)null);

            Client? createdClient = null;
            _clientRepo.Setup(x => x.Create(It.IsAny<Client>()))
                       .Callback<Client>(c => createdClient = c);

            Account? createdAccount = null;
            _accountRepo.Setup(x => x.Create(It.IsAny<Account>()))
                        .Callback<Account>(acc => createdAccount = acc);

            var sut = CreateSut();

            var result = await sut.Create(input);

            Assert.NotNull(createdClient);
            Assert.NotNull(createdAccount);

            Assert.Equal(createdClient!.Code, result.Data!.ClientId);
            Assert.False(string.IsNullOrWhiteSpace(createdAccount!.Code));
            Assert.Equal(createdAccount.Code, result.Data!.AccountId);
        }

        [Fact]
        public async Task Create_WithoutCreditLimit_ThrowsExceptionAndRollbacks()
        {
            var input = new AccountCreateInputDto
            {
                ClientId = "CLI-123",
                AvailableBalance = 0
            };

            _uow.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _uow.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);
            _uow.Setup(x => x.RollbackAsync()).Returns(Task.CompletedTask);

            _clientRepo.Setup(x => x.GetByCodeAsync(input.ClientId))
                       .ReturnsAsync(new Client(input.ClientId));

            var sut = CreateSut();

            var ex = await Assert.ThrowsAsync<ApplicationException>(() => sut.Create(input));

            Assert.Equal("Erro ao criar conta", ex.Message);
            Assert.NotNull(ex.InnerException);
        }

        [Fact]
        public async Task Create_WithoutAvailableBalance_ThrowsExceptionAndRollbacks()
        {
            var input = new AccountCreateInputDto
            {
                ClientId = "CLI-123",
                CreditLimit = 0
            };

            _uow.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _uow.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);
            _uow.Setup(x => x.RollbackAsync()).Returns(Task.CompletedTask);

            _clientRepo.Setup(x => x.GetByCodeAsync(input.ClientId))
                       .ReturnsAsync(new Client(input.ClientId));

            var sut = CreateSut();

            var ex = await Assert.ThrowsAsync<ApplicationException>(() => sut.Create(input));

            Assert.Equal("Erro ao criar conta", ex.Message);
            Assert.NotNull(ex.InnerException);
        }

        [Fact]
        public async Task Create_AvailableBalanceNegative_ThrowsExceptionAndRollbacks()
        {
            var input = new AccountCreateInputDto
            {
                ClientId = "CLI-123",
                AvailableBalance = -100,
                CreditLimit = 0
            };

            _uow.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _uow.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);
            _uow.Setup(x => x.RollbackAsync()).Returns(Task.CompletedTask);

            _clientRepo.Setup(x => x.GetByCodeAsync(input.ClientId))
                       .ReturnsAsync(new Client(input.ClientId));

            var sut = CreateSut();

            var ex = await Assert.ThrowsAsync<ApplicationException>(() => sut.Create(input));

            Assert.Equal("Erro ao criar conta", ex.Message);
            Assert.NotNull(ex.InnerException);
        }

        [Fact]
        public async Task Create_AvailableCreditLimitNegative_ThrowsExceptionAndRollbacks()
        {
            var input = new AccountCreateInputDto
            {
                ClientId = "CLI-123",
                AvailableBalance = 100,
                CreditLimit = -50000
            };

            _uow.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _uow.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);
            _uow.Setup(x => x.RollbackAsync()).Returns(Task.CompletedTask);

            _clientRepo.Setup(x => x.GetByCodeAsync(input.ClientId))
                       .ReturnsAsync(new Client(input.ClientId));

            var sut = CreateSut();

            var ex = await Assert.ThrowsAsync<ApplicationException>(() => sut.Create(input));

            Assert.Equal("Erro ao criar conta", ex.Message);
            Assert.NotNull(ex.InnerException);
        }
    }
}
