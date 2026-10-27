namespace Models.Overview;

/// <summary>
/// Model representing a group of related items needing attention.
/// </summary>
public class OverviewAttentionGroupModel
{
    /// <summary>
    /// Kind of items in the group.
    /// </summary>
    public required OverviewAttentionKindModel Kind { get; init; }

    /// <summary>
    /// Number of items in the group. For unposted Transactions, the number of Transactions.
    /// </summary>
    public required int Count { get; init; }

    /// <summary>
    /// Total amount of the group: the shortfall for goals and income, or the effect on net worth for unposted
    /// Transactions.
    /// </summary>
    public required decimal Amount { get; init; }

    /// <summary>
    /// Number of items the top items were chosen from. For unposted Transactions, the number of Accounts they affect.
    /// </summary>
    public required int ItemCount { get; init; }

    /// <summary>
    /// The items with the largest amounts. For unposted Transactions, the Accounts they affect.
    /// </summary>
    public required IReadOnlyCollection<OverviewAttentionItemModel> TopItems { get; init; }

    /// <summary>
    /// Earliest date among the items, when the group's items are dated.
    /// </summary>
    public DateOnly? OldestDate { get; init; }
}

/// <summary>
/// Model representing a single item needing attention.
/// </summary>
public class OverviewAttentionItemModel
{
    /// <summary>
    /// Name of the Fund, Account, or expected income source.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Amount for the item.
    /// </summary>
    public required decimal Amount { get; init; }

    /// <summary>
    /// Number of Transactions the item stands for, when it stands for several.
    /// </summary>
    public int? Count { get; init; }
}
