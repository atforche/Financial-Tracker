using Domain.Payroll;

namespace Domain.Income;

/// <summary>
/// Creates detached snapshots of supported income breakdowns.
/// </summary>
public static class IncomeBreakdownSnapshot
{
    /// <summary>
    /// Creates a detached copy suitable for a separately owned aggregate.
    /// </summary>
    public static IncomeBreakdown Create(IncomeBreakdown income) => income switch
    {
        SimpleIncome simpleIncome => simpleIncome.Snapshot(),
        PayrollPayment payrollPayment => payrollPayment.Snapshot(),
        _ => throw new ArgumentException("Unsupported income breakdown type.", nameof(income)),
    };
}