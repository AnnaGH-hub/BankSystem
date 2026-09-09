using BankSystemPro.Domain.Enums;

namespace BankSystemPro.Domain.Entities
{
    public class Loan
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }
        public Customer Customer { get; set; } = null!;

        public decimal Amount { get; set; }
        public decimal InterestRate { get; set; }
        public int TermMonths { get; set; }
        public LoanStatus Status { get; set; } = LoanStatus.Pending;

        public int? ReviewedByUserId { get; set; }
        public User? ReviewedBy { get; set; }
        public string? RejectionReason { get; set; }

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReviewedAt { get; set; }
    }
}
