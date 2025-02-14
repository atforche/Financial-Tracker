using Rest.Models.Account;
using Rest.Models.AccountingPeriod;
using Rest.Models.Fund;
using Utilities.BulkDataUpload.Models;

namespace Utilities.BulkDataUpload.Uploaders;

/// <summary>
/// Bulk data uploader that uploads an Accounting Period to the REST API
/// </summary>
internal sealed class AccountingPeriodUploader : DataUploader<AccountingPeriodUploadModel>
{
    private List<FundModel>? _funds;
    private List<AccountModel>? _accounts;
    private Dictionary<Guid, TransactionModel> _transactions = [];

    /// <inheritdoc/>
    public override async Task UploadAsync(AccountingPeriodUploadModel model)
    {
        // Ensure the existing entities that are needed are populated
        _funds ??= await GetAsync<List<FundModel>>("/funds");
        _accounts ??= await GetAsync<List<AccountModel>>("/accounts");

        // Create the accounting period
        Console.WriteLine($"Creating Accounting Period: {model.Year}-{model.Month}");
        AccountingPeriodModel accountingPeriod = await PostAsync<CreateAccountingPeriodModel, AccountingPeriodModel>(
            "/accountingPeriods",
            model.GetAsCreateAccountingPeriodModel());

        // Create any new accounts for this accounting period
        foreach (AccountUploadModel accountUploadModel in model.NewAccounts)
        {
            Console.WriteLine($"Creating Account: {accountUploadModel.Name}");
            _accounts.Add(await PostAsync<CreateAccountModel, AccountModel>(
                "/accounts",
                accountUploadModel.GetAsCreateAccountModel(_funds)));
        }
        // Create the balance events
        foreach (BalanceEventUploadModel balanceEventUploadModel in model.BalanceEvents)
        {
            if (balanceEventUploadModel is TransactionAddedUploadModel transactionAddedUploadModel)
            {
                Console.WriteLine($"Creating Transaction: {transactionAddedUploadModel.Id}");
                _transactions.Add(transactionAddedUploadModel.Id,
                    await PostAsync<CreateTransactionModel, TransactionModel>(
                        $"/accountingPeriods/{accountingPeriod.Id}/Transactions",
                        transactionAddedUploadModel.GetAsCreateTransactionModel(_funds, _accounts)));
            }
            else if (balanceEventUploadModel is TransactionPostedUploadModel transactionPostedUploadModel)
            {
                Console.WriteLine($"Posting Transaction '{transactionPostedUploadModel.TransactionId}' in Account '{transactionPostedUploadModel.AccountName}'");
                await PostAsync<PostTransactionModel, TransactionModel>(
                    $"/accountingPeriods/{accountingPeriod.Id}/Transactions/{transactionPostedUploadModel.GetTransactionIdToPost(_transactions)}",
                    transactionPostedUploadModel.GetAsPostTransactionModel(_accounts));
            }
            else if (balanceEventUploadModel is FundConversionUploadModel fundConversionUploadModel)
            {
                Console.WriteLine($"Creating Fund Conversion: {fundConversionUploadModel.Id}");
                await PostAsync<CreateFundConversionModel, FundConversionModel>(
                    $"/accountingPeriods/{accountingPeriod.Id}/FundConversions",
                    fundConversionUploadModel.GetAsCreateFundConversionModel(_funds, _accounts));
            }
            else if (balanceEventUploadModel is ChangeInValueUploadModel changeInValueUploadModel)
            {
                Console.WriteLine($"Creating Change In Value: {changeInValueUploadModel.Id}");
                await PostAsync<CreateChangeInValueModel, ChangeInValueModel>(
                    $"/accountingPeriods/{accountingPeriod.Id}/ChangeInValues",
                    changeInValueUploadModel.GetAsCreateChangeInValueModel(_funds, _accounts));
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
        // Close the accounting period if necessary
        if (model.IsClosed)
        {
            Console.WriteLine($"Closing Accounting Period: {model.Year}-{model.Month}");
            await PostAsync<object?, AccountingPeriodModel>($"/accountingPeriods/close/{accountingPeriod.Id}", null);
        }
    }
}