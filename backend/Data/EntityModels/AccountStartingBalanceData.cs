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

    /// <see cref="AccountStartingBalance.Id"/>
    public required Guid Id { get; set; }

    /// <see cref="AccountStartingBalance.AccountId"/>
    public required Guid AccountId { get; set; }

    /// <see cref="AccountStartingBalance.AccountingPeriodId"/>
    public required Guid AccountingPeriodId { get; set; }

    /// <see cref="AccountStartingBalance.StartingBalance"/>
    public required decimal StartingBalance { get; set; }

    /// <inheritdoc/>
    public void Replace(AccountStartingBalanceData newModel)
    {
        Id = newModel.Id;
        AccountId = newModel.AccountId;
        AccountingPeriodId = newModel.AccountingPeriodId;
        StartingBalance = newModel.StartingBalance;
    }
}