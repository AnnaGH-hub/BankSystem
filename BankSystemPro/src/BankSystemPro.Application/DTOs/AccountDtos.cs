using BankSystemPro.Domain.Enums;

namespace BankSystemPro.Application.DTOs
{
    public record CreateAccountRequest(int CustomerId, int BranchId, AccountType Type, decimal InitialDeposit);

    public record AccountDto(int Id, string AccountNumber, string Type, string Status, decimal Balance, int CustomerId, int BranchId);

    public record DepositWithdrawRequest(string AccountNumber, decimal Amount);

    public record TransferRequest(string FromAccountNumber, string ToAccountNumber, decimal Amount);

    public record TransactionDto(int Id, string Type, decimal Amount, decimal BalanceAfter, string? Description, DateTime CreatedAt);
}
