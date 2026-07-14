using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Data.Transactions;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Funds;
using Domain.Transactions;
using Domain.Transactions.Accounts;
using Domain.Transactions.Funds;
using Domain.Transactions.Income;
using Domain.Transactions.Spending;
using Models.Transactions;
using Models.Transactions.Read;
using Rest.Accounts;
using Rest.Funds;

namespace Rest.Transactions;

/// <summary>
/// Converter class that handles converting Transactions to Transaction Models
/// </summary>
public sealed class TransactionConverter(
    AccountBalanceService accountBalanceService,
    FundBalanceService fundBalanceService,
    IAccountingPeriodRepository accountingPeriodRepository,
    IAccountRepository accountRepository,
    IFundRepository fundRepository,
    TransactionRepository transactionRepository)
{
    /// <summary>
    /// Maps the provided Transaction to a Transaction Model.
    /// </summary>
    public TransactionModel ToModel(Transaction transaction)
        => transaction switch
        {
            SpendingTransaction spendingTransaction => new SpendingTransactionModel
            {
                Id = transaction.Id.Value,
                TransactionType = TransactionTypeModel.Spending,
                AccountingPeriodId = transaction.AccountingPeriodId.Value,
                AccountingPeriodName = accountingPeriodRepository.GetById(transaction.AccountingPeriodId).PeriodStartDate.ToString("MMMM yyyy", CultureInfo.InvariantCulture),
                Date = transaction.Date,
                Sequence = transaction.Sequence,
                Description = transaction.Description,
                Amount = transaction.Amount,
                Source = new SpendingTransactionSourceModel
                {
                    Account = BuildAccountModel(transaction, spendingTransaction.Source.Account.Id, spendingTransaction.Source.PostedDate, TransactionAccountTypeModel.Source),
                },
                Destinations = spendingTransaction.Destinations
                    .Select(destination => new SpendingTransactionDestinationModel
                    {
                        Account = destination.Account != null
                            ? BuildAccountModel(transaction, destination.Account.Id, destination.PostedDate, TransactionAccountTypeModel.Destination)
                            : null,
                        Location = destination.Location,
                        Amount = destination.Amount,
                        PostedDate = destination.PostedDate,
                        FundAssignments = destination.FundAssignments.Select(fundAmount => BuildFundModel(transaction, fundAmount)).ToList(),
                    })
                    .ToList(),
            },
            IncomeTransaction incomeTransaction => new IncomeTransactionModel
            {
                Id = transaction.Id.Value,
                TransactionType = TransactionTypeModel.Income,
                AccountingPeriodId = transaction.AccountingPeriodId.Value,
                AccountingPeriodName = accountingPeriodRepository.GetById(transaction.AccountingPeriodId).PeriodStartDate.ToString("MMMM yyyy", CultureInfo.InvariantCulture),
                Date = transaction.Date,
                Sequence = transaction.Sequence,
                Description = transaction.Description,
                Amount = transaction.Amount,
                Source = new IncomeTransactionSourceModel
                {
                    Account = incomeTransaction.Source.Account != null
                        ? BuildAccountModel(transaction, incomeTransaction.Source.Account.Id, incomeTransaction.Source.PostedDate, TransactionAccountTypeModel.Source)
                        : null,
                    Location = incomeTransaction.Source.Location,
                    IncomeLines = incomeTransaction.Source.IncomeLines
                        .Select(line => new IncomeLineModel
                        {
                            Description = line.Description,
                            Amount = line.Amount,
                        })
                        .ToList(),
                    IncomeDeductions = incomeTransaction.Source.IncomeDeductions
                        .Select(deduction => new IncomeDeductionModel
                        {
                            Description = deduction.Description,
                            Amount = deduction.Amount,
                        })
                        .ToList(),
                },
                TrackedAmount = incomeTransaction.TrackedAmount,
                Destinations = incomeTransaction.Destinations
                    .Select(destination => new IncomeTransactionDestinationModel
                    {
                        Account = BuildAccountModel(transaction, destination.Account.Id, destination.PostedDate, TransactionAccountTypeModel.Destination),
                        Amount = destination.Amount,
                        PostedDate = destination.PostedDate,
                        FundAssignments = destination.FundAssignments.Select(fundAmount => BuildFundModel(transaction, fundAmount)).ToList(),
                    })
                    .ToList(),
            },
            AccountTransaction accountTransaction => new AccountTransactionModel
            {
                Id = transaction.Id.Value,
                TransactionType = TransactionTypeModel.Account,
                AccountingPeriodId = transaction.AccountingPeriodId.Value,
                AccountingPeriodName = accountingPeriodRepository.GetById(transaction.AccountingPeriodId).PeriodStartDate.ToString("MMMM yyyy", CultureInfo.InvariantCulture),
                Date = transaction.Date,
                Sequence = transaction.Sequence,
                Description = transaction.Description,
                Amount = transaction.Amount,
                Source = new AccountTransactionSourceModel
                {
                    Account = accountTransaction.Source.Account != null
                        ? BuildAccountModel(transaction, accountTransaction.Source.Account.Id, accountTransaction.Source.PostedDate, TransactionAccountTypeModel.Source)
                        : null,
                    Location = accountTransaction.Source.Location,
                },
                Destinations = accountTransaction.Destinations
                    .Select(destination => new AccountTransactionDestinationModel
                    {
                        Account = destination.Account != null
                            ? BuildAccountModel(transaction, destination.Account.Id, destination.PostedDate, TransactionAccountTypeModel.Destination)
                            : null,
                        Location = destination.Location,
                        Amount = destination.Amount,
                        PostedDate = destination.PostedDate,
                    })
                    .ToList(),
            },
            FundTransaction fundTransaction => new FundTransactionModel
            {
                Id = transaction.Id.Value,
                TransactionType = TransactionTypeModel.Fund,
                AccountingPeriodId = transaction.AccountingPeriodId.Value,
                AccountingPeriodName = accountingPeriodRepository.GetById(transaction.AccountingPeriodId).PeriodStartDate.ToString("MMMM yyyy", CultureInfo.InvariantCulture),
                Date = transaction.Date,
                Sequence = transaction.Sequence,
                Description = transaction.Description,
                Amount = transaction.Amount,
                Source = new FundTransactionSourceModel
                {
                    Fund = BuildFundModel(transaction, new FundAmount
                    {
                        FundId = fundTransaction.Source.Fund.Id,
                        Amount = transaction.Amount
                    })
                },
                Destinations = fundTransaction.Destinations
                    .Select(destination => new FundTransactionDestinationModel
                    {
                        Fund = BuildFundModel(transaction, new FundAmount
                        {
                            FundId = destination.Fund.Id,
                            Amount = destination.Amount,
                        })
                    })
                    .ToList(),
            },
            _ => throw new InvalidOperationException($"Unrecognized transaction type: {transaction.GetType().Name}")
        };

    /// <summary>
    /// Attempts to map the provided ID to a Transaction.
    /// </summary>
    public bool TryToDomain(Guid transactionId, [NotNullWhen(true)] out Transaction? transaction) =>
        transactionRepository.TryGetById(transactionId, out transaction);

    private TransactionAccountModel BuildAccountModel(
        Transaction transaction,
        AccountId accountId,
        DateOnly? postedDate,
        TransactionAccountTypeModel type)
    {
        Account account = accountRepository.GetById(accountId);
        return new TransactionAccountModel
        {
            AccountId = accountId.Value,
            AccountName = account.Name,
            AccountType = AccountTypeConverter.ToModel(account.Type),
            Type = type,
            PostedDate = postedDate,
            PreviousAccountBalance = AccountBalanceConverter.ToModel(accountBalanceService.GetPreviousBalanceForTransaction(transaction, accountId)),
            NewAccountBalance = AccountBalanceConverter.ToModel(accountBalanceService.GetNewBalanceForTransaction(transaction, accountId)),
        };
    }

    private TransactionFundModel BuildFundModel(Transaction transaction, FundAmount fundAmount)
    {
        Fund fund = fundRepository.GetById(fundAmount.FundId);
        FundBalance? previousBalance = fundBalanceService.GetPreviousBalancesForTransaction(transaction)
            .FirstOrDefault(b => b.FundId == fundAmount.FundId);
        FundBalance? newBalance = fundBalanceService.GetNewBalanceForTransaction(transaction)
            .FirstOrDefault(b => b.FundId == fundAmount.FundId);
        return new TransactionFundModel
        {
            FundId = fundAmount.FundId.Value,
            FundName = fund.Name,
            Amount = fundAmount.Amount,
            PreviousFundBalance = FundBalanceConverter.ToModel(fund, previousBalance),
            NewFundBalance = FundBalanceConverter.ToModel(fund, newBalance),
        };
    }
}