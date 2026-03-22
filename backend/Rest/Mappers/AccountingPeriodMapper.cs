using System.Diagnostics.CodeAnalysis;
using Data.AccountingPeriods;
using Domain.AccountingPeriods;
using Models.AccountingPeriods;

namespace Rest.Mappers;

/// <summary>
/// Mapper class that handles mapping Accounting Periods to Accounting Period Models
/// </summary>
public sealed class AccountingPeriodMapper(
    AccountingPeriodRepository accountingPeriodRepository,
    AccountingPeriodBalanceHistoryRepository accountingPeriodBalanceHistoryRepository)
{
    /// <summary>
    /// Maps the provided Accounting Period to an Accounting Period Model
    /// </summary>
    public AccountingPeriodModel ToModel(AccountingPeriod accountingPeriod)
    {
        AccountingPeriodBalanceHistory balanceHistory = accountingPeriodBalanceHistoryRepository.GetForAccountingPeriod(accountingPeriod.Id);
        return new AccountingPeriodModel
        {
            Id = accountingPeriod.Id.Value,
            Name = accountingPeriod.Name,
            Year = accountingPeriod.Year,
            Month = accountingPeriod.Month,
            IsOpen = accountingPeriod.IsOpen,
            OpeningBalance = balanceHistory.OpeningBalance,
            ClosingBalance = balanceHistory.ClosingBalance
        };
    }

    /// <summary>
    /// Attempts to map the provided ID to an Accounting Period
    /// </summary>
    public bool TryToDomain(Guid accountingPeriodId, [NotNullWhen(true)] out AccountingPeriod? accountingPeriod) =>
        accountingPeriodRepository.TryGetById(accountingPeriodId, out accountingPeriod);
}