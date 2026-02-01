using PagueVeloz.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace PagueVeloz.Application.Transactions
{
    public class TransactionInputDto
    {
        [Required]
        public OperationType Operation { get; set; }
        [Required]
        public string Account_id { get; set; }
        public string? Destination_account_id { get; set; }
        [Range(1, int.MaxValue)]
        public int Amount { get; set; }
        [Required]
        public Currency Currency { get; set; }
        [Required]
        public string Reference_id { get; set; }
        public OperationMetadataDto? Metadata { get; set; }
    }

    public class OperationMetadataDto
    {
        public string Description { get; set; }
    }
}
