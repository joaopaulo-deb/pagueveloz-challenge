
using Microsoft.EntityFrameworkCore;
using PagueVeloz.Domain.Entities;

namespace PagueVeloz.Repository.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Customer> Customer { get; set; }
        public DbSet<Account> Account { get; set; }

    }
}
