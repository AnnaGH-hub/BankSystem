using BankSystemPro.Domain.Enums;

namespace BankSystemPro.Domain.Entities
{
    public class Account
    {
        public int Id { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public AccountType Type { get; set; }
        public AccountStatus Status { get; set; } = AccountStatus.Active;
        public decimal Balance { get; set; }

        public int CustomerId { get; set; }
        public Customer Customer { get; set; } = null!;

        public int BranchId { get; set; }
        public Branch Branch { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Concurrency token: prevents two simultaneous transfers/withdrawals from
        // silently overwriting each other's balance update.
        public byte[]? RowVersion { get; set; }

        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
