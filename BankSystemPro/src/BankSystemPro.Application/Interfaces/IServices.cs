using BankSystemPro.Application.DTOs;
using BankSystemPro.Domain.Entities;

namespace BankSystemPro.Application.Interfaces
{
    public interface IPasswordHasher
    {
        string Hash(string password);
        bool Verify(string password, string hash);
    }

    public interface ITokenService
    {
        (string token, DateTime expiresAt) GenerateToken(User user);
    }

    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);
    }

    public interface IAccountService
    {
        Task<AccountDto> CreateAccountAsync(CreateAccountRequest request);
        Task<AccountDto> DepositAsync(DepositWithdrawRequest request);
        Task<AccountDto> WithdrawAsync(DepositWithdrawRequest request);
        Task TransferAsync(TransferRequest request);
        Task<List<AccountDto>> GetAccountsForCustomerAsync(int customerId);
    }

    public interface ILoanService
    {
        Task<LoanDto> RequestLoanAsync(LoanRequestDto request);
        Task<LoanDto> ReviewLoanAsync(int loanId, int reviewerUserId, LoanDecisionRequest decision);
        Task<List<LoanDto>> GetPendingLoansAsync();
    }
}
