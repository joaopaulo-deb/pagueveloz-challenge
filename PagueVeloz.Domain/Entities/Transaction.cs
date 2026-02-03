using PagueVeloz.Domain.Enums;

namespace PagueVeloz.Domain.Entities
{
    public sealed class Transaction : BaseEntity
    {
        public OperationType Operation { get; private set; }
        public int AccountId { get; private set; }
        public Account Account { get; private set; }
        public int Amount { get; private set; }
        public TransactionStatus Status { get; set; }
        public Currency Currency { get; set; }
        public string ReferenceId { get; set; }
        public string Description { get; set; }

        public Transaction()
        {
            
        }

        public Transaction(OperationType operation, int accountId, int amount, TransactionStatus status, Currency currency, string referenceId, string description)
        {
            Operation = operation;
            AccountId = accountId;
            Amount = amount;
            Status = status;
            Currency = currency;
            ReferenceId = referenceId;
            Description = description;

        }
    }
}
