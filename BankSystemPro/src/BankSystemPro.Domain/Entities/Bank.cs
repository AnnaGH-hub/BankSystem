namespace BankSystemPro.Domain.Entities
{
    public class Bank
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string SwiftCode { get; set; } = string.Empty;

        public ICollection<Branch> Branches { get; set; } = new List<Branch>();
    }
}
