using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using PagueVeloz.Domain.Contracts;
using PagueVeloz.Domain.Entities;
using PagueVeloz.Repository.Context;

namespace PagueVeloz.Repository.Repositories
{
    public class TransactionRepository : BaseRepository<Transaction>, ITransactionRepository
    {
        public TransactionRepository(AppDbContext context) : base(context)
        { }

        public async Task<Transaction?> GetAsync(string referenceId, int? accountId = null)
        {
            var query = _context.Transaction
                .AsNoTracking()
                .Where(t => t.ReferenceId == referenceId);

            if (accountId.HasValue)
            {
                query = query.Where(t => t.AccountId == accountId.Value);
            }

            return await query.FirstOrDefaultAsync();
        }
    }
}
