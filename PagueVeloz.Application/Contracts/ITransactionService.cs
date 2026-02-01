
using PagueVeloz.Application.Transactions;

namespace PagueVeloz.Application.Contracts
{
    public interface ITransactionService
    {
        Task<TransactionOutputDto> Handle(TransactionInputDto input);
    }
}
