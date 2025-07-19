using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Funds;
using Domain.Transactions;
using Tests.Mocks;

namespace Tests.Builders;

/// <summary>
/// Builder class that constructs a Transaction
/// </summary>
public sealed class TransactionBuilder(
    TransactionFactory transactionFactory,
    IAccountingPeriodRepository accountingPeriodRepository,
    ITransactionRepository transactionRepository,
    TestUnitOfWork testUnitOfWork)
{
    private AccountingPeriodId? _accountingPeriodId;
    private DateOnly _date = new(2025, 1, 15);
    private AccountId? _debitAccountId;
    private ICollection<FundAmount>? _debitFundAmounts;
    private AccountId? _creditAccountId;
    private ICollection<FundAmount>? _creditFundAmounts;

    /// <summary>
    /// Builds the specified Transaction
    /// </summary>
    /// <returns>The newly constructed Transaction</returns>
    public Transaction Build()
    {
        AccountingPeriodId accountingPeriodId = _accountingPeriodId ?? accountingPeriodRepository.FindAll()
            .Single(accountingPeriod => accountingPeriod.Year == _date.Year && accountingPeriod.Month == _date.Month).Id;
        Transaction transaction = transactionFactory.Create(accountingPeriodId,
            _date,
            _debitAccountId,
            _debitFundAmounts,
            _creditAccountId,
            _creditFundAmounts);
        transactionRepository.Add(transaction);
        testUnitOfWork.SaveChanges();
        return transaction;
    }

    /// <summary>
    /// Sets the Accounting Period for this Transaction Builder
    /// </summary>
    public TransactionBuilder WithAccountingPeriod(AccountingPeriodId accountingPeriodId)
    {
        _accountingPeriodId = accountingPeriodId;
        return this;
    }

    /// <summary>
    /// Sets the Date for this Transaction Builder
    /// </summary>
    public TransactionBuilder WithDate(DateOnly date)
    {
        _date = date;
        return this;
    }

    /// <summary>
    /// Sets the Debit Account for this Transaction Builder
    /// </summary>
    public TransactionBuilder WithDebitAccount(AccountId? debitAccountId)
    {
        _debitAccountId = debitAccountId;
        return this;
    }

    /// <summary>
    /// Sets the Debit Fund Amounts for this Transaction Builder
    /// </summary>
    public TransactionBuilder WithDebitFundAmounts(ICollection<FundAmount> debitFundAmounts)
    {
        _debitFundAmounts = debitFundAmounts;
        return this;
    }

    /// <summary>
    /// Sets the Credit Account for this Transaction Builder
    /// </summary>
    public TransactionBuilder WithCreditAccount(AccountId? creditAccountId)
    {
        _creditAccountId = creditAccountId;
        return this;
    }

    /// <summary>
    /// Sets the Credit Fund Amounts for this Transaction Builder
    /// </summary>
    public TransactionBuilder WithCreditFundAmounts(ICollection<FundAmount> creditFundAmounts)
    {
        _creditFundAmounts = creditFundAmounts;
        return this;
    }
}