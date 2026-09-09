using BankSystemPro.Application.Interfaces;
using BankSystemPro.Domain.Entities;
using BankSystemPro.Domain.Enums;
using BankSystemPro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BankSystemPro.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly BankSystemDbContext _db;
        public UserRepository(BankSystemDbContext db) => _db = db;

        public Task<User?> GetByEmailAsync(string email) =>
            _db.Users.FirstOrDefaultAsync(u => u.Email == email);

        public Task<User?> GetByIdAsync(int id) =>
            _db.Users.FirstOrDefaultAsync(u => u.Id == id);

        public async Task AddAsync(User user) => await _db.Users.AddAsync(user);
    }

    public class AccountRepository : IAccountRepository
    {
        private readonly BankSystemDbContext _db;
        public AccountRepository(BankSystemDbContext db) => _db = db;

        public Task<Account?> GetByAccountNumberAsync(string accountNumber) =>
            _db.Accounts.FirstOrDefaultAsync(a => a.AccountNumber == accountNumber);

        // EF Core's optimistic concurrency (Account.RowVersion) is what actually
        // guards against two concurrent writes clobbering each other; SaveChangesAsync
        // throws DbUpdateConcurrencyException if the row changed since it was read.
        public Task<Account?> GetByAccountNumberForUpdateAsync(string accountNumber) =>
            _db.Accounts.FirstOrDefaultAsync(a => a.AccountNumber == accountNumber);

        public Task<List<Account>> GetByCustomerIdAsync(int customerId) =>
            _db.Accounts.Where(a => a.CustomerId == customerId).ToListAsync();

        public async Task AddAsync(Account account) => await _db.Accounts.AddAsync(account);

        public async Task AddTransactionAsync(Transaction transaction) =>
            await _db.Transactions.AddAsync(transaction);
    }

    public class LoanRepository : ILoanRepository
    {
        private readonly BankSystemDbContext _db;
        public LoanRepository(BankSystemDbContext db) => _db = db;

        public Task<Loan?> GetByIdAsync(int id) =>
            _db.Loans.FirstOrDefaultAsync(l => l.Id == id);

        public Task<List<Loan>> GetByCustomerIdAsync(int customerId) =>
            _db.Loans.Where(l => l.CustomerId == customerId).ToListAsync();

        public Task<List<Loan>> GetPendingAsync() =>
            _db.Loans.Where(l => l.Status == LoanStatus.Pending).ToListAsync();

        public async Task AddAsync(Loan loan) => await _db.Loans.AddAsync(loan);
    }

    public class UnitOfWork : IUnitOfWork
    {
        private readonly BankSystemDbContext _db;

        public UnitOfWork(BankSystemDbContext db, IUserRepository users, IAccountRepository accounts, ILoanRepository loans)
        {
            _db = db;
            Users = users;
            Accounts = accounts;
            Loans = loans;
        }

        public IUserRepository Users { get; }
        public IAccountRepository Accounts { get; }
        public ILoanRepository Loans { get; }

        public Task<int> SaveChangesAsync() => _db.SaveChangesAsync();

        public async Task ExecuteInTransactionAsync(Func<Task> operation)
        {
            // ExecutionStrategy handles SQL Server's connection-resiliency retries;
            // wrapping the transaction inside it (rather than the other way round)
            // is required by EF Core when retry-on-failure is enabled.
            var strategy = _db.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _db.Database.BeginTransactionAsync();
                try
                {
                    await operation();
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }
    }
}
