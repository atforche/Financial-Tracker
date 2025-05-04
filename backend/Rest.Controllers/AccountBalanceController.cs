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
internal sealed class AccountBalanceController(IAccountRepository accountRepository, AccountBalanceService accountBalanceService) : ControllerBase
{
    /// <summary>
    /// Retrieves the balance for the provided Account by date across the provided date range
    /// </summary>
    /// <param name="accountId">ID of the Account to retrieve the balance for</param>
    /// <param name="dateRangeModel">Date Range to get the daily Account balances for</param>
    /// <returns>The balance of the provided Account as of the provided date</returns>
    [HttpPost("{accountId}/ByDate")]
    public IActionResult GetAccountBalanceByDate(Guid accountId, DateRangeModel dateRangeModel)
    {
        Account? account = accountRepository.FindByExternalIdOrNull(accountId);
        if (account == null)
        {
            return NotFound();
        }
        IEnumerable<AccountBalanceByDate> accountBalances =
            accountBalanceService.GetAccountBalancesByDate(account, dateRangeModel.ConvertToDateRange());
        return Ok(accountBalances.Select(accountBalance => new AccountBalanceByDateModel(accountBalance)));
    }
}