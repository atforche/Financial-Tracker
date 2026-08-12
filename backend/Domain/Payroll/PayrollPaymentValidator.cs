using Domain.Payroll.Withholding;
using Domain.Validation;

namespace Domain.Payroll;

/// <summary>
/// Validates the intrinsic rules of a payroll payment.
/// </summary>
public static class PayrollPaymentValidator
{
    /// <summary>
    /// Validates a payroll payment and returns path-aware validation errors.
    /// </summary>
    public static IEnumerable<ValidationError> Validate(
        PayrollPayment payment,
        ValidationErrorPath path)
    {
        IEnumerable<ValidationError> errors = [];
        foreach ((int index, PayrollEarning earning) in payment.Earnings.Index())
        {
            if (earning.Amount <= 0)
            {
                errors = errors.Append(new ValidationError(path.AppendWithIndex(nameof(PayrollPayment.Earnings), index), "Payroll earning amounts must be positive."));
            }
        }
        foreach ((int index, EmployeePayrollDeduction deduction) in payment.EmployeeDeductions.Index())
        {
            if (deduction.Amount <= 0)
            {
                errors = errors.Append(new ValidationError(path.AppendWithIndex(nameof(PayrollPayment.EmployeeDeductions), index), "Employee deduction amounts must be positive."));
            }
            if (!IsValidTaxTreatment(deduction.ReducesTaxableWagesFor))
            {
                errors = errors.Append(new ValidationError(path.AppendWithIndex(nameof(PayrollPayment.EmployeeDeductions), index), "Employee deduction tax treatment is invalid."));
            }
        }
        foreach ((int index, EmployerContribution contribution) in payment.EmployerContributions.Index())
        {
            if (contribution.Amount <= 0)
            {
                errors = errors.Append(new ValidationError(path.AppendWithIndex(nameof(PayrollPayment.EmployerContributions), index), "Employer contribution amounts must be positive."));
            }
        }
        foreach ((int index, PayrollTaxWithholding withholding) in payment.TaxWithholdings.Index())
        {
            if (withholding.Amount <= 0)
            {
                errors = errors.Append(new ValidationError(path.AppendWithIndex(nameof(PayrollPayment.TaxWithholdings), index), "Payroll tax withholding amounts must be positive."));
            }
            if (string.IsNullOrWhiteSpace(withholding.Jurisdiction.CountryCode))
            {
                errors = errors.Append(new ValidationError(path.AppendWithIndex(nameof(PayrollPayment.TaxWithholdings), index), "Payroll tax withholding jurisdiction is required."));
            }
            if (withholding.TaxType is PayrollTaxType.SocialSecurity or PayrollTaxType.Medicare
                && withholding.Jurisdiction != PayrollTaxJurisdiction.UnitedStatesFederal)
            {
                errors = errors.Append(new ValidationError(path.AppendWithIndex(nameof(PayrollPayment.TaxWithholdings), index), "Social Security and Medicare withholding must use the United States federal jurisdiction."));
            }
            if (withholding.TaxType == PayrollTaxType.Local && string.IsNullOrWhiteSpace(withholding.Jurisdiction.Locality))
            {
                errors = errors.Append(new ValidationError(path.AppendWithIndex(nameof(PayrollPayment.TaxWithholdings), index), "Local withholding must identify a locality."));
            }
        }
        if (payment.Earnings.Count == 0)
        {
            errors = errors.Append(new ValidationError(path.Append(nameof(PayrollPayment.Earnings)), "Payroll income must have at least one earning."));
        }
        if (payment.TrackedAmount < 0)
        {
            errors = errors.Append(new ValidationError(path, "Payroll deductions and tax withholding cannot exceed gross cash earnings."));
        }
        PayrollTaxableWages taxableWages = payment.GetTaxableWages();
        if (taxableWages.FederalIncome < 0 || taxableWages.StateIncome < 0
            || taxableWages.SocialSecurity < 0 || taxableWages.Medicare < 0)
        {
            errors = errors.Append(new ValidationError(path, "Payroll deductions cannot reduce a taxable wage base below zero."));
        }
        return errors;
    }

    private static bool IsValidTaxTreatment(PayrollTaxTreatment treatment) =>
        (treatment & ~PayrollTaxTreatment.FullyTaxable) == 0;
}