using Domain.Entities;

namespace Domain.ValueObjects;

/// <summary>
/// Value object class representing a Fund Amount.
/// A Fund Amount represents a monetary amount associated with a particular Fund.
/// </summary>
public class FundAmount
{
    /// <summary>
    /// ID of the Fund for this Fund Amount
    /// </summary>
    public Guid FundId { get; }

    /// <summary>
    /// Amount for this Fund Amount
    /// </summary>
    public decimal Amount { get; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="fundId">Fund ID for this Fund Amount</param>
    /// <param name="amount">Amount for this Fund Amount</param>
    internal FundAmount(Guid fundId, decimal amount)
    {
        FundId = fundId;
        Amount = amount;
        Validate();
    }

    /// <summary>
    /// Validates the current Fund Amount
    /// </summary>
    private void Validate()
    {
        if (FundId == Guid.Empty)
        {
            throw new InvalidOperationException();
        }
        if (Amount <= 0)
        {
            throw new InvalidOperationException();
        }
    }
}

/// <summary>
/// Record representing a request to create a Fund Amount
/// </summary>
public record CreateFundAmountRequest
{
    /// <inheritdoc cref="FundAmount.FundId"/>
    public required Fund Fund { get; init; }

    /// <inheritdoc cref="FundAmount.Amount"/>
    public required decimal Amount { get; init; }
}

/// <summary>
/// Interface representing a request to recreate an existing Fund Amount
/// </summary>
public interface IRecreateFundAmountRequest
{
    /// <inheritdoc cref="FundAmount.FundId"/>
    Guid FundId { get; }

    /// <inheritdoc cref="FundAmount.Amount"/>
    decimal Amount { get; }
}