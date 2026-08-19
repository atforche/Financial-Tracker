using Domain.AccountingPeriods;

namespace Domain.Funds;

/// <summary>
/// Entity class representing a Fund
/// </summary>
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
    /// True if this Fund was created during onboarding, false otherwise
    /// </summary>
    public bool IsOnboarded => OpeningAccountingPeriodId == null;

    #region Unassigned Fund

    /// <summary>
    /// ID of the Unassigned Fund
    /// </summary>
    public static readonly FundId UnassignedFundId = new(new("51A70FF9-49DA-4463-88FD-818B17ACF5C4"));

    /// <summary>
    /// Name of the Unassigned fund
    /// </summary>
    public const string UnassignedFundName = "Unassigned";

    /// <summary>
    /// Description of the Unassigned fund
    /// </summary>
    public const string UnassignedFundDescription = "Money that has not been assigned to a specific fund";

    /// <summary>
    /// True if this Fund is the Unassigned fund, false otherwise
    /// </summary>
    public bool IsUnassignedFund => Id == UnassignedFundId;

    #endregion

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
    /// Constructs a new instance of this class representing the unassigned fund
    /// </summary>
    internal Fund(AccountingPeriodId openingAccountingPeriodId)
        : base(UnassignedFundId)
    {
        Name = UnassignedFundName;
        Description = UnassignedFundDescription;
        OpeningAccountingPeriodId = openingAccountingPeriodId;
        OnboardedBalance = null;
    }

    /// <summary>
    /// Constructs a new instance of this class representing the unassigned fund
    /// </summary>
    internal Fund(decimal onboardedBalance)
        : base(UnassignedFundId)
    {
        Name = UnassignedFundName;
        Description = UnassignedFundDescription;
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
