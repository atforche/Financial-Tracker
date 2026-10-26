using Models.Accounts;

namespace Tests.Support;

/// <summary>
/// Builds an account, onboarded before accounting begins or created in an accounting period.
/// </summary>
internal sealed class AccountBuilder(TestApiClient apiClient, string name)
{
    private decimal _openingBalance;
    private string? _financialInstitution;
    private AccountTypeModel _type = AccountTypeModel.Standard;
    private AccountingPeriodHandle? _period;
    private DateOnly? _dateOpened;

    /// <summary>Sets the account type.</summary>
    public AccountBuilder WithType(AccountTypeModel value)
    {
        _type = value;
        return this;
    }

    /// <summary>
    /// Sets the account's financial institution.
    /// </summary>
    public AccountBuilder WithFinancialInstitution(string value)
    {
        _financialInstitution = value;
        return this;
    }

    /// <summary>
    /// Sets the account's onboarded balance.
    /// </summary>
    public AccountBuilder WithOpeningBalance(decimal value)
    {
        _openingBalance = value;
        return this;
    }

    /// <summary>
    /// Creates the account in an accounting period instead of onboarding it.
    /// </summary>
    public AccountBuilder In(AccountingPeriodHandle period)
    {
        _period = period;
        return this;
    }

    /// <summary>
    /// Sets the date a created account opened, which defaults to the first day of its period.
    /// </summary>
    public AccountBuilder OpenedOn(DateOnly date)
    {
        _dateOpened = date;
        return this;
    }

    /// <summary>
    /// Creates the account.
    /// </summary>
    public async Task<AccountHandle> CreateAsync()
    {
        AccountModel model = _period is null
            ? await apiClient.PostAsync<OnboardAccountModel, AccountModel>("/accounts/onboard", new OnboardAccountModel
            {
                Name = name,
                FinancialInstitution = _financialInstitution,
                Type = _type,
                OnboardedBalance = _openingBalance
            })
            : await apiClient.PostAsync<CreateAccountModel, AccountModel>("/accounts", new CreateAccountModel
            {
                Name = name,
                FinancialInstitution = _financialInstitution,
                Type = _type,
                OpeningAccountingPeriodId = _period.Id,
                DateOpened = _dateOpened ?? new DateOnly(_period.Year, _period.Month, 1)
            });
        return new AccountHandle(model.Id, model.Name);
    }
}
