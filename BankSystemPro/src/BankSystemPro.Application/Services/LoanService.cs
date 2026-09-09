using BankSystemPro.Application.Common;
using BankSystemPro.Application.DTOs;
using BankSystemPro.Application.Interfaces;
using BankSystemPro.Domain.Entities;
using BankSystemPro.Domain.Enums;

namespace BankSystemPro.Application.Services
{
    public class LoanService : ILoanService
    {
        private readonly IUnitOfWork _uow;

        public LoanService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<LoanDto> RequestLoanAsync(LoanRequestDto request)
        {
            if (request.Amount <= 0)
                throw new BadRequestException("Loan amount must be positive.");
            if (request.TermMonths <= 0)
                throw new BadRequestException("Loan term must be positive.");

            var loan = new Loan
            {
                CustomerId = request.CustomerId,
                Amount = request.Amount,
                InterestRate = request.InterestRate,
                TermMonths = request.TermMonths,
                Status = LoanStatus.Pending
            };

            await _uow.Loans.AddAsync(loan);
            await _uow.SaveChangesAsync();

            return ToDto(loan);
        }

        // The state-machine part of the domain: a loan starts Pending and can only be
        // reviewed once. This is the kind of rule that separates "CRUD app" from
        // "app that models a real workflow" in an interview conversation.
        public async Task<LoanDto> ReviewLoanAsync(int loanId, int reviewerUserId, LoanDecisionRequest decision)
        {
            var loan = await _uow.Loans.GetByIdAsync(loanId)
                ?? throw new NotFoundException("Loan not found.");

            if (loan.Status != LoanStatus.Pending)
                throw new BadRequestException("This loan has already been reviewed.");

            loan.Status = decision.Approve ? LoanStatus.Approved : LoanStatus.Rejected;
            loan.ReviewedByUserId = reviewerUserId;
            loan.ReviewedAt = DateTime.UtcNow;
            loan.RejectionReason = decision.Approve ? null : decision.RejectionReason;

            await _uow.SaveChangesAsync();
            return ToDto(loan);
        }

        public async Task<List<LoanDto>> GetPendingLoansAsync()
        {
            var loans = await _uow.Loans.GetPendingAsync();
            return loans.Select(ToDto).ToList();
        }

        private static LoanDto ToDto(Loan l) =>
            new(l.Id, l.CustomerId, l.Amount, l.InterestRate, l.TermMonths, l.Status.ToString(), l.RequestedAt, l.ReviewedAt);
    }
}
