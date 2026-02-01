using PagueVeloz.Domain.Entities;

namespace PagueVeloz.Application.Contracts
{
    public interface IBaseRepository<T> where T : BaseEntity
    {
        void Create(T entity);
        void Update(T entity);
        void Delete(T entity);
        Task<T?> Get(int Id);
        Task<List<T>> GetAll();
        Task<bool> SaveChangesAsync();
    }
}
