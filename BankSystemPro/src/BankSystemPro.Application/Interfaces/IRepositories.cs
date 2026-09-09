using BankSystemPro.Domain.Entities;

namespace BankSystemPro.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByIdAsync(int id);
        Task AddAsync(User user);
    }

    public interface IAccountRepository
    {
        Task<Account?> GetByAccountNumberAsync(string accountNumber);

        // Semantically distinct from GetByAccountNumberAsync: callers use this before
        // mutating balances, signalling intent even though EF Core's optimistic
        // concurrency (via Account.RowVersion) is what actually protects the write.
        Task<Account?> GetByAccountNumberForUpdateAsync(string accountNumber);

        Task<List<Account>> GetByCustomerIdAsync(int customerId);
        Task AddAsync(Account account);
        Task AddTransactionAsync(Transaction transaction);
    }

    public interface ILoanRepository
    {
        Task<Loan?> GetByIdAsync(int id);
        Task<List<Loan>> GetByCustomerIdAsync(int customerId);
        Task<List<Loan>> GetPendingAsync();
        Task AddAsync(Loan loan);
    }

    public interface IUnitOfWork
    {
        IUserRepository Users { get; }
        IAccountRepository Accounts { get; }
        ILoanRepository Loans { get; }

        Task<int> SaveChangesAsync();

        // Wraps multi-step operations (e.g. a transfer touching two accounts) in a
        // single DB transaction, without leaking EF Core transaction types into the
        // Application layer.
        Task ExecuteInTransactionAsync(Func<Task> operation);
    }
}
