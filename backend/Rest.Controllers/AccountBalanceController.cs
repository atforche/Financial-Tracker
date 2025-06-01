using Domain.Accounts;
using Domain.Services;
using Microsoft.AspNetCore.Mvc;
using Rest.Models.Accounts;
using Rest.Models.DateRange;

namespace Rest.Controllers;

/// <summary>
/// Controller class that exposes endpoints related to Account Balances
/// </summary>
[ApiController]
[Route("/accountBalance")]
public sealed class AccountBalanceController(
    AccountBalanceService accountBalanceService,
    AccountIdFactory accountIdFactory) : ControllerBase
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
        IEnumerable<AccountBalanceByDate> accountBalances =
            accountBalanceService.GetAccountBalancesByDateRange(accountIdFactory.Create(accountId), dateRangeModel.ConvertToDateRange());
        return Ok(accountBalances.Select(accountBalance => new AccountBalanceByDateModel(accountBalance)));
    }
}