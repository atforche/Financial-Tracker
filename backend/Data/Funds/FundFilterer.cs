namespace Data.Funds;

/// <summary>
/// Filter class responsible for filtering Funds based on the specified criteria
/// </summary>
internal sealed class FundFilterer(DatabaseContext databaseContext)
{
    /// <summary>
    /// Gets the Funds that match the specified criteria
    /// </summary>
    public List<FundSortModel> Get(GetFundsRequest request)
    {
        var fundsWithBalance = databaseContext.Funds.GroupJoin(databaseContext.FundBalanceHistories,
            fund => fund.Id,
            history => history.FundId,
            (fund, histories) => new
            {
                fund,
                currentBalance = histories.OrderByDescending(history => history.Date).ThenByDescending(history => history.Sequence).FirstOrDefault()
            }).AsQueryable();
        if (request.Names != null && request.Names.Count > 0)
        {
            fundsWithBalance = fundsWithBalance.Where(fund => request.Names.Contains(fund.fund.Name));
        }
        return fundsWithBalance.ToList().Select(pair => new FundSortModel
        {
            Fund = pair.fund,
            PostedBalance = pair.currentBalance?.ToFundBalance().PostedBalance ?? 0.00m
        }).ToList();
    }
}