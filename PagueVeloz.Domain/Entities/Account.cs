using PagueVeloz.Domain.Enums;

namespace PagueVeloz.Domain.Entities
{
    public class Account : BaseEntity
    {
        public int AvailableBalance { get; set; }
        public int ReservedBalance { get; set; }
        public int CreditLimit { get; set; }
        public StatusAccount Status { get; private set; }

        private readonly List<Transaction> _transactionHistory = new();
        public IReadOnlyCollection<Transaction> TransactionHistory => _transactionHistory;
    }
}
