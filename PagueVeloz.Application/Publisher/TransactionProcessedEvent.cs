using PagueVeloz.Domain.Enums;

namespace PagueVeloz.Application.Publisher
{
    public record TransactionProcessedEvent(
     string TransactionId,
     int AccountId,
     OperationType Operation,
     string Status,
     int Amount,
     Currency Currency,
     DateTime Timestamp
 );

}
