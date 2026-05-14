using Domain.AccountingPeriods;

namespace Domain.Funds;

/// <summary>
/// Entity class representing a Fund
/// </summary>
/// <remarks>
/// A Fund represents a collection of money that the user has allocated for a specific purpose. 
/// Funds can be used to track savings goals, monthly expenses, or any other financial goal the user may have. 
/// Each Fund can optionally be marked as a system fund when it is managed by the application rather than by the user.
/// </remarks>
public class Fund : Entity<FundId>
{
    /// <summary>
    /// Name for this Fund
    /// </summary>
    public string Name { get; internal set; }

    /// <summary>
    /// Description for this Fund
    /// </summary>
    public string Description { get; internal set; }

    /// <summary>
    /// Accounting Period that this Fund was opened in
    /// </summary>
    public AccountingPeriodId? OpeningAccountingPeriodId { get; private set; }

    /// <summary>
    /// Balance assigned during onboarding before accounting periods exist
    /// </summary>
    public decimal? OnboardedBalance { get; internal set; }

    /// <summary>
    /// Name of the Unassigned fund
    /// </summary>
    public const string UnassignedFundName = "Unassigned";

    /// <summary>
    /// Description of the Unassigned fund
    /// </summary>
    public const string UnassignedFundDescription = "Fund that tracks money that has not been assigned to a specific fund";

    /// <summary>
    /// True if this Fund is the Unassigned fund, false otherwise
    /// </summary>
    public bool IsUnassignedFund => Name == UnassignedFundName;

    /// <summary>
    /// True if this Fund was created during onboarding, false otherwise
    /// </summary>
    public bool IsOnboarded => OpeningAccountingPeriodId == null;

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal Fund(string name, string description, AccountingPeriodId openingAccountingPeriodId)
        : base(new FundId(Guid.NewGuid()))
    {
        Name = name;
        Description = description;
        OpeningAccountingPeriodId = openingAccountingPeriodId;
        OnboardedBalance = null;
    }

    /// <summary>
    /// Constructs a new onboarded instance of this class
    /// </summary>
    internal Fund(string name, string description, decimal onboardedBalance)
        : base(new FundId(Guid.NewGuid()))
    {
        Name = name;
        Description = description;
        OpeningAccountingPeriodId = null;
        OnboardedBalance = onboardedBalance;
    }

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    private Fund()
        : base()
    {
        Name = "";
        Description = "";
        OpeningAccountingPeriodId = null;
    }
}

/// <summary>
/// Value object class representing the ID of an <see cref="Fund"/>
/// </summary>
public record FundId : EntityId
{
    /// <summary>
    /// Constructs a new instance of this class. 
    /// </summary>
    internal FundId(Guid value)
        : base(value)
    {
    }
}