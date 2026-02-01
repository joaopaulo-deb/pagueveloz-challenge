using PagueVeloz.Domain.Entities;

namespace PagueVeloz.Application.Transactions.Operations
{
    public interface ISingleAccountOperation : IOperation
    {
        Task<TransactionOutputDto> ExecuteAsync(
            Account account,
            TransactionInputDto dto
        );
    }

}
