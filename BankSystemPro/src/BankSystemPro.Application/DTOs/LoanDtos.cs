namespace BankSystemPro.Application.DTOs
{
    public record LoanRequestDto(int CustomerId, decimal Amount, decimal InterestRate, int TermMonths);

    public record LoanDecisionRequest(bool Approve, string? RejectionReason);

    public record LoanDto(int Id, int CustomerId, decimal Amount, decimal InterestRate, int TermMonths, string Status, DateTime RequestedAt, DateTime? ReviewedAt);
}
