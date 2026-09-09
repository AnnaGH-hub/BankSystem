using System.Security.Claims;
using BankSystemPro.Application.DTOs;
using BankSystemPro.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankSystemPro.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LoansController : ControllerBase
    {
        private readonly ILoanService _loanService;
        public LoansController(ILoanService loanService) => _loanService = loanService;

        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<ActionResult<LoanDto>> Request(LoanRequestDto request)
        {
            var loan = await _loanService.RequestLoanAsync(request);
            return Ok(loan);
        }

        [HttpGet("pending")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<ActionResult<List<LoanDto>>> GetPending()
        {
            var loans = await _loanService.GetPendingLoansAsync();
            return Ok(loans);
        }

        [HttpPost("{id:int}/review")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<ActionResult<LoanDto>> Review(int id, LoanDecisionRequest decision)
        {
            var reviewerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var loan = await _loanService.ReviewLoanAsync(id, reviewerId, decision);
            return Ok(loan);
        }
    }
}
