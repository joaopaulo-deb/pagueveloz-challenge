using Moq;
using PagueVeloz.Domain.Entities;
using PagueVeloz.Domain.Enums;
using PagueVeloz.Domain.Contracts;
using PagueVeloz.Application.Transactions;
using PagueVeloz.Application.Transactions.Operations;

namespace Tests.Unit.Application
{
    public class TransactionServiceTests
    {
        private readonly Mock<IAccountRepository> _accountRepo = new();

        private static TransactionInputDto ValidInput(OperationType op = OperationType.credit) => new TransactionInputDto
        {
            Operation = op,
            Account_id = "ACC-001",
            Destination_account_id = "ACC-002",
            Amount = 100,
            Reference_id = "REF-123",
            Currency = Currency.BRL,
        };

        private static Account ActiveAccount(int clientId = 1, int creditLimit = 1000, int availableBalance = 100)
        {
            var acc = new Account(clientId, creditLimit, availableBalance)
            {
                Code = "ACC-001"
            };
            return acc;
        }

        private TransactionService CreateSut(IEnumerable<IOperation> ops)
            => new TransactionService(ops, _accountRepo.Object);

        [Fact]
        public async Task Handle_WhenAmountIsZero_ReturnsFailedWithMessage()
        {
            var input = ValidInput();
            input.Amount = 0;

            var sut = CreateSut(Array.Empty<IOperation>());

            var result = await sut.Handle(input);

            Assert.Equal(TransactionStatus.failed, result.status);
            Assert.Equal("Operação inválida", result.error_message);
            Assert.Equal(input.Reference_id, result.transaction_id);
        }

        [Fact]
        public async Task Handle_WhenReferenceIdMissing_ReturnsFailedWithMessage()
        {
            var input = ValidInput();
            input.Reference_id = "  ";

            var op = new Mock<IOperation>();
            op.SetupGet(o => o.Type).Returns(input.Operation);

            var sut = CreateSut(new[] { op.Object });

            var result = await sut.Handle(input);

            Assert.Equal(TransactionStatus.failed, result.status);
            Assert.Equal("Reference_id é obrigatório", result.error_message);
            Assert.Equal(input.Reference_id, result.transaction_id);
        }

        [Fact]
        public async Task Handle_WhenAccountNotFound_ReturnsFailedContaInvalidaOuInativa()
        {
            var input = ValidInput();

            var op = new Mock<IOperation>();
            op.SetupGet(o => o.Type).Returns(input.Operation);

            _accountRepo.Setup(r => r.GetByCodeAsync(input.Account_id))
                       .ReturnsAsync((Account?)null);

            var sut = CreateSut(new[] { op.Object });

            var result = await sut.Handle(input);

            Assert.Equal(TransactionStatus.failed, result.status);
            Assert.Equal("Conta inválida ou inativa", result.error_message);
            Assert.Equal(input.Reference_id, result.transaction_id);

            _accountRepo.Verify(r => r.GetByCodeAsync(input.Account_id), Times.Once);
        }

        [Fact]
        public async Task Handle_WhenOperationNotFound_ReturnsFailedOperacaoInvalida()
        {
            var input = ValidInput(op: OperationType.credit);

            _accountRepo.Setup(r => r.GetByCodeAsync(input.Account_id))
                        .ReturnsAsync(ActiveAccount());

            var sut = CreateSut(Array.Empty<IOperation>());

            var result = await sut.Handle(input);

            Assert.Equal(TransactionStatus.failed, result.status);
            Assert.Equal("Operação inválida", result.error_message);
            Assert.Equal(input.Reference_id, result.transaction_id);
        }

        [Fact]
        public async Task Handle_WhenSingleAccountOperation_CallsExecuteWithAccount_AndReturnsResult()
        {
            var input = ValidInput(op: OperationType.credit);
            var account = ActiveAccount();

            _accountRepo.Setup(r => r.GetByCodeAsync(input.Account_id))
                        .ReturnsAsync(account);

            var singleOp = new Mock<ISingleAccountOperation>();
            singleOp.SetupGet(x => x.Type).Returns(OperationType.credit);

            var expectedResult = new TransactionOutputDto
            {
                transaction_id = input.Reference_id,
                status = TransactionStatus.success,
                timestamp = DateTime.UtcNow
            };

            singleOp.Setup(x => x.ExecuteAsync(account, input))
                    .ReturnsAsync(expectedResult);

            var sut = CreateSut(new IOperation[] { singleOp.Object });

            var result = await sut.Handle(input);

            Assert.Equal(TransactionStatus.success, result.status);
            Assert.Equal(input.Reference_id, result.transaction_id);
        }

        [Fact]
        public async Task Handle_WhenTransferOperation_FetchesDestination_AndCallsExecuteWithBothAccounts()
        {
            var input = ValidInput(op: OperationType.transfer);

            var origin = ActiveAccount();
            origin.Code = input.Account_id;

            var destination = ActiveAccount(clientId: 2);
            destination.Code = input.Destination_account_id;

            _accountRepo.Setup(r => r.GetByCodeAsync(input.Account_id))
                        .ReturnsAsync(origin);

            _accountRepo.Setup(r => r.GetByCodeAsync(input.Destination_account_id))
                        .ReturnsAsync(destination);

            var transferOp = new Mock<ITransferOperation>();
            transferOp.SetupGet(x => x.Type).Returns(OperationType.transfer);

            var expectedResult = new TransactionOutputDto
            {
                transaction_id = input.Reference_id,
                status = TransactionStatus.success,
                timestamp = DateTime.UtcNow
            };

            transferOp.Setup(x => x.ExecuteAsync(origin, destination, input))
                      .ReturnsAsync(expectedResult);

            var sut = CreateSut(new IOperation[] { transferOp.Object });

            var result = await sut.Handle(input);

            Assert.Equal(TransactionStatus.success, result.status);
            Assert.Equal(input.Reference_id, result.transaction_id);
        }

        [Fact]
        public async Task Handle_WhenTransferOperation_DestinationInvalid_ReturnsFailedContaInvalidaOuInativa()
        {
            var input = ValidInput(op: OperationType.transfer);

            _accountRepo.Setup(r => r.GetByCodeAsync(input.Account_id))
                        .ReturnsAsync(ActiveAccount());

            _accountRepo.Setup(r => r.GetByCodeAsync(input.Destination_account_id))
                        .ReturnsAsync((Account?)null);

            var transferOp = new Mock<ITransferOperation>();
            transferOp.SetupGet(x => x.Type).Returns(OperationType.transfer);

            var sut = CreateSut(new IOperation[] { transferOp.Object });

            var result = await sut.Handle(input);

            Assert.Equal(TransactionStatus.failed, result.status);
            Assert.Equal("Conta inválida ou inativa", result.error_message);
            Assert.Equal(input.Reference_id, result.transaction_id);

            transferOp.Verify(x => x.ExecuteAsync(It.IsAny<Account>(), It.IsAny<Account>(), It.IsAny<TransactionInputDto>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenOperationExistsButIsNotSingleOrTransfer_ReturnsFailedOperacaoInvalida()
        {
            var input = ValidInput(op: OperationType.credit);

            _accountRepo.Setup(r => r.GetByCodeAsync(input.Account_id))
                        .ReturnsAsync(ActiveAccount());

            var op = new Mock<IOperation>();
            op.SetupGet(x => x.Type).Returns(OperationType.credit);

            var sut = CreateSut(new IOperation[] { op.Object });

            var result = await sut.Handle(input);

            Assert.Equal(TransactionStatus.failed, result.status);
            Assert.Equal("Operação inválida", result.error_message);
            Assert.Equal(input.Reference_id, result.transaction_id);
        }

        [Fact]
        public async Task Handle_WhenExecuteThrows_ReturnsFailedWithExceptionMessage()
        {
            var input = ValidInput(op: OperationType.credit);
            var account = ActiveAccount();

            _accountRepo.Setup(r => r.GetByCodeAsync(input.Account_id))
                        .ReturnsAsync(account);

            var singleOp = new Mock<ISingleAccountOperation>();
            singleOp.SetupGet(x => x.Type).Returns(OperationType.credit);

            singleOp.Setup(x => x.ExecuteAsync(account, input))
                    .ThrowsAsync(new Exception("boom"));

            var sut = CreateSut(new IOperation[] { singleOp.Object });

            var result = await sut.Handle(input);

            Assert.Equal(TransactionStatus.failed, result.status);
            Assert.Equal("boom", result.error_message);
            Assert.Equal(input.Reference_id, result.transaction_id);
        }
    }
}
