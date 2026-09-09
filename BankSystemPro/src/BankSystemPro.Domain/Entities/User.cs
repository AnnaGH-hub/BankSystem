using BankSystemPro.Domain.Enums;

namespace BankSystemPro.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; }

        // Only set for Employee/Admin users tied to a specific branch.
        public int? BranchId { get; set; }
        public Branch? Branch { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Only set when Role == Customer.
        public Customer? CustomerProfile { get; set; }
    }
}
