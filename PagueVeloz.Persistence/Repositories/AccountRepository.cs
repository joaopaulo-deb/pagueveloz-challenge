using Microsoft.EntityFrameworkCore;
using PagueVeloz.Application.Contracts;
using PagueVeloz.Domain.Entities;
using PagueVeloz.Repository.Context;

namespace PagueVeloz.Repository.Repositories
{
    public class AccountRepository : BaseRepository<Account>, IAccountRepository
    {
        public AccountRepository(AppDbContext context) : base(context)
        { }

        public Task<Account?> GetByCodeAsync(string Code)
        {
            return _context.Account.FirstOrDefaultAsync(_ => _.Code == Code);
        }
    }
}
