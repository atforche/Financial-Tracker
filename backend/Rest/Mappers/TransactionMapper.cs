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

namespace Rest.Mappers;

/// <summary>
/// Mapper class that handles mapping Transactions to Transaction Models
/// </summary>
public sealed class TransactionMapper(
    AccountBalanceService accountBalanceService,
    AccountBalanceMapper accountBalanceMapper,
    FundAmountMapper fundAmountMapper,
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
                Location = transaction.Location,
                Description = transaction.Description,
                Amount = transaction.Amount,
                DebitAccount = BuildAccountModel(transaction, spendingTransaction.DebitAccountId, spendingTransaction.DebitPostedDate, spendingTransaction.FundAssignments, TransactionAccountTypeModel.Debit),
                CreditAccount = spendingTransaction.CreditAccountId != null
                    ? BuildAccountModel(transaction, spendingTransaction.CreditAccountId, spendingTransaction.CreditPostedDate, [], TransactionAccountTypeModel.Credit)
                    : null,
            },
            IncomeTransaction incomeTransaction => new IncomeTransactionModel
            {
                Id = transaction.Id.Value,
                AccountingPeriodId = transaction.AccountingPeriodId.Value,
                AccountingPeriodName = accountingPeriodRepository.GetById(transaction.AccountingPeriodId).PeriodStartDate.ToString("MMMM yyyy", CultureInfo.InvariantCulture),
                Date = transaction.Date,
                Location = transaction.Location,
                Description = transaction.Description,
                Amount = transaction.Amount,
                DebitAccount = incomeTransaction.DebitAccountId != null
                    ? BuildAccountModel(transaction, incomeTransaction.DebitAccountId, incomeTransaction.DebitPostedDate, [], TransactionAccountTypeModel.Debit)
                    : null,
                CreditAccount = BuildAccountModel(transaction, incomeTransaction.CreditAccountId, incomeTransaction.CreditPostedDate, incomeTransaction.FundAssignments, TransactionAccountTypeModel.Credit),
            },
            AccountTransaction accountTransaction => new AccountTransactionModel
            {
                Id = transaction.Id.Value,
                AccountingPeriodId = transaction.AccountingPeriodId.Value,
                AccountingPeriodName = accountingPeriodRepository.GetById(transaction.AccountingPeriodId).PeriodStartDate.ToString("MMMM yyyy", CultureInfo.InvariantCulture),
                Date = transaction.Date,
                Location = transaction.Location,
                Description = transaction.Description,
                Amount = transaction.Amount,
                DebitAccount = accountTransaction.DebitAccountId != null
                    ? BuildAccountModel(transaction, accountTransaction.DebitAccountId, accountTransaction.DebitPostedDate, [], TransactionAccountTypeModel.Debit)
                    : null,
                CreditAccount = accountTransaction.CreditAccountId != null
                    ? BuildAccountModel(transaction, accountTransaction.CreditAccountId, accountTransaction.CreditPostedDate, [], TransactionAccountTypeModel.Credit)
                    : null,
            },
            FundTransaction fundTransaction => new FundTransactionModel
            {
                Id = transaction.Id.Value,
                AccountingPeriodId = transaction.AccountingPeriodId.Value,
                AccountingPeriodName = accountingPeriodRepository.GetById(transaction.AccountingPeriodId).PeriodStartDate.ToString("MMMM yyyy", CultureInfo.InvariantCulture),
                Date = transaction.Date,
                Location = transaction.Location,
                Description = transaction.Description,
                Amount = transaction.Amount,
                DebitFund = BuildFundModel(fundTransaction.DebitFundId),
                CreditFund = BuildFundModel(fundTransaction.CreditFundId),
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
        IEnumerable<FundAmount> fundAmounts,
        TransactionAccountTypeModel type)
    {
        Account account = accountRepository.GetById(accountId);
        return new TransactionAccountModel
        {
            AccountId = accountId.Value,
            AccountName = account.Name,
            AccountType = AccountTypeMapper.ToModel(account.Type),
            Type = type,
            PostedDate = postedDate,
            FundAmounts = fundAmounts.Select(fundAmountMapper.ToModel).ToList(),
            PreviousAccountBalance = accountBalanceMapper.ToModel(accountBalanceService.GetPreviousBalanceForTransaction(transaction, accountId)),
            NewAccountBalance = accountBalanceMapper.ToModel(accountBalanceService.GetNewBalanceForTransaction(transaction, accountId)),
        };
    }

    /// <summary>
    /// Builds a single TransactionFundModel for the given fund.
    /// </summary>
    private TransactionFundModel BuildFundModel(FundId fundId)
    {
        Fund fund = fundRepository.GetById(fundId);
        return new TransactionFundModel
        {
            FundId = fundId.Value,
            FundName = fund.Name,
        };
    }
}