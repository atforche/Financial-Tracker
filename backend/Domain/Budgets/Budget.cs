using Domain.Funds;

namespace Domain.Budgets;

/// <summary>
/// Entity class representing a Budget
/// </summary>
/// <remarks>
/// A Budget is used to track spending against a particular category on a per-accounting period basis.
/// </remarks>
public class Budget : Entity<BudgetId>
{
    /// <summary>
    /// Name for this Budget
    /// </summary>
    public string Name { get; internal set; }

    /// <summary>
    /// Description for this Budget
    /// </summary>
    public string Description { get; internal set; }

    /// <summary>
    /// Type of this Budget
    /// </summary>
    public BudgetType Type { get; private set; }

    /// <summary>
    /// Fund ID linked to this Budget
    /// </summary>
    public FundId FundId { get; private set; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal Budget(string name, string description, BudgetType type, FundId fundId)
        : base(new BudgetId(Guid.NewGuid()))
    {
        Name = name;
        Description = description;
        Type = type;
        FundId = fundId;
    }

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    private Budget()
        : base()
    {
        Name = "";
        Description = "";
        FundId = null!;
    }
}

/// <summary>
/// Value object class representing the ID of a <see cref="Budget"/>
/// </summary>
public record BudgetId : EntityId
{
    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal BudgetId(Guid value) : base(value) { }
}
