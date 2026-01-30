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

        public void GenerateCode()
        {
            Code = $"ACC-{Id:D3}";
        }
    }
}
