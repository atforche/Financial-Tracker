using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Microsoft.AspNetCore.Mvc;
using Models.AccountingPeriods;

namespace Rest.Mappers;

/// <summary>
/// Mapper class that handles mapping Accounting Periods to Accounting Period Models
/// </summary>
public sealed class AccountingPeriodMapper(IAccountingPeriodRepository accountingPeriodRepository)
{
    /// <summary>
    /// Maps the provided Accounting Period to an Accounting Period Model
    /// </summary>
    public static AccountingPeriodModel ToModel(AccountingPeriod accountingPeriod) => new()
    {
        Id = accountingPeriod.Id.Value,
        Year = accountingPeriod.Year,
        Month = accountingPeriod.Month,
        IsOpen = accountingPeriod.IsOpen
    };

    /// <summary>
    /// Attempts to map the provided ID to an Accounting Period
    /// </summary>
    public bool TryToDomain(
        Guid accountingPeriodId,
        [NotNullWhen(true)] out AccountingPeriod? accountingPeriod,
        [NotNullWhen(false)] out IActionResult? errorResult)
    {
        errorResult = null;
        if (!accountingPeriodRepository.TryFindById(accountingPeriodId, out accountingPeriod))
        {
            errorResult = new NotFoundObjectResult(ErrorMapper.ToModel($"Accounting Period with ID {accountingPeriodId} was not found.", []));
            return false;
        }
        return true;
    }
}