using PagueVeloz.Domain.Entities;

namespace PagueVeloz.Application.Transactions.Operations
{
    public interface ITransferOperation : IOperation
    {
        Task<TransactionOutputDto> ExecuteAsync(
            Account sourceAccount,
            Account destinationAccount,
            TransactionInputDto dto
        );
    }

}
