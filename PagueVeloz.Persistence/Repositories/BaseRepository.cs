
using Microsoft.EntityFrameworkCore;
using PagueVeloz.Application.Contracts;
using PagueVeloz.Domain.Entities;
using PagueVeloz.Repository.Context;

namespace PagueVeloz.Repository.Repositories
{
    public class BaseRepository<T> : IBaseRepository<T> where T : BaseEntity
    {
        protected readonly AppDbContext _context;

        public BaseRepository(AppDbContext context)
        {
            _context = context;
        }


        public void Create(T entity)
        {
            entity.CreatedAt = DateTime.UtcNow;
            _context.Add(entity);
        }

        public void Delete(T entity)
        {
            _context.Remove(entity);
        }

        public void Update(T entity)
        {
            entity.UpdatedAt = DateTime.UtcNow;
            _context.Update(entity);
        }

        public async Task<T?> Get(int id)
        {
            return await _context.Set<T>().FirstOrDefaultAsync(_ => _.Id == id);
        }

        public async Task<List<T>> GetAll()
        {
            return await _context.Set<T>().ToListAsync();
        }

        public async Task<bool> SaveChangesAsync()
        {
            return (await _context.SaveChangesAsync() > 0);
        }
    }
}
