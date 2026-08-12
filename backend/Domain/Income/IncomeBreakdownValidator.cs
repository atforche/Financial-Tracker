using Domain.Payroll;
using Domain.Validation;

namespace Domain.Income;

/// <summary>
/// Validates the intrinsic rules of an income breakdown.
/// </summary>
public static class IncomeBreakdownValidator
{
    /// <summary>
    /// Validates an income breakdown and returns path-aware validation errors.
    /// </summary>
    public static IEnumerable<ValidationError> Validate(
        IncomeBreakdown income,
        ValidationErrorPath path) => income switch
        {
            SimpleIncome simpleIncome => ValidateSimpleIncome(simpleIncome, path),
            PayrollPayment payrollPayment => PayrollPaymentValidator.Validate(payrollPayment, path),
            _ => [new ValidationError(path, "Unsupported income breakdown type.")],
        };

    private static IEnumerable<ValidationError> ValidateSimpleIncome(
        SimpleIncome income,
        ValidationErrorPath path)
    {
        if (income.TrackedAmount < 0 || income.UntrackedAmount < 0 || income.TotalAmount <= 0)
        {
            return [new ValidationError(path, "Simple income amounts must be non-negative and have a positive total.")];
        }
        return [];
    }
}