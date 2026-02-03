using PagueVeloz.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace PagueVeloz.Application.Transactions
{
    public class TransactionInputDto
    {
        public OperationType Operation { get; set; }
        public string Account_id { get; set; } = string.Empty;
        public string? Destination_account_id { get; set; }
        public int Amount { get; set; }
        public Currency Currency { get; set; }
        public string Reference_id { get; set; } = string.Empty;
        public OperationMetadataDto? Metadata { get; set; }
    }

    public class OperationMetadataDto
    {
        public string Description { get; set; }
    }
}
