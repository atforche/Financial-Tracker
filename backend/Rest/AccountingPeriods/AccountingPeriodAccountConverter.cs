using Domain.AccountingPeriods;
using Models.AccountingPeriods;
using Rest.Accounts;

namespace Rest.AccountingPeriods;

/// <summary>
/// Converter class that handles converting Accounting Period Account Balance Histories to Accounting Period Account Models
/// </summary>
public sealed class AccountingPeriodAccountConverter
{
    /// <summary>
    /// Converts the provided Accounting Period Account Balance History to an Accounting Period Account Model
    /// </summary>
    public static AccountingPeriodAccountModel ToModel(AccountingPeriodAccountBalanceHistory accountAccountingPeriodBalanceHistory) => new()
    {
        Id = accountAccountingPeriodBalanceHistory.Account.Id.Value,
        Name = accountAccountingPeriodBalanceHistory.Account.Name,
        Type = AccountTypeConverter.ToModel(accountAccountingPeriodBalanceHistory.Account.Type),
        OpeningBalance = accountAccountingPeriodBalanceHistory.OpeningBalance,
        ClosingBalance = accountAccountingPeriodBalanceHistory.ClosingBalance
    };
}