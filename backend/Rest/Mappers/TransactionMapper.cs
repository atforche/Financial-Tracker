using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Data.Transactions;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Funds;
using Domain.Transactions;
using Models.Transactions;

namespace Rest.Mappers;

/// <summary>
/// Mapper class that handles mapping Transactions to Transaction Models
/// </summary>
public sealed class TransactionMapper(
    AccountBalanceService accountBalanceService,
    FundBalanceService fundBalanceService,
    AccountBalanceMapper accountBalanceMapper,
    FundAmountMapper fundAmountMapper,
    FundBalanceMapper fundBalanceMapper,
    IAccountingPeriodRepository accountingPeriodRepository,
    IAccountRepository accountRepository,
    TransactionRepository transactionRepository)
{
    /// <summary>
    /// Maps the provided Transaction to a Transaction Model
    /// </summary>
    public TransactionModel ToModel(Transaction transaction)
    {
        (TransactionAccountModel? debitAccount, TransactionAccountModel? creditAccount) = BuildAccountModels(transaction);
        return new TransactionModel
        {
            Id = transaction.Id.Value,
            AccountingPeriodId = transaction.AccountingPeriodId.Value,
            AccountingPeriodName = accountingPeriodRepository.GetById(transaction.AccountingPeriodId).PeriodStartDate.ToString("MMMM yyyy", CultureInfo.InvariantCulture),
            Date = transaction.Date,
            Location = transaction.Location,
            Description = transaction.Description,
            Amount = transaction.Amount,
            DebitAccount = debitAccount,
            CreditAccount = creditAccount,
            PreviousFundBalances = fundBalanceService.GetPreviousBalancesForTransaction(transaction)
                .Select(fundBalanceMapper.ToModel)
                .ToList(),
            NewFundBalances = fundBalanceService.GetNewBalanceForTransaction(transaction)
                .Select(fundBalanceMapper.ToModel)
                .ToList(),
        };
    }

    /// <summary>
    /// Attempts to map the provided ID to a Transaction
    /// </summary>
    public bool TryToDomain(Guid transactionId, [NotNullWhen(true)] out Transaction? transaction) =>
        transactionRepository.TryGetById(transactionId, out transaction);

    /// <summary>
    /// Builds the debit and credit TransactionAccountModels by type-switching on the concrete transaction subtype
    /// </summary>
    private (TransactionAccountModel? debit, TransactionAccountModel? credit) BuildAccountModels(Transaction transaction) => transaction switch
    {
        SpendingTransferTransaction spendingTransfer => (
            BuildAccountModel(transaction, spendingTransfer.AccountId, spendingTransfer.PostedDate, spendingTransfer.FundAmounts, TransactionAccountTypeModel.Debit),
            BuildAccountModel(transaction, spendingTransfer.CreditAccountId, spendingTransfer.CreditPostedDate, [], TransactionAccountTypeModel.Credit)
        ),
        SpendingTransaction spending => (
            BuildAccountModel(transaction, spending.AccountId, spending.PostedDate, spending.FundAmounts, TransactionAccountTypeModel.Debit),
            null
        ),
        IncomeTransferTransaction incomeTransfer => (
            BuildAccountModel(transaction, incomeTransfer.DebitAccountId, incomeTransfer.DebitPostedDate, [], TransactionAccountTypeModel.Debit),
            BuildAccountModel(transaction, incomeTransfer.AccountId, incomeTransfer.PostedDate, incomeTransfer.FundAmounts, TransactionAccountTypeModel.Credit)
        ),
        IncomeTransaction income => (
            null,
            BuildAccountModel(transaction, income.AccountId, income.PostedDate, income.FundAmounts, TransactionAccountTypeModel.Credit)
        ),
        TransferTransaction transfer => (
            BuildAccountModel(transaction, transfer.DebitAccountId, transfer.DebitPostedDate, [], TransactionAccountTypeModel.Debit),
            BuildAccountModel(transaction, transfer.CreditAccountId, transfer.CreditPostedDate, [], TransactionAccountTypeModel.Credit)
        ),
        // RefundTransaction refund => BuildAccountModels(refund.Transaction),
        _ => (null, null),
    };

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
}
