using System.Text.Json.Serialization;
using Domain.Transactions;

namespace Rest.Models.Transactions;

/// <summary>
/// REST model representing a <see cref="TransactionBalanceEvent"/>
/// </summary>
public class TransactionBalanceEventModel
{
    /// <inheritdoc cref="TransactionBalanceEvent.EventDate"/>
    public DateOnly EventDate { get; init; }

    /// <inheritdoc cref="TransactionBalanceEvent.Parts"/>
    public IReadOnlyCollection<TransactionBalanceEventPartType> Parts { get; init; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    [JsonConstructor]
    public TransactionBalanceEventModel(DateOnly eventDate, ICollection<TransactionBalanceEventPartType> parts)
    {
        EventDate = eventDate;
        Parts = parts.ToList();
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="transactionBalanceEvent">Transaction Balance Event entity to build this REST model from</param>
    public TransactionBalanceEventModel(TransactionBalanceEvent transactionBalanceEvent)
    {
        EventDate = transactionBalanceEvent.EventDate;
        Parts = transactionBalanceEvent.Parts.Select(part => part.Type).ToList();
    }
}