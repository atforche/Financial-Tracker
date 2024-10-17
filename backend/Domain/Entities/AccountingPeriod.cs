namespace Domain.Entities;

/// <summary>
/// Entity class representing an Accounting Period
/// </summary>
/// <remarks>
/// An Accounting Period represents a month-long period used to organize 
/// transactions and track balances and budgets.
/// </remarks>
public class AccountingPeriod
{
    /// <summary>
    /// ID for this Accounting Period
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Year for this Accounting Period
    /// </summary>
    public int Year { get; }

    /// <summary>
    /// Month for this Accounting Period
    /// </summary>
    public int Month { get; }

    /// <summary>
    /// Is Open flag for this Accounting Period
    /// </summary>
    /// <remarks>
    /// New Transactions can only be added to an Accounting Period that is open.
    /// Once an Accounting Period has been closed, no changes can be made to any
    /// Transactions that fall within that Accounting Period. Multiple Accounting
    /// Periods can be open at the same time, assuming all the open periods represent
    /// a contiguous period of time. Only the earliest open period can be closed.
    /// </remarks>
    public bool IsOpen { get; internal set; }

    /// <summary>
    /// Reconstructs an existing Accounting Period
    /// </summary>
    /// <param name="request">Request to recreate an Accounting Period</param>
    public AccountingPeriod(IRecreateAccountingPeriodRequest request)
    {
        Id = request.Id;
        Year = request.Year;
        Month = request.Month;
        IsOpen = request.IsOpen;
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="request">Request to create an Accounting Period</param>
    internal AccountingPeriod(CreateAccountingPeriodRequest request)
    {
        Id = Guid.NewGuid();
        Year = request.Year;
        Month = request.Month;
        IsOpen = true;
        Validate();
    }

    /// <summary>
    /// Validates the current Accounting Period
    /// </summary>
    private void Validate()
    {
        if (Id == Guid.Empty)
        {
            throw new InvalidOperationException();
        }
        if (Year < 2020 || Year > 2050)
        {
            throw new InvalidOperationException();
        }
        if (Month <= 0 || Month > 12)
        {
            throw new InvalidOperationException();
        }
    }
}

/// <summary>
/// Record representing a request to create an Accounting Period
/// </summary>
internal sealed record CreateAccountingPeriodRequest
{
    /// <inheritdoc cref="AccountingPeriod.Year"/>
    public required int Year { get; init; }

    /// <inheritdoc cref="AccountingPeriod.Month"/>
    public required int Month { get; init; }
}

/// <summary>
/// Interface representing a request to recreate an existing Accounting Period
/// </summary>
public interface IRecreateAccountingPeriodRequest
{
    /// <inheritdoc cref="AccountingPeriod.Id"/>
    Guid Id { get; }

    /// <inheritdoc cref="AccountingPeriod.Year"/>
    int Year { get; }

    /// <inheritdoc cref="AccountingPeriod.Month"/>
    int Month { get; }

    /// <inheritdoc cref="AccountingPeriod.IsOpen"/>
    bool IsOpen { get; }
}