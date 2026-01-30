using PagueVeloz.Application.Contracts;
using PagueVeloz.Domain.Entities;
using PagueVeloz.Repository.Context;
using Microsoft.EntityFrameworkCore;


namespace PagueVeloz.Repository.Repositories
{
    public class CustomerRepository : BaseRepository<Customer>, ICustomerRepository
    {
        public CustomerRepository(AppDbContext context) : base(context)
        { }

        public Task<Customer?> GetByCodeAsync(string Code)
        {
            return _context.Customer.FirstOrDefaultAsync(_ => _.Code == Code);
        }
    }
}
