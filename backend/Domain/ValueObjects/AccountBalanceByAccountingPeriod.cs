using Domain.Aggregates.AccountingPeriods;

namespace Domain.ValueObjects;

/// <summary>
/// Value object class representing the starting and ending balances across an Accounting Period
/// </summary>
public record AccountBalanceByAccountingPeriod
{
    /// <summary>
    /// Accounting Period for this Account Balance by Accounting Period
    /// </summary>
    public AccountingPeriod AccountingPeriod { get; }

    /// <summary>
    /// Starting Balance for this Account Balance by Accounting Period
    /// </summary>
    public AccountBalance StartingBalance { get; }

    /// <summary>
    /// Ending Balance for this Account Balance by Accounting Period
    /// </summary>
    public AccountBalance EndingBalance { get; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="accountingPeriod">Accounting Period for this Account Balance by Accounting Period</param>
    /// <param name="startingBalance">Starting Balance for this Account Balance by Accounting Period</param>
    /// <param name="endingBalance">Ending Balance for this Account Balance by Accounting Period</param>
    public AccountBalanceByAccountingPeriod(AccountingPeriod accountingPeriod,
        AccountBalance startingBalance,
        AccountBalance endingBalance)
    {
        AccountingPeriod = accountingPeriod;
        StartingBalance = startingBalance;
        EndingBalance = endingBalance;
        Validate();
    }

    /// <summary>
    /// Validates this Account Balance by Accounting Period
    /// </summary>
    private void Validate()
    {
        if (StartingBalance.PendingFundBalanceChanges.Count != 0)
        {
            throw new InvalidOperationException();
        }
        if (!AccountingPeriod.IsOpen && EndingBalance.PendingFundBalanceChanges.Count != 0)
        {
            throw new InvalidOperationException();
        }
    }
}