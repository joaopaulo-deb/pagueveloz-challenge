using PagueVeloz.Domain.Enums;

namespace PagueVeloz.Domain.Entities
{
    public class Account : BaseEntity
    {
        public string? Code { get; set; }
        public int AvailableBalance { get; set; } //saldo disponivel
        public int ReservedBalance { get; set; } //saldo reservado
        public int CreditLimit { get; set; }  //limite de credito
        public StatusAccount Status { get; private set; }
        public int CustomerId { get; private set; }
        public Customer Customer { get; private set; }
        private readonly List<Transaction> _transactionHistory = new();
        public IReadOnlyCollection<Transaction> TransactionHistory => _transactionHistory;

        public Account(int customerId, int creditLimit, int availableBalance)
        {
            CustomerId = customerId;
            CreditLimit = creditLimit;
            AvailableBalance = availableBalance;
            ReservedBalance = 0;
            Status = StatusAccount.Active;
        }

        public void Credit(int amount)
        {
            if (Status != StatusAccount.Active)
                throw new Exception("Conta inativa");

            if (amount <= 0)
                throw new Exception("Valor inválido");

            AvailableBalance += amount;
        }

        public void Debit(int amount)
        {
            if (Status != StatusAccount.Active)
                throw new Exception("Conta inativa");

            if (amount <= 0)
                throw new Exception("Valor inválido");

            var availableFunds = AvailableBalance + CreditLimit;

            if (amount > availableFunds)
                throw new Exception("Saldo insuficiente");

            if (amount <= AvailableBalance)
            {
                AvailableBalance -= amount;
                return;
            }

            var remaining = amount - AvailableBalance;

            AvailableBalance = 0;
            CreditLimit -= remaining;
        }

        public void GenerateCode()
        {
            Code = $"ACC-{Id:D3}";
        }
    }
}
