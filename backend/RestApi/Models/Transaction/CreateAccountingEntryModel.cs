namespace RestApi.Models.Transaction;

/// <summary>
/// REST model representing a request to create an AccountingEntry
/// </summary>
public class CreateAccountingEntryModel
{
    /// <see cref="Domain.ValueObjects.AccountingEntry.Amount"/>
    public required decimal Amount { get; init; }
}