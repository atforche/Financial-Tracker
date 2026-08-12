namespace Domain.Payroll.Withholding;

/// <summary>
/// Filing statuses supported by the current federal Form W-4.
/// </summary>
public enum FederalFilingStatus
{
    /// <summary>
    /// Single or married filing separately.
    /// </summary>
    SingleOrMarriedFilingSeparately,

    /// <summary>
    /// Married filing jointly or qualifying surviving spouse.
    /// </summary>
    MarriedFilingJointlyOrQualifyingSurvivingSpouse,

    /// <summary>
    /// Head of household.
    /// </summary>
    HeadOfHousehold,
}