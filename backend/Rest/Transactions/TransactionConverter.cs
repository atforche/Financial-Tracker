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
using Rest.Accounts;
using Rest.Funds;

namespace Rest.Transactions;

/// <summary>
/// Converter class that handles converting Transactions to Transaction Models
/// </summary>
public sealed class TransactionConverter(
    AccountBalanceService accountBalanceService,
    AccountBalanceConverter accountBalanceConverter,
    FundBalanceService fundBalanceService,
    IAccountingPeriodRepository accountingPeriodRepository,
    IAccountRepository accountRepository,
    IFundRepository fundRepository,
    TransactionRepository transactionRepository)
{
    /// <summary>
    /// Maps the provided Transaction to a Transaction Model
    /// </summary>
    public TransactionModel ToModel(Transaction transaction)
        => transaction switch
        {
            SpendingTransaction spendingTransaction => new SpendingTransactionModel
            {
                Id = transaction.Id.Value,
                AccountingPeriodId = transaction.AccountingPeriodId.Value,
                AccountingPeriodName = accountingPeriodRepository.GetById(transaction.AccountingPeriodId).PeriodStartDate.ToString("MMMM yyyy", CultureInfo.InvariantCulture),
                Date = transaction.Date,
                Sequence = transaction.Sequence,
                Location = transaction.Location,
                Description = transaction.Description,
                Amount = transaction.Amount,
                DebitAccount = BuildAccountModel(transaction, spendingTransaction.DebitAccountId, spendingTransaction.DebitPostedDate, TransactionAccountTypeModel.Debit),
                CreditAccount = spendingTransaction.CreditAccountId != null
                    ? BuildAccountModel(transaction, spendingTransaction.CreditAccountId, spendingTransaction.CreditPostedDate, TransactionAccountTypeModel.Credit)
                    : null,
                FundAssignments = spendingTransaction.FundAssignments.Select(fundAmount => BuildFundModel(transaction, fundAmount)).ToList(),
            },
            IncomeTransaction incomeTransaction => new IncomeTransactionModel
            {
                Id = transaction.Id.Value,
                AccountingPeriodId = transaction.AccountingPeriodId.Value,
                AccountingPeriodName = accountingPeriodRepository.GetById(transaction.AccountingPeriodId).PeriodStartDate.ToString("MMMM yyyy", CultureInfo.InvariantCulture),
                Date = transaction.Date,
                Sequence = transaction.Sequence,
                Location = transaction.Location,
                Description = transaction.Description,
                Amount = transaction.Amount,
                DebitAccount = incomeTransaction.DebitAccountId != null
                    ? BuildAccountModel(transaction, incomeTransaction.DebitAccountId, incomeTransaction.DebitPostedDate, TransactionAccountTypeModel.Debit)
                    : null,
                CreditAccount = BuildAccountModel(transaction, incomeTransaction.CreditAccountId, incomeTransaction.CreditPostedDate, TransactionAccountTypeModel.Credit),
                FundAssignments = incomeTransaction.FundAssignments.Select(fundAmount => BuildFundModel(transaction, fundAmount)).ToList(),
            },
            AccountTransaction accountTransaction => new AccountTransactionModel
            {
                Id = transaction.Id.Value,
                AccountingPeriodId = transaction.AccountingPeriodId.Value,
                AccountingPeriodName = accountingPeriodRepository.GetById(transaction.AccountingPeriodId).PeriodStartDate.ToString("MMMM yyyy", CultureInfo.InvariantCulture),
                Date = transaction.Date,
                Sequence = transaction.Sequence,
                Location = transaction.Location,
                Description = transaction.Description,
                Amount = transaction.Amount,
                DebitAccount = accountTransaction.DebitAccountId != null
                    ? BuildAccountModel(transaction, accountTransaction.DebitAccountId, accountTransaction.DebitPostedDate, TransactionAccountTypeModel.Debit)
                    : null,
                CreditAccount = accountTransaction.CreditAccountId != null
                    ? BuildAccountModel(transaction, accountTransaction.CreditAccountId, accountTransaction.CreditPostedDate, TransactionAccountTypeModel.Credit)
                    : null,
            },
            FundTransaction fundTransaction => new FundTransactionModel
            {
                Id = transaction.Id.Value,
                AccountingPeriodId = transaction.AccountingPeriodId.Value,
                AccountingPeriodName = accountingPeriodRepository.GetById(transaction.AccountingPeriodId).PeriodStartDate.ToString("MMMM yyyy", CultureInfo.InvariantCulture),
                Date = transaction.Date,
                Sequence = transaction.Sequence,
                Location = transaction.Location,
                Description = transaction.Description,
                Amount = transaction.Amount,
                DebitFund = BuildFundModel(transaction, new FundAmount
                {
                    FundId = fundTransaction.DebitFundId,
                    Amount = transaction.Amount
                }),
                CreditFund = BuildFundModel(transaction, new FundAmount
                {
                    FundId = fundTransaction.CreditFundId,
                    Amount = transaction.Amount
                }),
            },
            _ => throw new InvalidOperationException($"Unrecognized transaction type: {transaction.GetType().Name}")
        };

    /// <summary>
    /// Attempts to map the provided ID to a Transaction
    /// </summary>
    public bool TryToDomain(Guid transactionId, [NotNullWhen(true)] out Transaction? transaction) =>
        transactionRepository.TryGetById(transactionId, out transaction);

    /// <summary>
    /// Builds a single TransactionAccountModel for the given account on the given transaction
    /// </summary>
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
            PreviousAccountBalance = accountBalanceConverter.ToModel(accountBalanceService.GetPreviousBalanceForTransaction(transaction, accountId)),
            NewAccountBalance = accountBalanceConverter.ToModel(accountBalanceService.GetNewBalanceForTransaction(transaction, accountId)),
        };
    }

    /// <summary>
    /// Builds a single TransactionFundModel for the given fund.
    /// </summary>
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