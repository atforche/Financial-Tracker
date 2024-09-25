namespace RestApi.Models.Transaction;

/// <summary>
/// REST model representing an Accounting Entry
/// </summary>
public class AccountingEntryModel
{
    /// <see cref="Domain.ValueObjects.AccountingEntry.Amount"/>
    public required decimal Amount { get; init; }
}