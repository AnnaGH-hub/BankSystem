using BankSystemPro.Application.DTOs;
using BankSystemPro.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankSystemPro.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AccountsController : ControllerBase
    {
        private readonly IAccountService _accountService;
        public AccountsController(IAccountService accountService) => _accountService = accountService;

        [HttpPost]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<ActionResult<AccountDto>> Create(CreateAccountRequest request)
        {
            var account = await _accountService.CreateAccountAsync(request);
            return CreatedAtAction(nameof(GetForCustomer), new { customerId = account.CustomerId }, account);
        }

        [HttpGet("customer/{customerId:int}")]
        public async Task<ActionResult<List<AccountDto>>> GetForCustomer(int customerId)
        {
            var accounts = await _accountService.GetAccountsForCustomerAsync(customerId);
            return Ok(accounts);
        }

        [HttpPost("deposit")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<ActionResult<AccountDto>> Deposit(DepositWithdrawRequest request)
        {
            var account = await _accountService.DepositAsync(request);
            return Ok(account);
        }

        [HttpPost("withdraw")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<ActionResult<AccountDto>> Withdraw(DepositWithdrawRequest request)
        {
            var account = await _accountService.WithdrawAsync(request);
            return Ok(account);
        }

        [HttpPost("transfer")]
        public async Task<IActionResult> Transfer(TransferRequest request)
        {
            await _accountService.TransferAsync(request);
            return NoContent();
        }
    }
}
