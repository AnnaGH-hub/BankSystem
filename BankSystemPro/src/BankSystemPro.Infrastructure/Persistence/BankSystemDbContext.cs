using BankSystemPro.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BankSystemPro.Infrastructure.Persistence
{
    public class BankSystemDbContext : DbContext
    {
        public BankSystemDbContext(DbContextOptions<BankSystemDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Bank> Banks => Set<Bank>();
        public DbSet<Branch> Branches => Set<Branch>();
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Account> Accounts => Set<Account>();
        public DbSet<Transaction> Transactions => Set<Transaction>();
        public DbSet<Loan> Loans => Set<Loan>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.Role).HasConversion<string>().HasMaxLength(20);
                entity.HasOne(u => u.Branch)
                      .WithMany(b => b.Employees)
                      .HasForeignKey(u => u.BranchId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Bank>(entity =>
            {
                entity.HasIndex(b => b.SwiftCode).IsUnique();
            });

            modelBuilder.Entity<Branch>(entity =>
            {
                entity.HasOne(b => b.Bank)
                      .WithMany(bk => bk.Branches)
                      .HasForeignKey(b => b.BankId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasIndex(c => c.NationalId).IsUnique();
                entity.HasOne(c => c.User)
                      .WithOne(u => u.CustomerProfile)
                      .HasForeignKey<Customer>(c => c.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Account>(entity =>
            {
                entity.HasIndex(a => a.AccountNumber).IsUnique();
                entity.Property(a => a.Balance).HasColumnType("decimal(18,2)");
                entity.Property(a => a.Type).HasConversion<string>().HasMaxLength(20);
                entity.Property(a => a.Status).HasConversion<string>().HasMaxLength(20);
                entity.Property(a => a.RowVersion).IsRowVersion();

                entity.HasOne(a => a.Customer)
                      .WithMany(c => c.Accounts)
                      .HasForeignKey(a => a.CustomerId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Branch)
                      .WithMany(b => b.Accounts)
                      .HasForeignKey(a => a.BranchId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.Property(t => t.Amount).HasColumnType("decimal(18,2)");
                entity.Property(t => t.BalanceAfter).HasColumnType("decimal(18,2)");
                entity.Property(t => t.Type).HasConversion<string>().HasMaxLength(20);

                entity.HasOne(t => t.Account)
                      .WithMany(a => a.Transactions)
                      .HasForeignKey(t => t.AccountId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Loan>(entity =>
            {
                entity.Property(l => l.Amount).HasColumnType("decimal(18,2)");
                entity.Property(l => l.InterestRate).HasColumnType("decimal(5,2)");
                entity.Property(l => l.Status).HasConversion<string>().HasMaxLength(20);

                entity.HasOne(l => l.Customer)
                      .WithMany(c => c.Loans)
                      .HasForeignKey(l => l.CustomerId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(l => l.ReviewedBy)
                      .WithMany()
                      .HasForeignKey(l => l.ReviewedByUserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
