namespace Models.Income;

/// <summary>
/// Government jurisdiction imposing a payroll tax.
/// </summary>
public sealed class PayrollTaxJurisdictionModel
{
    /// <summary>
    /// ISO country code.
    /// </summary>
    public required string CountryCode { get; init; }

    /// <summary>
    /// Optional state or subdivision code.
    /// </summary>
    public string? SubdivisionCode { get; init; }

    /// <summary>
    /// Optional local jurisdiction name.
    /// </summary>
    public string? Locality { get; init; }
}