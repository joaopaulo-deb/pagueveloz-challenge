using PagueVeloz.Domain.Entities;

namespace PagueVeloz.Application.Transactions.Operations
{
    public interface IReversalOperation : IOperation
    {
        Task<TransactionOutputDto> ExecuteAsync(string referenceId);
    }

}
