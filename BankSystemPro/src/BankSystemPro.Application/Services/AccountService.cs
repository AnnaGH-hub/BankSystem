using BankSystemPro.Application.Common;
using BankSystemPro.Application.DTOs;
using BankSystemPro.Application.Interfaces;
using BankSystemPro.Domain.Entities;
using BankSystemPro.Domain.Enums;

namespace BankSystemPro.Application.Services
{
    public class AccountService : IAccountService
    {
        private readonly IUnitOfWork _uow;

        public AccountService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<AccountDto> CreateAccountAsync(CreateAccountRequest request)
        {
            if (request.InitialDeposit < 0)
                throw new BadRequestException("Initial deposit cannot be negative.");

            var account = new Account
            {
                AccountNumber = GenerateAccountNumber(),
                Type = request.Type,
                Balance = request.InitialDeposit,
                CustomerId = request.CustomerId,
                BranchId = request.BranchId,
                Status = AccountStatus.Active
            };

            await _uow.Accounts.AddAsync(account);
            await _uow.SaveChangesAsync();

            return ToDto(account);
        }

        public async Task<AccountDto> DepositAsync(DepositWithdrawRequest request)
        {
            if (request.Amount <= 0)
                throw new BadRequestException("Deposit amount must be positive.");

            var account = await _uow.Accounts.GetByAccountNumberAsync(request.AccountNumber)
                ?? throw new NotFoundException("Account not found.");

            EnsureAccountIsActive(account);

            account.Balance += request.Amount;

            await _uow.Accounts.AddTransactionAsync(new Transaction
            {
                AccountId = account.Id,
                Type = TransactionType.Deposit,
                Amount = request.Amount,
                BalanceAfter = account.Balance,
                Description = "Deposit"
            });

            await _uow.SaveChangesAsync();
            return ToDto(account);
        }

        public async Task<AccountDto> WithdrawAsync(DepositWithdrawRequest request)
        {
            if (request.Amount <= 0)
                throw new BadRequestException("Withdrawal amount must be positive.");

            var account = await _uow.Accounts.GetByAccountNumberAsync(request.AccountNumber)
                ?? throw new NotFoundException("Account not found.");

            EnsureAccountIsActive(account);

            if (account.Balance < request.Amount)
                throw new BadRequestException("Insufficient funds.");

            account.Balance -= request.Amount;

            await _uow.Accounts.AddTransactionAsync(new Transaction
            {
                AccountId = account.Id,
                Type = TransactionType.Withdrawal,
                Amount = request.Amount,
                BalanceAfter = account.Balance,
                Description = "Withdrawal"
            });

            await _uow.SaveChangesAsync();
            return ToDto(account);
        }

        // The one method worth walking an interviewer through: it moves money between
        // two accounts as a single atomic unit. If either account is missing, inactive,
        // or underfunded, nothing is written - both balances change together or not at all.
        public async Task TransferAsync(TransferRequest request)
        {
            if (request.Amount <= 0)
                throw new BadRequestException("Transfer amount must be positive.");

            if (request.FromAccountNumber == request.ToAccountNumber)
                throw new BadRequestException("Cannot transfer to the same account.");

            await _uow.ExecuteInTransactionAsync(async () =>
            {
                var from = await _uow.Accounts.GetByAccountNumberForUpdateAsync(request.FromAccountNumber)
                    ?? throw new NotFoundException("Source account not found.");
                var to = await _uow.Accounts.GetByAccountNumberForUpdateAsync(request.ToAccountNumber)
                    ?? throw new NotFoundException("Destination account not found.");

                EnsureAccountIsActive(from);
                EnsureAccountIsActive(to);

                if (from.Balance < request.Amount)
                    throw new BadRequestException("Insufficient funds.");

                from.Balance -= request.Amount;
                to.Balance += request.Amount;

                await _uow.Accounts.AddTransactionAsync(new Transaction
                {
                    AccountId = from.Id,
                    Type = TransactionType.TransferOut,
                    Amount = request.Amount,
                    BalanceAfter = from.Balance,
                    RelatedAccountId = to.Id,
                    Description = $"Transfer to {to.AccountNumber}"
                });

                await _uow.Accounts.AddTransactionAsync(new Transaction
                {
                    AccountId = to.Id,
                    Type = TransactionType.TransferIn,
                    Amount = request.Amount,
                    BalanceAfter = to.Balance,
                    RelatedAccountId = from.Id,
                    Description = $"Transfer from {from.AccountNumber}"
                });

                await _uow.SaveChangesAsync();
            });
        }

        public async Task<List<AccountDto>> GetAccountsForCustomerAsync(int customerId)
        {
            var accounts = await _uow.Accounts.GetByCustomerIdAsync(customerId);
            return accounts.Select(ToDto).ToList();
        }

        private static void EnsureAccountIsActive(Account account)
        {
            if (account.Status != AccountStatus.Active)
                throw new BadRequestException($"Account {account.AccountNumber} is {account.Status} and cannot be used.");
        }

        private static string GenerateAccountNumber()
        {
            // Simple, readable, and unique-enough for a portfolio project.
            // A production system would use a dedicated sequence or check-digit scheme.
            return DateTime.UtcNow.Ticks.ToString()[^10..];
        }

        private static AccountDto ToDto(Account a) =>
            new(a.Id, a.AccountNumber, a.Type.ToString(), a.Status.ToString(), a.Balance, a.CustomerId, a.BranchId);
    }
}
