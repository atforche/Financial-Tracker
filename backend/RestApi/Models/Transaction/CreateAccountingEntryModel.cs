namespace RestApi.Models.Transaction;

/// <summary>
/// REST model representing a request to create an Accounting Entry
/// </summary>
public class CreateAccountingEntryModel
{
    /// <inheritdoc cref="Domain.ValueObjects.AccountingEntry.Amount"/>
    public required decimal Amount { get; init; }
}