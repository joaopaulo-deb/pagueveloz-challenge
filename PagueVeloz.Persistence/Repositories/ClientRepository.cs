using PagueVeloz.Application.Contracts;
using PagueVeloz.Domain.Entities;
using PagueVeloz.Repository.Context;
using Microsoft.EntityFrameworkCore;
using PagueVeloz.Domain.Contracts;


namespace PagueVeloz.Repository.Repositories
{
    public class ClientRepository : BaseRepository<Client>, IClientRepository
    {
        public ClientRepository(AppDbContext context) : base(context)
        { }

        public Task<Client?> GetByCodeAsync(string Code)
        {
            return _context.Client.FirstOrDefaultAsync(_ => _.Code == Code);
        }
    }
}
