using PagueVeloz.Domain.Enums;

namespace PagueVeloz.Application.Transactions
{
    public class TransactionOutputDto
    {
        public string transaction_id { get; set; }
        public TransactionStatus status { get; set; }
        public int balance { get; set; }
        public int reserved_balance { get; set; }
        public int available_balance { get; set; }
        public DateTime timestamp { get; set; }
        public string? error_message { get; set; }
    }
}
