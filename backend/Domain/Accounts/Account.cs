using Domain.AccountingPeriods;

namespace Domain.Accounts;

/// <summary>
/// Entity class representing an Account
/// </summary> 
/// <remarks>
/// An Account represents a financial account that money can be held in and transferred from.
/// </remarks>
public class Account : Entity<AccountId>
{
    /// <summary>
    /// Name for this Account
    /// </summary>
    public string Name { get; internal set; }

    /// <summary>
    /// Type for this Account
    /// </summary>
    public AccountType Type { get; private set; }

    /// <summary>
    /// Accounting Period that this Account was opened in
    /// </summary>
    public AccountingPeriodId? OpeningAccountingPeriodId { get; private set; }

    /// <summary>
    /// Date that this Account was opened
    /// </summary>
    public DateOnly? DateOpened { get; private set; }

    /// <summary>
    /// Balance assigned during onboarding before accounting periods exist
    /// </summary>
    public decimal? OnboardedBalance { get; private set; }

    /// <summary>
    /// True if this Account was created during onboarding, false otherwise
    /// </summary>
    public bool IsOnboarded => OpeningAccountingPeriodId == null && DateOpened == null;

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal Account(
        string name,
        AccountType type,
        AccountingPeriodId? openingAccountingPeriodId,
        DateOnly? dateOpened,
        decimal? onboardedBalance)
        : base(new AccountId(Guid.NewGuid()))
    {
        Name = name;
        Type = type;
        OpeningAccountingPeriodId = openingAccountingPeriodId;
        DateOpened = dateOpened;
        OnboardedBalance = onboardedBalance;
    }

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    private Account() : base()
    {
        Name = "";
        OpeningAccountingPeriodId = null;
    }
}

/// <summary>
/// Value object class representing the ID of an <see cref="Account"/>
/// </summary>
public record AccountId : EntityId
{
    /// <summary>
    /// Constructs a new instance of this class. 
    /// </summary>
    internal AccountId(Guid value)
        : base(value)
    {
    }
}