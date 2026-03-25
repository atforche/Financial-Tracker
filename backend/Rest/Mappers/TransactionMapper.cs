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
    public TransactionModel ToModel(Transaction transaction) => new()
    {
        Id = transaction.Id.Value,
        AccountingPeriodId = transaction.AccountingPeriodId.Value,
        AccountingPeriodName = accountingPeriodRepository.GetById(transaction.AccountingPeriodId).PeriodStartDate.ToString("MMMM yyyy", CultureInfo.InvariantCulture),
        Date = transaction.Date,
        Location = transaction.Location,
        Description = transaction.Description,
        Amount = transaction.Amount,
        DebitAccount = ToModel(transaction.DebitAccount),
        CreditAccount = ToModel(transaction.CreditAccount),
        PreviousFundBalances = fundBalanceService.GetPreviousBalancesForTransaction(transaction)
            .Select(fundBalanceMapper.ToModel)
            .ToList(),
        NewFundBalances = fundBalanceService.GetNewBalanceForTransaction(transaction)
            .Select(fundBalanceMapper.ToModel)
            .ToList(),
    };

    /// <summary>
    /// Attempts to map the provided ID to a Transaction
    /// </summary>
    public bool TryToDomain(Guid transactionId, [NotNullWhen(true)] out Transaction? transaction) =>
        transactionRepository.TryGetById(transactionId, out transaction);

    /// <summary>
    /// Maps the provided Transaction Account to a Transaction Account Model
    /// </summary>
    private TransactionAccountModel? ToModel(TransactionAccount? transactionAccount)
    {
        if (transactionAccount == null)
        {
            return null;
        }
        Account account = accountRepository.GetById(transactionAccount.AccountId);
        return new TransactionAccountModel
        {
            AccountId = transactionAccount.AccountId.Value,
            AccountName = account.Name,
            AccountType = AccountTypeMapper.ToModel(account.Type),
            Type = ToModel(transactionAccount.Type),
            PostedDate = transactionAccount.PostedDate,
            FundAmounts = transactionAccount.FundAmounts.Select(fundAmountMapper.ToModel).ToList(),
            PreviousAccountBalance = accountBalanceMapper.ToModel(accountBalanceService.GetPreviousBalanceForTransaction(transactionAccount)),
            NewAccountBalance = accountBalanceMapper.ToModel(accountBalanceService.GetNewBalanceForTransaction(transactionAccount)),
        };
    }

    /// <summary>
    /// Maps the provided Transaction Account Type to a Transaction Account Type Model
    /// </summary>
    private static TransactionAccountTypeModel ToModel(TransactionAccountType transactionAccountType) => transactionAccountType switch
    {
        TransactionAccountType.Debit => TransactionAccountTypeModel.Debit,
        TransactionAccountType.Credit => TransactionAccountTypeModel.Credit,
        _ => throw new InvalidOperationException($"Unrecognized Transaction Account Type: {transactionAccountType}")
    };
}