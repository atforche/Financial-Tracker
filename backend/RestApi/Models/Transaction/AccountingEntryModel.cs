using Domain.Entities;

namespace RestApi.Models.Transaction;

/// <summary>
/// REST model representing an Accounting Entry
/// </summary>
public class AccountingEntryModel
{
    /// <see cref="Domain.Entities.AccountingEntry.Type"/>
    public required AccountingEntryType Type { get; init; }

    /// <see cref="Domain.Entities.AccountingEntry.Amount"/>
    public required decimal Amount { get; init; }
}