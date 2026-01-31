using Microsoft.EntityFrameworkCore;
using PagueVeloz.Application.Contracts;
using PagueVeloz.Domain.Entities;
using PagueVeloz.Repository.Context;

namespace PagueVeloz.Repository.Repositories
{
    public class TransactionRepository : BaseRepository<Transaction>, ITransactionRepository
    {
        public TransactionRepository(AppDbContext context) : base(context)
        { }

        /*public Task<Transaction?> GetByCodeAsync(string Code)
        {
            return _context.Transaction.FirstOrDefaultAsync(_ => _.Code == Code);
        }*/
    }
}
