using Domain.Aggregates.Accounts;
using Domain.Services;
using Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using Rest.Models.AccountBalance;
using Rest.Models.DateRange;

namespace Rest.Controllers;

/// <summary>
/// Controller class that exposes endpoints related to Account Balances
/// </summary>
[ApiController]
[Route("/accountBalance")]
internal sealed class AccountBalanceController : ControllerBase
{
    private readonly IAccountRepository _accountRepository;
    private readonly IAccountBalanceService _accountBalanceService;

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="accountRepository">Repository of Accounts</param>
    /// <param name="accountBalanceService">Service that calculates account balances</param>
    public AccountBalanceController(IAccountRepository accountRepository, IAccountBalanceService accountBalanceService)
    {
        _accountRepository = accountRepository;
        _accountBalanceService = accountBalanceService;
    }

    /// <summary>
    /// Retrieves the balance for the provided Account by date across the provided date range
    /// </summary>
    /// <param name="accountId">ID of the Account to retrieve the balance for</param>
    /// <param name="dateRangeModel">Date Range to get the daily Account balances for</param>
    /// <returns>The balance of the provided Account as of the provided date</returns>
    [HttpPost("{accountId}/ByDate")]
    public IActionResult GetAccountBalanceByDate(Guid accountId, DateRangeModel dateRangeModel)
    {
        Account? account = _accountRepository.FindByExternalIdOrNull(accountId);
        if (account == null)
        {
            return NotFound();
        }
        IEnumerable<AccountBalanceByDate> accountBalances =
            _accountBalanceService.GetAccountBalancesByDate(account, dateRangeModel.ConvertToDateRange());
        return Ok(accountBalances.Select(accountBalance => new AccountBalanceByDateModel(accountBalance)));
    }
}