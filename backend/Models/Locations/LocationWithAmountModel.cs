namespace Models.Locations;

/// <summary>
/// Location endpoint and its signed impact on a Transaction.
/// </summary>
public sealed class LocationWithAmountModel : LocationModel
{
    /// <summary>
    /// Positive for money received and negative for money sent.
    /// </summary>
    public required decimal Amount { get; init; }
}
