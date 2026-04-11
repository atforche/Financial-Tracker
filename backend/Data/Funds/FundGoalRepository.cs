using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Domain.AccountingPeriods;
using Domain.Funds;

namespace Data.Funds;

/// <summary>
/// Repository that allows Fund Goals to be persisted to the database
/// </summary>
public class FundGoalRepository(DatabaseContext databaseContext) : IFundGoalRepository
{
    #region IFundGoalRepository

    /// <inheritdoc/>
    public FundGoal GetById(FundGoalId id) =>
        databaseContext.FundGoals.Single(fundGoal => fundGoal.Id == id);

    /// <inheritdoc/>
    public IReadOnlyCollection<FundGoal> GetAllByFund(FundId fundId) =>
        databaseContext.FundGoals.Where(fundGoal => fundGoal.Fund.Id == fundId).ToList();

    /// <inheritdoc/>
    public IReadOnlyCollection<FundGoal> GetAllByAccountingPeriod(AccountingPeriodId accountingPeriodId) =>
        databaseContext.FundGoals.Where(fundGoal => fundGoal.AccountingPeriodId == accountingPeriodId).ToList();

    /// <inheritdoc/>
    public FundGoal? GetByFundAndAccountingPeriod(FundId fundId, AccountingPeriodId accountingPeriodId) =>
        databaseContext.FundGoals.FirstOrDefault(fundGoal =>
            fundGoal.Fund.Id == fundId &&
            fundGoal.AccountingPeriodId == accountingPeriodId);

    /// <inheritdoc/>
    public void Add(FundGoal fundGoal) => databaseContext.Add(fundGoal);

    /// <inheritdoc/>
    public void Delete(FundGoal fundGoal) => databaseContext.Remove(fundGoal);

    #endregion

    /// <summary>
    /// Attempts to get the Fund Goal with the specified ID
    /// </summary>
    public bool TryGetById(Guid id, [NotNullWhen(true)] out FundGoal? fundGoal)
    {
        fundGoal = databaseContext.FundGoals.FirstOrDefault(goal => ((Guid)(object)goal.Id) == id);
        return fundGoal != null;
    }

    /// <summary>
    /// Gets the Fund Goals for the provided Fund that match the specified criteria
    /// </summary>
    public PaginatedCollection<FundGoal> GetManyByFund(FundId fundId, GetFundGoalsRequest request)
    {
        var fundGoals = databaseContext.FundGoals
            .Where(fundGoal => fundGoal.Fund.Id == fundId)
            .ToList();

        var accountingPeriods = databaseContext.AccountingPeriods
            .Where(accountingPeriod => fundGoals.Select(fundGoal => fundGoal.AccountingPeriodId).Contains(accountingPeriod.Id))
            .ToDictionary(accountingPeriod => accountingPeriod.Id);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            string search = request.Search.Trim();
            fundGoals = fundGoals
                .Where(fundGoal => DoesFundGoalMatchSearch(fundGoal, accountingPeriods[fundGoal.AccountingPeriodId], search))
                .ToList();
        }

        fundGoals = SortFundGoals(fundGoals, accountingPeriods, request.Sort);

        return new PaginatedCollection<FundGoal>
        {
            Items = GetPagedFundGoals(fundGoals, request.Limit, request.Offset),
            TotalCount = fundGoals.Count,
        };
    }

    /// <summary>
    /// Determines whether the provided Fund Goal matches the provided search text
    /// </summary>
    private static bool DoesFundGoalMatchSearch(FundGoal fundGoal, AccountingPeriod accountingPeriod, string search) =>
        MatchesSearch(accountingPeriod.Name, search) ||
        MatchesSearch(accountingPeriod.Year.ToString(CultureInfo.InvariantCulture), search) ||
        MatchesSearch(accountingPeriod.Month.ToString(CultureInfo.InvariantCulture), search) ||
        MatchesSearch(fundGoal.GoalAmount.ToString(CultureInfo.InvariantCulture), search) ||
        MatchesSearch(fundGoal.IsGoalMet.ToString(), search);

    /// <summary>
    /// Determines whether the provided value matches the provided search text
    /// </summary>
    private static bool MatchesSearch(string value, string search) =>
        value.Contains(search, StringComparison.InvariantCultureIgnoreCase);

    /// <summary>
    /// Sorts the provided Fund Goals according to the provided sort order
    /// </summary>
    private static List<FundGoal> SortFundGoals(
        List<FundGoal> fundGoals,
        Dictionary<AccountingPeriodId, AccountingPeriod> accountingPeriods,
        FundGoalSortOrder? sort) =>
        sort switch
        {
            null or FundGoalSortOrder.AccountingPeriod => fundGoals
                .OrderBy(fundGoal => accountingPeriods[fundGoal.AccountingPeriodId].Year)
                .ThenBy(fundGoal => accountingPeriods[fundGoal.AccountingPeriodId].Month)
                .ThenBy(fundGoal => fundGoal.Id.Value)
                .ToList(),
            FundGoalSortOrder.AccountingPeriodDescending => fundGoals
                .OrderByDescending(fundGoal => accountingPeriods[fundGoal.AccountingPeriodId].Year)
                .ThenByDescending(fundGoal => accountingPeriods[fundGoal.AccountingPeriodId].Month)
                .ThenBy(fundGoal => fundGoal.Id.Value)
                .ToList(),
            FundGoalSortOrder.GoalAmount => fundGoals
                .OrderBy(fundGoal => fundGoal.GoalAmount)
                .ThenBy(fundGoal => accountingPeriods[fundGoal.AccountingPeriodId].Year)
                .ThenBy(fundGoal => accountingPeriods[fundGoal.AccountingPeriodId].Month)
                .ToList(),
            FundGoalSortOrder.GoalAmountDescending => fundGoals
                .OrderByDescending(fundGoal => fundGoal.GoalAmount)
                .ThenBy(fundGoal => accountingPeriods[fundGoal.AccountingPeriodId].Year)
                .ThenBy(fundGoal => accountingPeriods[fundGoal.AccountingPeriodId].Month)
                .ToList(),
            FundGoalSortOrder.IsGoalMet => fundGoals
                .OrderBy(fundGoal => fundGoal.IsGoalMet)
                .ThenBy(fundGoal => accountingPeriods[fundGoal.AccountingPeriodId].Year)
                .ThenBy(fundGoal => accountingPeriods[fundGoal.AccountingPeriodId].Month)
                .ToList(),
            FundGoalSortOrder.IsGoalMetDescending => fundGoals
                .OrderByDescending(fundGoal => fundGoal.IsGoalMet)
                .ThenBy(fundGoal => accountingPeriods[fundGoal.AccountingPeriodId].Year)
                .ThenBy(fundGoal => accountingPeriods[fundGoal.AccountingPeriodId].Month)
                .ToList(),
            _ => throw new ArgumentOutOfRangeException(nameof(sort), sort, null),
        };

    /// <summary>
    /// Gets the paged collection of Fund Goals based on the provided request
    /// </summary>
    private static List<FundGoal> GetPagedFundGoals(List<FundGoal> sortedFundGoals, int? limit, int? offset)
    {
        if (offset != null)
        {
            sortedFundGoals = sortedFundGoals.Skip(offset.Value).ToList();
        }
        if (limit != null)
        {
            sortedFundGoals = sortedFundGoals.Take(limit.Value).ToList();
        }
        return sortedFundGoals;
    }
}
