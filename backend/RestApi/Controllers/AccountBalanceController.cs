using Domain.Entities;
using Domain.Repositories;
using Domain.Services;
using Microsoft.AspNetCore.Mvc;
using RestApi.Models.AccountBalance;

namespace RestApi.Controllers;

/// <summary>
/// Controller class that exposes endpoints related to Account Balances
/// </summary>
[ApiController]
[Route("/accountBalance")]
public class AccountBalanceController : ControllerBase
{
    private readonly IAccountRepository _accountRepository;
    private readonly IAccountBalanceService _accountBalanceService;

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="accountRepository">Repository of Accounts</param>
    /// <param name="accountBalanceService">Service used to calculate account balances</param>
    public AccountBalanceController(IAccountRepository accountRepository, IAccountBalanceService accountBalanceService)
    {
        _accountRepository = accountRepository;
        _accountBalanceService = accountBalanceService;
    }

    /// <summary>
    /// Retrieves the balance for the provided Account as of the provided date
    /// </summary>
    /// <param name="accountId">ID of the Account to retrieve the balance for</param>
    /// <param name="asOfDate">Date to retrieve the account balance as of</param>
    /// <returns>The balance of the provided Account as of the provided date</returns>
    [HttpGet("{accountId}/{asOfDate}")]
    public IActionResult GetAccountBalanceForDate(Guid accountId, DateOnly asOfDate)
    {
        Account? account = _accountRepository.FindOrNull(accountId);
        if (account == null)
        {
            return NotFound();
        }
        AccountBalance accountBalance = _accountBalanceService.GetAccountBalanceAsOfDate(account, asOfDate);
        return Ok(new AccountBalanceByDateModel
        {
            Date = asOfDate,
            Balance = accountBalance.Balance,
            BalanceIncludingPendingTransactions = accountBalance.BalanceIncludingPendingTransactions
        });
    }
}