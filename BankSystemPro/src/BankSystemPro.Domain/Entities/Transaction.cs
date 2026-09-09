using BankSystemPro.Domain.Enums;

namespace BankSystemPro.Domain.Entities
{
    public class Transaction
    {
        public int Id { get; set; }

        public int AccountId { get; set; }
        public Account Account { get; set; } = null!;

        public TransactionType Type { get; set; }
        public decimal Amount { get; set; }
        public decimal BalanceAfter { get; set; }
        public string? Description { get; set; }

        // Populated for TransferIn/TransferOut so both legs of a transfer are traceable.
        public int? RelatedAccountId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
