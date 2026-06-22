using Data.Accounts;
using Data.Transactions;
using Domain.Accounts;
using Domain.Transactions;
using Domain.Transactions.Accounts;
using Domain.Transactions.Income;
using Domain.Transactions.Spending;
using Models.Accounts;

namespace Rest.Accounts;

/// <summary>
/// Class that handles retrieving current Account data.
/// </summary>
public class CurrentAccountsGetter(
    AccountRepository accountRepository,
    TransactionRepository transactionRepository,
    AccountConverter accountConverter,
    AccountSummaryGetter accountSummaryGetter)
{
    /// <summary>
    /// Retrieves the current Accounts page data.
    /// </summary>
    public CurrentAccountsModel Get(CurrentAccountsQueryParameterModel request)
    {
        HashSet<AccountType>? accountTypes = null;
        if (request.AccountType is { Count: > 0 } requestAccountTypes)
        {
            accountTypes = [];
            foreach (AccountTypeModel requestAccountType in requestAccountTypes)
            {
                if (AccountTypeConverter.TryToDomain(requestAccountType, out AccountType? accountType))
                {
                    _ = accountTypes.Add(accountType.Value);
                }
            }
        }

        HashSet<string>? requestedAccountNames = NormalizeNames(request.AccountName);

        var baseAccounts = accountRepository.GetAll()
            .Where(account => accountTypes == null || accountTypes.Contains(account.Type))
            .OrderBy(account => account.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(account => account.Id.Value)
            .ToList();

        var availableAccountNames = baseAccounts
            .Select(account => account.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToList();

        HashSet<string>? applicableAccountNames = GetApplicableNames(requestedAccountNames, availableAccountNames);
        var accounts = baseAccounts
            .Where(account => applicableAccountNames == null || applicableAccountNames.Contains(account.Name))
            .ToList();

        var balanceEventsByAccountId = transactionRepository.GetAll()
            .SelectMany(BuildBalanceEvents)
            .GroupBy(balanceEvent => balanceEvent.AccountId.Value)
            .ToDictionary(
                grouping => grouping.Key,
                grouping => SortBalanceEvents(grouping.ToList()));

        return new CurrentAccountsModel
        {
            AvailableAccountNames = availableAccountNames,
            Summary = accountSummaryGetter.Get(accounts),
            Accounts = accounts
                .Select(account => ToModel(
                    account,
                    balanceEventsByAccountId.GetValueOrDefault(account.Id.Value) ?? []))
                .ToList(),
        };
    }

    private static HashSet<string>? NormalizeNames(IReadOnlyCollection<string>? names)
    {
        if (names is not { Count: > 0 })
        {
            return null;
        }

        var normalizedNames = names
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return normalizedNames.Count == 0 ? null : normalizedNames;
    }

    private static HashSet<string>? GetApplicableNames(
        IReadOnlySet<string>? requestedNames,
        IReadOnlyCollection<string> availableNames)
    {
        if (requestedNames == null)
        {
            return null;
        }

        var applicableNames = availableNames
            .Where(requestedNames.Contains)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return applicableNames.Count == 0 ? null : applicableNames;
    }

    private CurrentAccountModel ToModel(
        Account account,
        IReadOnlyList<CurrentAccountBalanceEventRow> balanceEvents)
    {
        AccountModel accountModel = accountConverter.ToModel(account);

        return new CurrentAccountModel
        {
            Id = accountModel.Id,
            Name = accountModel.Name,
            Type = accountModel.Type,
            CurrentBalance = accountModel.CurrentBalance,
            LastBalanceEventDate = balanceEvents.Count > 0 ? balanceEvents[0].Date : null,
            RecentBalanceEvents = balanceEvents
                .Take(5)
                .Select(ToModel)
                .ToList(),
        };
    }

    private static List<CurrentAccountBalanceEventRow> SortBalanceEvents(
        IReadOnlyList<CurrentAccountBalanceEventRow> balanceEvents) => balanceEvents
        .OrderByDescending(balanceEvent => balanceEvent.Date)
        .ThenByDescending(balanceEvent => balanceEvent.TransactionDate)
        .ThenByDescending(balanceEvent => balanceEvent.Sequence)
        .ThenByDescending(balanceEvent => balanceEvent.TransactionId)
        .ThenBy(balanceEvent => balanceEvent.Type)
        .ToList();

    private static IEnumerable<CurrentAccountBalanceEventRow> BuildBalanceEvents(
        Transaction transaction)
    {
        switch (transaction)
        {
            case SpendingTransaction spendingTransaction:
                yield return new CurrentAccountBalanceEventRow(
                    spendingTransaction.Source.Account.Id,
                    spendingTransaction.Source.PostedDate ?? transaction.Date,
                    AccountTrendsBalanceEventTypeModel.Debit,
                    spendingTransaction.Source.PostedDate != null,
                    transaction.Amount,
                    transaction.Date,
                    transaction.Sequence,
                    transaction.Id.Value);
                foreach (SpendingTransactionDestination destination in spendingTransaction.Destinations)
                {
                    if (destination.Account != null)
                    {
                        yield return new CurrentAccountBalanceEventRow(
                            destination.Account.Id,
                            destination.PostedDate ?? transaction.Date,
                            AccountTrendsBalanceEventTypeModel.Credit,
                            destination.PostedDate != null,
                            destination.Amount,
                            transaction.Date,
                            transaction.Sequence,
                            transaction.Id.Value);
                    }
                }

                yield break;
            case IncomeTransaction incomeTransaction:
                if (incomeTransaction.Source.Account != null)
                {
                    yield return new CurrentAccountBalanceEventRow(
                        incomeTransaction.Source.Account.Id,
                        incomeTransaction.Source.PostedDate ?? transaction.Date,
                        AccountTrendsBalanceEventTypeModel.Debit,
                        incomeTransaction.Source.PostedDate != null,
                        transaction.Amount,
                        transaction.Date,
                        transaction.Sequence,
                        transaction.Id.Value);
                }

                foreach (IncomeTransactionDestination destination in incomeTransaction.Destinations)
                {
                    yield return new CurrentAccountBalanceEventRow(
                        destination.Account.Id,
                        destination.PostedDate ?? transaction.Date,
                        AccountTrendsBalanceEventTypeModel.Credit,
                        destination.PostedDate != null,
                        destination.Amount,
                        transaction.Date,
                        transaction.Sequence,
                        transaction.Id.Value);
                }

                yield break;
            case AccountTransaction accountTransaction:
                if (accountTransaction.Source.Account != null)
                {
                    yield return new CurrentAccountBalanceEventRow(
                        accountTransaction.Source.Account.Id,
                        accountTransaction.Source.PostedDate ?? transaction.Date,
                        AccountTrendsBalanceEventTypeModel.Debit,
                        accountTransaction.Source.PostedDate != null,
                        transaction.Amount,
                        transaction.Date,
                        transaction.Sequence,
                        transaction.Id.Value);
                }
                foreach (AccountTransactionDestination destination in accountTransaction.Destinations)
                {
                    if (destination.Account != null)
                    {
                        yield return new CurrentAccountBalanceEventRow(
                            destination.Account.Id,
                            destination.PostedDate ?? transaction.Date,
                            AccountTrendsBalanceEventTypeModel.Credit,
                            destination.PostedDate != null,
                            transaction.Amount,
                            transaction.Date,
                            transaction.Sequence,
                            transaction.Id.Value);
                    }
                }

                yield break;
            default:
                yield break;
        }
    }

    private static CurrentAccountBalanceEventModel ToModel(
        CurrentAccountBalanceEventRow balanceEvent) => new()
        {
            TransactionId = balanceEvent.TransactionId,
            Date = balanceEvent.Date,
            Type = balanceEvent.Type,
            IsPosted = balanceEvent.IsPosted,
            Amount = balanceEvent.Amount,
        };

    private sealed record CurrentAccountBalanceEventRow(
        AccountId AccountId,
        DateOnly Date,
        AccountTrendsBalanceEventTypeModel Type,
        bool IsPosted,
        decimal Amount,
        DateOnly TransactionDate,
        int Sequence,
        Guid TransactionId);
}