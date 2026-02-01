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

        public async Task<Transaction?> GetByAccountAndReferenceIdAsync(int accountId, string referenceId)
        {
            return await _context.Transaction
                .AsNoTracking()
                .FirstOrDefaultAsync(_ =>
                    _.AccountId == accountId &&
                    _.ReferenceId == referenceId);
        }
    }
}
