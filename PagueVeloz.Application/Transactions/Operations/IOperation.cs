using PagueVeloz.Domain.Entities;
using PagueVeloz.Domain.Enums;


namespace PagueVeloz.Application.Transactions.Operations
{
    public interface IOperation
    {
        OperationType Type { get; }
    }
}
