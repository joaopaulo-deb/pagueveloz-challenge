
using Microsoft.EntityFrameworkCore;
using PagueVeloz.Domain.Entities;

namespace PagueVeloz.Repository.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Client> Client { get; set; }
        public DbSet<Account> Account { get; set; }
        public DbSet<Transaction> Transaction { get; set; }
        public DbSet<Event> Event { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Account>()
                .HasOne(a => a.Client)
                .WithMany(c => c.Accounts)
                .HasForeignKey(a => a.ClientId);

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

            modelBuilder.Entity<Event>(e =>
            {
                e.Property(t => t.Operation)
                    .HasConversion<string>()
                    .IsUnicode(false);

                e.Property(t => t.Status)
                    .HasConversion<string>()
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Event>()
              .Property(a => a.Status)
              .HasConversion<string>();

            modelBuilder.Entity<Account>()
            .Property(a => a.RowVersion)
            .IsRowVersion();
        }
    }
}
