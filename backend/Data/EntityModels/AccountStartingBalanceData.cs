using Data.ValueObjectModels;
using Domain.Entities;

namespace Data.EntityModels;

/// <summary>
/// Data model representing an Account Starting Balance
/// </summary>
public class AccountStartingBalanceData : IEntityDataModel<AccountStartingBalanceData>
{
    /// <summary>
    /// Database primary key for this Account Starting Balance
    /// </summary>
    public long PrimaryKey { get; set; }

    /// <inheritdoc cref="AccountStartingBalance.Id"/>
    public required Guid Id { get; set; }

    /// <inheritdoc cref="AccountStartingBalance.AccountId"/>
    public required Guid AccountId { get; set; }

    /// <inheritdoc cref="AccountStartingBalance.AccountingPeriodId"/>
    public required Guid AccountingPeriodId { get; set; }

    /// <inheritdoc cref="AccountStartingBalance.StartingFundBalances"/>
    public required ICollection<FundAmountData> StartingFundBalances { get; init; }

    /// <inheritdoc/>
    public void Replace(AccountStartingBalanceData newModel)
    {
        Id = newModel.Id;
        AccountId = newModel.AccountId;
        AccountingPeriodId = newModel.AccountingPeriodId;
        StartingFundBalances.Clear();
        foreach (FundAmountData startingFundBalance in newModel.StartingFundBalances)
        {
            StartingFundBalances.Add(startingFundBalance);
        }
    }
}