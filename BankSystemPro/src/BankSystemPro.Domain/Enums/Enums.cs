namespace BankSystemPro.Domain.Enums
{
    public enum UserRole
    {
        Admin,
        Employee,
        Customer
    }

    public enum AccountType
    {
        Checking,
        Savings
    }

    public enum AccountStatus
    {
        Active,
        Frozen,
        Closed
    }

    public enum TransactionType
    {
        Deposit,
        Withdrawal,
        TransferIn,
        TransferOut
    }

    public enum LoanStatus
    {
        Pending,
        Approved,
        Rejected,
        Closed
    }
}
