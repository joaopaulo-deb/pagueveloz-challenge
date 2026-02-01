
namespace PagueVeloz.Application.Contracts
{
    public interface IUnitOfWork
    {
        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
        Task SaveChangesAsync();
        void ClearTracking();
    }
}
