using Domain.Accounts;
using Domain.Income;
using Domain.Payroll;

namespace Domain.Transactions.Income;

/// <summary>
/// Value object representing the source of money for an income transaction.
/// </summary>
public class IncomeTransactionSource
{
    /// <summary>
    /// Account of the income transaction source.
    /// </summary>
    public Account? Account { get; private set; }

    /// <summary>
    /// Posted Date of the income transaction source.
    /// </summary>
    public DateOnly? PostedDate { get; internal set; }

    /// <summary>
    /// Location of the income transaction source.
    /// </summary>
    public string? Location { get; private set; }

    /// <summary>
    /// Economic composition of the income received from this source.
    /// </summary>
    public IncomeBreakdown Income { get; private set; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public IncomeTransactionSource(
        Account? account,
        DateOnly? postedDate,
        string? location,
        IncomeBreakdown income)
    {
        Account = account;
        PostedDate = postedDate;
        Location = location;
        Income = income;
    }

    /// <summary>
    /// Replaces the source details while preserving the owned persistence boundary.
    /// </summary>
    internal void UpdateFrom(IncomeTransactionSource source)
    {
        Account = source.Account;
        PostedDate = source.PostedDate;
        Location = source.Location;
        if (Income is PayrollPayment payroll && source.Income is PayrollPayment updatedPayroll)
        {
            payroll.UpdateFrom(updatedPayroll);
        }
        else
        {
            Income = source.Income;
        }
    }

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    private IncomeTransactionSource()
    {
        Account = null;
        Income = null!;
    }
}