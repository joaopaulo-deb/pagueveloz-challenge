using PagueVeloz.Domain.Enums;

namespace PagueVeloz.Domain.Entities
{
    public class Account : BaseEntity
    {
        public string? Code { get; set; }
        public int AvailableBalance { get; set; }
        public int ReservedBalance { get; set; }
        public int CreditLimit { get; set; }
        public StatusAccount Status { get; private set; }
        public int ClientId { get; private set; }
        public Client Client { get; private set; }
        private readonly List<Transaction> _transactionHistory = new();
        public IReadOnlyCollection<Transaction> TransactionHistory => _transactionHistory;

        public Account(int clientId, int creditLimit, int availableBalance)
        {
            ClientId = clientId;
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

            var newBalance = AvailableBalance - amount;
            var maxNegativeBalance = -CreditLimit;

            if (newBalance < maxNegativeBalance)
            {
                if (CreditLimit == 0)
                    throw new Exception("Saldo insuficiente");

                throw new Exception("Excedeu o limite");
            }

            AvailableBalance = newBalance;
        }

        public void Reserve(int amount)
        {
            if (Status != StatusAccount.Active)
                throw new Exception("Conta inativa");

            if (amount <= 0)
                throw new Exception("Valor inválido");

            if (amount > AvailableBalance)
                throw new Exception("Saldo disponível insuficiente");

            AvailableBalance -= amount;
            ReservedBalance += amount;
        }

        public void Capture(int amount)
        {
            if (Status != StatusAccount.Active)
                throw new Exception("Conta inativa");

            if (amount <= 0)
                throw new Exception("Valor inválido");

            if (amount > ReservedBalance)
                throw new Exception("Saldo reservado insuficiente");

            ReservedBalance -= amount;
        }

        public void TransferTo(Account destination, int amount)
        {
            if (Status != StatusAccount.Active)
                throw new Exception("Conta origem inativa");

            if (destination.Status != StatusAccount.Active)
                throw new Exception("Conta destino inativa");

            if (amount <= 0)
                throw new Exception("Valor inválido");

            var availableFunds = AvailableBalance + CreditLimit;

            if (amount > availableFunds)
                throw new Exception("Saldo insuficiente");

            if (amount <= AvailableBalance)
            {
                AvailableBalance -= amount;
            }
            else
            {
                var remaining = amount - AvailableBalance;
                AvailableBalance = 0;
                CreditLimit -= remaining;
            }

            destination.AvailableBalance += amount;
        }

        public void Reversal()
        {
           
        }

        public void GenerateCode()
        {
            Code = $"ACC-{Id:D3}";
        }
    }
}
