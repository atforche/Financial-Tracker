namespace Data.Accounts;

/// <summary>
/// Filter class responsible for filtering Accounts based on the specified criteria
/// </summary>
internal sealed class AccountFilterer(DatabaseContext databaseContext)
{
    /// <summary>
    /// Gets the Accounts that match the specified criteria
    /// </summary>
    public List<AccountSortModel> Get(GetAccountsRequest request)
    {
        var accountsWithBalance = databaseContext.Accounts.GroupJoin(databaseContext.AccountBalanceHistories,
            account => account.Id,
            history => history.Account.Id,
            (account, histories) => new
            {
                account,
                currentBalance = histories.OrderByDescending(history => history.Date).ThenByDescending(history => history.Sequence).FirstOrDefault()
            }).AsQueryable();
        if (request.Names != null && request.Names.Count > 0)
        {
            accountsWithBalance = accountsWithBalance.Where(pair => request.Names.Contains(pair.account.Name));
        }
        if (request.Types != null && request.Types.Count > 0)
        {
            accountsWithBalance = accountsWithBalance.Where(pair => request.Types.Contains(pair.account.Type));
        }
        return accountsWithBalance.ToList().Select(pair => new AccountSortModel
        {
            Account = pair.account,
            PostedBalance = pair.currentBalance?.ToAccountBalance().PostedBalance ?? 0.00m,
            AvailableToSpend = pair.currentBalance?.ToAccountBalance().AvailableToSpend ?? 0.00m
        }).ToList();
    }
}