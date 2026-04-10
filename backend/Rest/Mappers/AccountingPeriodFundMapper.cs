using Domain.AccountingPeriods;
using Models.Funds;

namespace Rest.Mappers;

/// <summary>
/// Mapper class that handles mapping Fund Accounting Period Balance Histories to Accounting Period Fund Models
/// </summary>
public sealed class AccountingPeriodFundMapper(FundBalanceMapper fundBalanceMapper)
{
    /// <summary>
    /// Maps the provided Fund Accounting Period Balance History to an Accounting Period Fund Model
    /// </summary>
    public AccountingPeriodFundModel ToModel(FundAccountingPeriodBalanceHistory fundAccountingPeriodBalanceHistory) => new()
    {
        Id = fundAccountingPeriodBalanceHistory.Fund.Id.Value,
        Name = fundAccountingPeriodBalanceHistory.Fund.Name,
        Type = FundTypeMapper.ToModel(fundAccountingPeriodBalanceHistory.Fund.Type),
        Description = fundAccountingPeriodBalanceHistory.Fund.Description,
        OpeningBalance = fundBalanceMapper.ToModel(fundAccountingPeriodBalanceHistory.GetOpeningFundBalance()),
        ClosingBalance = fundBalanceMapper.ToModel(fundAccountingPeriodBalanceHistory.GetClosingFundBalance())
    };
}