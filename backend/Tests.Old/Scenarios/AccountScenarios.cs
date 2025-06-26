using System.Collections;
using Domain.Accounts;

namespace Tests.Old.Scenarios;

/// <summary>
/// Collection class that contains all the unique Account scenarios
/// </summary>
public sealed class AccountScenarios : IEnumerable<TheoryDataRow<AccountType>>
{
    /// <inheritdoc/>
    public IEnumerator<TheoryDataRow<AccountType>> GetEnumerator() => Enum.GetValues<AccountType>()
        .Select(accountType => new TheoryDataRow<AccountType>(accountType)).GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}