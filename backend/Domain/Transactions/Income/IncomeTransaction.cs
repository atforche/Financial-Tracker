using Domain.Accounts;
using Domain.Funds;

namespace Domain.Transactions.Income;

/// <summary>
/// Entity class representing an income transaction.
/// </summary>
/// <remarks>
/// An income transaction represents money coming into a tracked account from some external source.
/// The credit to the tracked account can optionally be directly assigned to funds.
/// </remarks>
public class IncomeTransaction : Transaction
{
    private readonly List<IncomeLine> _incomeLines = [];
    private readonly List<IncomeDeduction> _incomeDeductions = [];
    private readonly List<IncomeDestination> _incomeDestinations = [];

    /// <summary>
    /// Source Account ID for this Income Transaction
    /// </summary>
    public AccountId? SourceAccountId { get; private set; }

    /// <summary>
    /// Posted Date for the Source Account of this Income Transaction
    /// </summary>
    public DateOnly? SourcePostedDate { get; internal set; }

    /// <summary>
    /// External location where the money for this Income Transaction came from (if not an account).
    /// </summary>
    public string? SourceLocation { get; private set; }

    /// <summary>
    /// Total tracked income amount for this Income Transaction
    /// </summary>
    public decimal TrackedIncomeAmount { get; private set; }

    /// <summary>
    /// Income Lines for this Income Transaction
    /// </summary>
    public IReadOnlyCollection<IncomeLine> IncomeLines => _incomeLines;

    /// <summary>
    /// Income Deductions for this Income Transaction
    /// </summary>
    public IReadOnlyCollection<IncomeDeduction> IncomeDeductions => _incomeDeductions;

    /// <summary>
    /// Income Destinations for this Income Transaction
    /// </summary>
    public IReadOnlyCollection<IncomeDestination> IncomeDestinations => _incomeDestinations;

    /// <inheritdoc/>
    public override IEnumerable<AccountId> GetAllAffectedAccountIds()
    {
        if (SourceAccountId != null)
        {
            yield return SourceAccountId;
        }
        foreach (AccountId accountId in IncomeDestinations.Select(d => d.Account.Id))
        {
            yield return accountId;
        }
    }

    /// <inheritdoc/>
    public override DateOnly? GetPostedDateForAccount(AccountId accountId)
    {
        if (accountId == SourceAccountId)
        {
            return SourcePostedDate;
        }
        else if (IncomeDestinations.Any(d => d.Account.Id == accountId))
        {
            return IncomeDestinations.First(d => d.Account.Id == accountId).PostedDate;
        }
        return null;
    }

    /// <inheritdoc/>
    public override IEnumerable<FundId> GetAllAffectedFundIds(AccountId? accountId)
    {
        if (accountId == null)
        {
            return IncomeDestinations.SelectMany(d => d.FundAssignments).Select(f => f.FundId);
        }
        if (IncomeDestinations.Any(d => d.Account.Id == accountId))
        {
            return IncomeDestinations.First(d => d.Account.Id == accountId).FundAssignments.Select(f => f.FundId);
        }
        return [];
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal IncomeTransaction(CreateIncomeTransactionRequest request, int sequence)
        : this(request, sequence, TransactionType.Income) { }

    /// <summary>
    /// Constructs a new instance of this class with an explicit TransactionType
    /// </summary>
    protected IncomeTransaction(CreateIncomeTransactionRequest request, int sequence, TransactionType type)
        : base(request, sequence, type)
    {
        SourceAccountId = request.SourceAccount?.Id;
        SourceLocation = request.SourceLocation;
        UpdateIncomeLines(request.IncomeLines);
        UpdateIncomeDeductions(request.IncomeDeductions);
        UpdateIncomeDestinations(request.IncomeDestinations);
    }

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    protected IncomeTransaction()
        : base()
    {
    }

    /// <summary>
    /// Updates the income lines for this income transaction.
    /// </summary>
    internal void UpdateIncomeLines(IReadOnlyCollection<IncomeLine> incomeLines)
    {
        _incomeLines.Clear();
        _incomeLines.AddRange(incomeLines.Select(line => new IncomeLine(line.Description, line.Amount)));
    }

    /// <summary>
    /// Updates the income deductions for this income transaction.
    /// </summary>
    internal void UpdateIncomeDeductions(IReadOnlyCollection<IncomeDeduction> incomeDeductions)
    {
        _incomeDeductions.Clear();
        _incomeDeductions.AddRange(incomeDeductions.Select(deduction => new IncomeDeduction(deduction.Description, deduction.Amount)));
    }

    /// <summary>
    /// Updates the income destinations for this income transaction.
    /// </summary>
    internal void UpdateIncomeDestinations(IReadOnlyCollection<IncomeDestination> incomeDestinations)
    {
        _incomeDestinations.Clear();
        _incomeDestinations.AddRange(incomeDestinations.Select(destination =>
            new IncomeDestination(destination.Account, destination.Amount, destination.PostedDate, destination.FundAssignments)));
        TrackedIncomeAmount = IncomeDestinations.Where(d => d.Account.Type.IsTracked()).Sum(d => d.Amount);
    }

    /// <summary>
    /// Sets the posted date for a specific account affected by this transaction.
    /// </summary>
    internal void SetPostedDate(AccountId accountId, DateOnly? postedDate)
    {
        if (accountId == SourceAccountId)
        {
            SourcePostedDate = postedDate;
            return;
        }
        int index = _incomeDestinations.FindIndex(d => d.Account.Id == accountId);
        if (index != -1)
        {
            IncomeDestination destination = _incomeDestinations[index];
            _incomeDestinations[index] = new IncomeDestination(destination.Account, destination.Amount, postedDate, destination.FundAssignments);
        }
    }

    /// <summary>
    /// Clears all posted dates for this transaction.
    /// </summary>
    internal void ClearPostedDates()
    {
        SourcePostedDate = null;
        foreach ((int i, IncomeDestination destination) in _incomeDestinations.Index())
        {
            _incomeDestinations[i] = new IncomeDestination(destination.Account, destination.Amount, null, destination.FundAssignments);
        }
    }

    /// <inheritdoc/>
    protected override AccountBalance AddToAccountBalance(AccountBalance existingAccountBalance, bool reverse)
    {
        IncomeDestination? destination = _incomeDestinations.FirstOrDefault(d => d.Account.Id == existingAccountBalance.Account.Id);
        if (destination != null)
        {
            return existingAccountBalance.AddNewPendingCreditAmount(reverse ? -destination.Amount : destination.Amount);
        }
        if (existingAccountBalance.Account.Id == SourceAccountId)
        {
            return existingAccountBalance.AddNewPendingDebitAmount(reverse ? -Amount : Amount);
        }
        return existingAccountBalance;
    }

    /// <inheritdoc/>
    protected override AccountBalance PostToAccountBalance(AccountBalance existingAccountBalance, bool reverse)
    {
        IncomeDestination? destination = _incomeDestinations.FirstOrDefault(d => d.Account.Id == existingAccountBalance.Account.Id);
        if (destination != null)
        {
            return existingAccountBalance.PostPendingCreditAmount(reverse ? -destination.Amount : destination.Amount);
        }
        if (existingAccountBalance.Account.Id == SourceAccountId)
        {
            return existingAccountBalance.PostPendingDebitAmount(reverse ? -Amount : Amount);
        }
        return existingAccountBalance;
    }

    /// <inheritdoc/>
    protected override FundBalance AddToFundBalance(FundBalance existingFundBalance, bool reverse)
    {
        var fundAmounts = IncomeDestinations.SelectMany(d => d.FundAssignments)
            .Where(fundAmount => fundAmount.FundId == existingFundBalance.FundId)
            .ToList();
        if (fundAmounts.Count == 0)
        {
            return existingFundBalance;
        }
        decimal amount = fundAmounts.Sum(f => f.Amount);
        return existingFundBalance.AddNewPendingAmountAssigned(reverse ? -amount : amount);
    }

    /// <inheritdoc/>
    protected override FundBalance PostToFundBalance(FundBalance existingFundBalance, AccountId accountId, bool reverse)
    {
        var fundAmounts = IncomeDestinations.Where(d => d.Account.Id == accountId)
            .SelectMany(d => d.FundAssignments)
            .Where(fundAmount => fundAmount.FundId == existingFundBalance.FundId)
            .ToList();
        if (fundAmounts.Count == 0)
        {
            return existingFundBalance;
        }
        decimal amount = fundAmounts.Sum(f => f.Amount);
        return existingFundBalance.PostPendingAmountAssigned(reverse ? -amount : amount);
    }
}
