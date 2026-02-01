
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
        public DbSet<Transaction> Transaction { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Account>()
                .HasOne(a => a.Customer)
                .WithMany(c => c.Accounts)
                .HasForeignKey(a => a.CustomerId);

            modelBuilder.Entity<Transaction>(e =>
            {
                e.Property(t => t.ReferenceId)
                    .IsRequired()
                    .HasMaxLength(100);

                e.HasIndex(t => new { t.AccountId, t.ReferenceId })
                    .IsUnique();

                e.Property(t => t.Operation)
                    .HasConversion<string>()
                    .HasMaxLength(32)
                    .IsUnicode(false);

                e.Property(t => t.Status)
                    .HasConversion<string>()
                    .HasMaxLength(32)
                    .IsUnicode(false);

                e.Property(t => t.Currency)
                    .HasConversion<string>()
                    .HasMaxLength(3)
                    .IsUnicode(false);

                e.Property(t => t.Description)
                    .HasMaxLength(255);
            });

            modelBuilder.Entity<Account>()
                .Property(a => a.Status)
                .HasConversion<string>();
        }
    }
}
