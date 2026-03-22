using Domain.AccountingPeriods;
using Models.Accounts;

namespace Rest.Mappers;

/// <summary>
/// Mapper class that handles mapping Account Accounting Period Balance Histories to Accounting Period Account Models
/// </summary>
public sealed class AccountingPeriodAccountMapper(AccountBalanceMapper accountBalanceMapper)
{
    /// <summary>
    /// Maps the provided Account Accounting Period Balance History to an Accounting Period Account Model
    /// </summary>
    public AccountingPeriodAccountModel ToModel(AccountAccountingPeriodBalanceHistory accountAccountingPeriodBalanceHistory) => new()
    {
        Id = accountAccountingPeriodBalanceHistory.Account.Id.Value,
        Name = accountAccountingPeriodBalanceHistory.Account.Name,
        Type = AccountTypeMapper.ToModel(accountAccountingPeriodBalanceHistory.Account.Type),
        OpeningBalance = accountBalanceMapper.ToModel(accountAccountingPeriodBalanceHistory.GetOpeningAccountBalance()),
        ClosingBalance = accountBalanceMapper.ToModel(accountAccountingPeriodBalanceHistory.GetClosingAccountBalance())
    };
}