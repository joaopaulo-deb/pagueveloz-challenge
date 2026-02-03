using PagueVeloz.Domain.Entities;
using PagueVeloz.Domain.Enums;

namespace Tests.Unit.Domain.Entities
{
    public class AccountTests
    {
        [Fact]
        public void Ctor_Defaults_StatusActive_ReservedZero()
        {
            var acc = new Account(clientId: 1, creditLimit: 100, availableBalance: 50);

            Assert.Equal(AccountStatus.Active, acc.Status);
            Assert.Equal(0, acc.ReservedBalance);
            Assert.Equal(50, acc.AvailableBalance);
            Assert.Equal(100, acc.CreditLimit);
            Assert.Equal(1, acc.ClientId);
        }

        [Fact]
        public void Credit_WhenAmountInvalid_Throws()
        {
            var acc = new Account(1, 0, 0);

            var ex = Assert.Throws<Exception>(() => acc.Credit(0));
            Assert.Equal("Valor inválido", ex.Message);
        }

        [Fact]
        public void Debit_WhenCreditLimitZero_AndWouldGoNegative_ThrowsSaldoInsuficiente()
        {
            var acc = new Account(1, creditLimit: 0, availableBalance: 10);

            var ex = Assert.Throws<Exception>(() => acc.Debit(11));
            Assert.Equal("Saldo insuficiente", ex.Message);
        }

        [Fact]
        public void Debit_WhenHasCreditLimit_CanGoNegative_UntilLimit()
        {
            var acc = new Account(1, creditLimit: 100, availableBalance: 10);

            acc.Debit(50);

            Assert.Equal(-40, acc.AvailableBalance);
        }

        [Fact]
        public void Debit_WhenExceedsLimit_ThrowsExcedeuOLimite()
        {
            var acc = new Account(1, creditLimit: 100, availableBalance: 10);

            var ex = Assert.Throws<Exception>(() => acc.Debit(200));
            Assert.Equal("Excedeu o limite", ex.Message);
        }

        [Fact]
        public void Reserve_MovesAvailableToReserved()
        {
            var acc = new Account(1, 0, 100);

            acc.Reserve(30);

            Assert.Equal(70, acc.AvailableBalance);
            Assert.Equal(30, acc.ReservedBalance);
        }

        [Fact]
        public void Capture_DecreasesReserved()
        {
            var acc = new Account(1, 0, 100);
            acc.Reserve(40);

            acc.Capture(15);

            Assert.Equal(25, acc.ReservedBalance);
            Assert.Equal(60, acc.AvailableBalance);
        }

        [Fact]
        public void TransferTo_WhenAmountGreaterThanAvailable_UsesCreditLimit()
        {
            var origin = new Account(1, creditLimit: 100, availableBalance: 20);
            var dest = new Account(2, creditLimit: 0, availableBalance: 0);

            origin.TransferTo(dest, 50);

            Assert.Equal(0, origin.AvailableBalance);
            Assert.Equal(70, origin.CreditLimit);
            Assert.Equal(50, dest.AvailableBalance);
        }

        [Fact]
        public void GenerateCode_FormatsWithPadding()
        {
            var acc = new Account(1, 0, 0) { Id = 7 };

            acc.GenerateCode();

            Assert.Equal("ACC-007", acc.Code);
        }
    }
}
