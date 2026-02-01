
namespace PagueVeloz.Application.Transactions
{
    public interface ITransactionService
    {
        Task<TransactionOutputDto> Handle(TransactionInputDto input);
    }
}
