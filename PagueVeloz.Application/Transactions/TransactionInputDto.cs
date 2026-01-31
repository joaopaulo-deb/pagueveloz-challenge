using PagueVeloz.Domain.Enums;

namespace PagueVeloz.Application.Transactions
{
    public class TransactionInputDto
    {
        public string Operation { get; set; }
        public string Account_id { get; set; }
        public int Amount { get; set; }
        public string Currency { get; set; }
        public string Reference_id { get; set; }
        public OperationMetadataDto Metadata { get; set; }
    }

    public class OperationMetadataDto
    {
        public string Description { get; set; }
    }
}
