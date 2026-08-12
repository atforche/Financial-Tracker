namespace Domain.Payroll.Withholding;

/// <summary>
/// Base type for jurisdiction-specific state withholding elections.
/// </summary>
public abstract record StateWithholdingElection(
    PayrollTaxJurisdiction Jurisdiction,
    string FormRevision,
    decimal AdditionalWithholdingPerPayPeriod) : WithholdingElection
{
    /// <summary>
    /// Creates a detached snapshot of the jurisdiction-specific election.
    /// </summary>
    public abstract StateWithholdingElection Snapshot();
}