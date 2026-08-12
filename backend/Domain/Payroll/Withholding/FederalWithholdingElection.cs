namespace Domain.Payroll.Withholding;

/// <summary>
/// Federal Form W-4 withholding election.
/// </summary>
public sealed record FederalWithholdingElection(
    string FormRevision,
    FederalFilingStatus FilingStatus,
    bool UseMultipleJobsAdjustment,
    decimal DependentCreditAnnual,
    decimal OtherIncomeAnnual,
    decimal DeductionsAnnual,
    decimal AdditionalWithholdingPerPayPeriod) : WithholdingElection;