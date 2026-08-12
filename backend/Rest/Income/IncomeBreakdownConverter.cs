using Domain.Income;
using Domain.Payroll;
using Domain.Payroll.Withholding;
using Models.Income;

namespace Rest.Income;

/// <summary>
/// Converts income breakdowns between domain and API models.
/// </summary>
public static class IncomeBreakdownConverter
{
    /// <summary>
    /// Converts an income breakdown to its API representation.
    /// </summary>
    public static IncomeBreakdownModel ToModel(IncomeBreakdown income)
    {
        var payroll = income as PayrollPayment;
        return new IncomeBreakdownModel
        {
            Kind = income switch
            {
                ExpectedPayrollPayment => IncomeBreakdownKindModel.ExpectedPayroll,
                PayrollPayment => IncomeBreakdownKindModel.Payroll,
                _ => IncomeBreakdownKindModel.Simple,
            },
            TrackedAmount = income.TrackedAmount,
            UntrackedAmount = income.UntrackedAmount,
            TotalAmount = income.TotalAmount,
            Earnings = payroll?.Earnings.Select(ToModel).ToList() ?? [],
            EmployeeDeductions = payroll?.EmployeeDeductions.Select(ToModel).ToList() ?? [],
            EmployerContributions = payroll?.EmployerContributions.Select(ToModel).ToList() ?? [],
            TaxWithholdings = payroll?.TaxWithholdings.Select(ToModel).ToList() ?? [],
            StateIncomeStateCode = payroll?.StateIncomeStateCode,
            PayPeriodsPerYear = (payroll as ExpectedPayrollPayment)?.PayPeriodsPerYear,
        };
    }

    /// <summary>
    /// Converts an API request to a supported domain income breakdown.
    /// </summary>
    public static IncomeBreakdown ToDomain(IncomeBreakdownRequestModel model) => model.Kind switch
    {
        IncomeBreakdownKindModel.Simple => new SimpleIncome(model.TrackedAmount ?? 0, model.UntrackedAmount ?? 0),
        IncomeBreakdownKindModel.Payroll => new PayrollPayment(
            model.Earnings.Select(ToDomain),
            model.EmployeeDeductions.Select(ToDomain),
            model.EmployerContributions.Select(ToDomain),
            model.TaxWithholdings.Select(ToDomain),
            model.StateIncomeStateCode),
        IncomeBreakdownKindModel.ExpectedPayroll => new PayrollPayment(
            model.Earnings.Select(ToDomain),
            model.EmployeeDeductions.Select(ToDomain),
            model.EmployerContributions.Select(ToDomain),
            model.TaxWithholdings.Select(ToDomain),
            model.StateIncomeStateCode),
        _ => throw new ArgumentOutOfRangeException(nameof(model)),
    };

    private static PayrollEarningModel ToModel(PayrollEarning earning) => new()
    {
        Description = earning.Description,
        Amount = earning.Amount,
    };

    private static EmployeePayrollDeductionModel ToModel(EmployeePayrollDeduction deduction) => new()
    {
        Description = deduction.Description,
        Amount = deduction.Amount,
        Disposition = (int)deduction.Disposition,
        ReducesTaxableWagesFor = (int)deduction.ReducesTaxableWagesFor,
    };

    private static EmployerContributionModel ToModel(EmployerContribution contribution) => new()
    {
        Description = contribution.Description,
        Amount = contribution.Amount,
    };

    private static PayrollTaxWithholdingModel ToModel(PayrollTaxWithholding withholding) => new()
    {
        Jurisdiction = ToModel(withholding.Jurisdiction),
        TaxType = (int)withholding.TaxType,
        Amount = withholding.Amount,
    };

    private static PayrollTaxJurisdictionModel ToModel(PayrollTaxJurisdiction jurisdiction) => new()
    {
        CountryCode = jurisdiction.CountryCode,
        SubdivisionCode = jurisdiction.SubdivisionCode,
        Locality = jurisdiction.Locality,
    };

    private static PayrollEarning ToDomain(PayrollEarningModel earning) => new(
        earning.Description,
        earning.Amount);

    private static EmployeePayrollDeduction ToDomain(EmployeePayrollDeductionModel deduction) => new(
        deduction.Description,
        deduction.Amount,
        (EmployeeDeductionDisposition)deduction.Disposition,
        (PayrollTaxTreatment)deduction.ReducesTaxableWagesFor);

    private static EmployerContribution ToDomain(EmployerContributionModel contribution) =>
        new(contribution.Description, contribution.Amount);

    private static PayrollTaxWithholding ToDomain(PayrollTaxWithholdingModel withholding) =>
        new(ToDomain(withholding.Jurisdiction), (PayrollTaxType)withholding.TaxType, withholding.Amount);

    private static PayrollTaxJurisdiction ToDomain(PayrollTaxJurisdictionModel jurisdiction) =>
        new(jurisdiction.CountryCode, jurisdiction.SubdivisionCode, jurisdiction.Locality);
}