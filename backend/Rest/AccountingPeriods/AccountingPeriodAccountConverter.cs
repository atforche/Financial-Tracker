using Domain.AccountingPeriods;
using Models.AccountingPeriods;
using Models.Accounts;
using Rest.Accounts;

namespace Rest.AccountingPeriods;

/// <summary>
/// Converter class that handles converting Accounting Period Account Balance Histories to Accounting Period Account Models
/// </summary>
public sealed class AccountingPeriodAccountConverter(AccountConverter accountConverter)
{
    /// <summary>
    /// Converts the provided Accounting Period Account Balance History to an Accounting Period Account Model
    /// </summary>
    public AccountingPeriodAccountModel ToModel(AccountingPeriodAccountBalanceHistory accountAccountingPeriodBalanceHistory)
    {
        AccountModel accountResult = accountConverter.ToModel(accountAccountingPeriodBalanceHistory.Account);
        return new()
        {
            Id = accountResult.Id,
            Name = accountResult.Name,
            Type = accountResult.Type,
            CurrentBalance = accountResult.CurrentBalance,
            OpeningBalance = accountAccountingPeriodBalanceHistory.OpeningBalance,
            ClosingBalance = accountAccountingPeriodBalanceHistory.ClosingBalance
        };
    }
}