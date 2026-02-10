using Domain.Accounts;
using Domain.Funds;

namespace Domain.Transactions;

/// <summary>
/// Value object class representing an Account involved in a Transaction
/// </summary>
public class TransactionAccount
{
    private readonly List<FundAmount> _fundAmounts = [];

    /// <summary>
    /// Parent Transaction for this Transaction Account
    /// </summary>
    public Transaction Transaction { get; private set; }

    /// <summary>
    /// Account id for this Transaction Account
    /// </summary>
    public AccountId AccountId { get; private set; }

    /// <summary>
    /// Fund Amounts for this Transaction Account
    /// </summary>
    public IReadOnlyCollection<FundAmount> FundAmounts => _fundAmounts;

    /// <summary>
    /// Date that the Transaction was posted to this Account
    /// </summary>
    public DateOnly? PostedDate { get; internal set; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal TransactionAccount(Transaction transaction, AccountId accountId, IEnumerable<FundAmount> fundAmounts)
    {
        Transaction = transaction;
        AccountId = accountId;
        _fundAmounts = fundAmounts.ToList();
    }

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    private TransactionAccount()
    {
        Transaction = null!;
        AccountId = null!;
    }
}