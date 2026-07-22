using Domain;
using Domain.BalanceEvents;
using Domain.Funds;
using Domain.Transactions.Accounts;
using Domain.Transactions.Funds;
using Domain.Transactions.Income;
using Domain.Transactions.Queries;
using Domain.Transactions.Spending;
using Models;
using Models.Transactions;
using Models.Transactions.Types;
using Rest.Accounts;
using Rest.FundPlans;
using Rest.Funds;

namespace Rest.Transactions;

/// <summary>
/// Converts interpreted Domain Transaction details to API models.
/// </summary>
public sealed class TransactionConverter(
    AccountBalanceEventConverter accountBalanceEventConverter,
    FundBalanceEventConverter fundBalanceEventConverter,
    FundPlanBalanceEventConverter fundPlanBalanceEventConverter)
{
    /// <summary>
    /// Converts an API Transaction query to Domain criteria.
    /// </summary>
    public TransactionQuery ToDomain(TransactionQueryParameterModel model) => new(
        new TransactionFilter(
            model.Filter?.AccountingPeriodIds ?? [],
            model.Filter?.AccountIds ?? [],
            model.Filter?.FundIds ?? []),
        model.Sort switch
        {
            TransactionSortModel.Date => TransactionSort.Date,
            TransactionSortModel.DateDescending => TransactionSort.DateDescending,
            TransactionSortModel.Description => TransactionSort.Description,
            TransactionSortModel.DescriptionDescending => TransactionSort.DescriptionDescending,
            TransactionSortModel.Amount => TransactionSort.Amount,
            TransactionSortModel.AmountDescending => TransactionSort.AmountDescending,
            TransactionSortModel.AccountingPeriod => TransactionSort.AccountingPeriod,
            TransactionSortModel.AccountingPeriodDescending => TransactionSort.AccountingPeriodDescending,
            TransactionSortModel.Source => TransactionSort.Source,
            TransactionSortModel.SourceDescending => TransactionSort.SourceDescending,
            TransactionSortModel.Destination => TransactionSort.Destination,
            TransactionSortModel.DestinationDescending => TransactionSort.DestinationDescending,
            _ => TransactionSort.DateDescending,
        },
        model.Offset ?? 0,
        model.Limit);

    /// <summary>
    /// Converts an interpreted Domain page to an API collection.
    /// </summary>
    public CollectionModel<TransactionModel> ToModel(QueryPage<TransactionDetails> page) => new()
    {
        Items = page.Items.Select(ToModel).ToList(),
        TotalCount = page.TotalCount,
    };

    /// <summary>
    /// Converts interpreted Transaction details to the polymorphic API model.
    /// </summary>
    public TransactionModel ToModel(TransactionDetails details) => details.Transaction switch
    {
        SpendingTransaction spending => new SpendingTransactionModel
        {
            Id = spending.Id.Value,
            TransactionType = TransactionTypeModel.Spending,
            AccountingPeriodId = details.AccountingPeriod.Id.Value,
            AccountingPeriodName = details.AccountingPeriod.Name,
            Date = spending.Date,
            Sequence = spending.Sequence,
            Description = spending.Description,
            Amount = spending.Amount,
            Source = new SpendingTransactionSourceModel
            {
                Account = accountBalanceEventConverter.ToModel(details.GetAccountEvent(
                    spending.Source.Account,
                    spending.Source.PostedDate,
                    spending.Amount,
                    BalanceEventType.Debit)),
            },
            Destinations = spending.Destinations.Select(destination => new SpendingTransactionDestinationModel
            {
                Account = destination.Account == null ? null : accountBalanceEventConverter.ToModel(details.GetAccountEvent(
                    destination.Account,
                    destination.PostedDate,
                    destination.Amount,
                    BalanceEventType.Credit)),
                Location = destination.Location,
                Amount = destination.Amount,
                PostedDate = destination.PostedDate,
                FundAssignments = destination.FundAssignments.Select(amount =>
                    fundBalanceEventConverter.ToModel(details.GetFundEvent(amount, BalanceEventType.Debit))).ToList(),
                FundPlans = destination.FundAssignments.Where(amount => amount.FundId != Fund.UnassignedFundId).Select(amount =>
                    fundPlanBalanceEventConverter.ToModel(details.GetFundPlanEvent(
                        amount,
                        destination.PostedDate,
                        BalanceEventType.Debit))).ToList(),
            }).ToList(),
        },
        IncomeTransaction income => new IncomeTransactionModel
        {
            Id = income.Id.Value,
            TransactionType = TransactionTypeModel.Income,
            AccountingPeriodId = details.AccountingPeriod.Id.Value,
            AccountingPeriodName = details.AccountingPeriod.Name,
            Date = income.Date,
            Sequence = income.Sequence,
            Description = income.Description,
            Amount = income.Amount,
            TrackedAmount = income.TrackedAmount,
            Source = new IncomeTransactionSourceModel
            {
                Account = income.Source.Account == null ? null : accountBalanceEventConverter.ToModel(details.GetAccountEvent(
                    income.Source.Account,
                    income.Source.PostedDate,
                    income.Amount,
                    BalanceEventType.Debit)),
                Location = income.Source.Location,
                IncomeLines = income.Source.IncomeLines.Select(line => new IncomeLineModel
                {
                    Description = line.Description,
                    Amount = line.Amount,
                }).ToList(),
                IncomeDeductions = income.Source.IncomeDeductions.Select(deduction => new IncomeDeductionModel
                {
                    Description = deduction.Description,
                    Amount = deduction.Amount,
                }).ToList(),
            },
            Destinations = income.Destinations.Select(destination => new IncomeTransactionDestinationModel
            {
                Account = accountBalanceEventConverter.ToModel(details.GetAccountEvent(
                    destination.Account,
                    destination.PostedDate,
                    destination.Amount,
                    BalanceEventType.Credit)),
                Amount = destination.Amount,
                PostedDate = destination.PostedDate,
                FundAssignments = destination.FundAssignments.Select(amount =>
                    fundBalanceEventConverter.ToModel(details.GetFundEvent(amount, BalanceEventType.Credit))).ToList(),
                FundPlans = destination.FundAssignments.Where(amount => amount.FundId != Fund.UnassignedFundId).Select(amount =>
                    fundPlanBalanceEventConverter.ToModel(details.GetFundPlanEvent(
                        amount,
                        destination.PostedDate,
                        BalanceEventType.Credit))).ToList(),
            }).ToList(),
        },
        AccountTransaction account => new AccountTransactionModel
        {
            Id = account.Id.Value,
            TransactionType = TransactionTypeModel.Account,
            AccountingPeriodId = details.AccountingPeriod.Id.Value,
            AccountingPeriodName = details.AccountingPeriod.Name,
            Date = account.Date,
            Sequence = account.Sequence,
            Description = account.Description,
            Amount = account.Amount,
            Source = new AccountTransactionSourceModel
            {
                Account = account.Source.Account == null ? null : accountBalanceEventConverter.ToModel(details.GetAccountEvent(
                    account.Source.Account,
                    account.Source.PostedDate,
                    account.Amount,
                    BalanceEventType.Debit)),
                Location = account.Source.Location,
            },
            Destinations = account.Destinations.Select(destination => new AccountTransactionDestinationModel
            {
                Account = destination.Account == null ? null : accountBalanceEventConverter.ToModel(details.GetAccountEvent(
                    destination.Account,
                    destination.PostedDate,
                    destination.Amount,
                    BalanceEventType.Credit)),
                Location = destination.Location,
                Amount = destination.Amount,
                PostedDate = destination.PostedDate,
            }).ToList(),
        },
        FundTransaction fund => new FundTransactionModel
        {
            Id = fund.Id.Value,
            TransactionType = TransactionTypeModel.Fund,
            AccountingPeriodId = details.AccountingPeriod.Id.Value,
            AccountingPeriodName = details.AccountingPeriod.Name,
            Date = fund.Date,
            Sequence = fund.Sequence,
            Description = fund.Description,
            Amount = fund.Amount,
            Source = new FundTransactionSourceModel
            {
                Fund = fundBalanceEventConverter.ToModel(details.GetFundEvent(
                    new FundAmount { FundId = fund.Source.Fund.Id, Amount = fund.Amount },
                    BalanceEventType.Debit)),
                FundPlan = fund.Source.Fund.Id == Fund.UnassignedFundId ? null : fundPlanBalanceEventConverter.ToModel(
                    details.GetFundPlanEvent(
                        new FundAmount { FundId = fund.Source.Fund.Id, Amount = fund.Amount },
                        fund.Date,
                        BalanceEventType.Debit)),
            },
            Destinations = fund.Destinations.Select(destination => new FundTransactionDestinationModel
            {
                Fund = fundBalanceEventConverter.ToModel(details.GetFundEvent(
                    new FundAmount { FundId = destination.Fund.Id, Amount = destination.Amount },
                    BalanceEventType.Credit)),
                FundPlan = destination.Fund.Id == Fund.UnassignedFundId ? null : fundPlanBalanceEventConverter.ToModel(
                    details.GetFundPlanEvent(
                        new FundAmount { FundId = destination.Fund.Id, Amount = destination.Amount },
                        fund.Date,
                        BalanceEventType.Credit)),
            }).ToList(),
        },
        _ => throw new InvalidOperationException($"Unrecognized Transaction type '{details.Transaction.GetType().Name}'."),
    };
}