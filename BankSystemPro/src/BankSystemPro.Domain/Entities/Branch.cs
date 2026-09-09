namespace BankSystemPro.Domain.Entities
{
    public class Branch
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;

        public int BankId { get; set; }
        public Bank Bank { get; set; } = null!;

        public ICollection<Account> Accounts { get; set; } = new List<Account>();
        public ICollection<User> Employees { get; set; } = new List<User>();
    }
}
