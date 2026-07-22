using Domain;
using Domain.Funds;
using Domain.Funds.Queries;
using Models;
using Models.Funds;

namespace Rest.Funds;

/// <summary>
/// Converter class that handles converting Funds to Fund Models
/// </summary>
public sealed class FundConverter
{
    /// <summary>
    /// Converts the provided Fund query model to a Domain query.
    /// </summary>
    public FundQuery ToDomain(FundQueryParameterModel model) => new(
        ToDomain(model.Filter),
        model.Sort switch
        {
            FundSortModel.Name => FundSort.Name,
            FundSortModel.NameDescending => FundSort.NameDescending,
            FundSortModel.Description => FundSort.Description,
            FundSortModel.DescriptionDescending => FundSort.DescriptionDescending,
            _ => FundSort.Name,
        },
        model.Offset ?? 0,
        model.Limit);

    /// <summary>
    /// Converts the provided Fund Balance query model to a Domain query.
    /// </summary>
    public FundBalanceQuery ToDomain(FundWithBalanceQueryParameterModel model) => new(
        ToDomain(model.Filter),
        model.Sort switch
        {
            FundWithBalanceSortModel.Name => FundBalanceSort.Name,
            FundWithBalanceSortModel.NameDescending => FundBalanceSort.NameDescending,
            FundWithBalanceSortModel.Description => FundBalanceSort.Description,
            FundWithBalanceSortModel.DescriptionDescending => FundBalanceSort.DescriptionDescending,
            FundWithBalanceSortModel.PostedBalance => FundBalanceSort.PostedBalance,
            FundWithBalanceSortModel.PostedBalanceDescending => FundBalanceSort.PostedBalanceDescending,
            _ => FundBalanceSort.Name,
        },
        model.Offset ?? 0,
        model.Limit);

    /// <summary>
    /// Converts the provided Fund to a Fund Model
    /// </summary>
    public FundModel ToModel(Fund fund) => new()
    {
        Id = fund.Id.Value,
        Name = fund.Name,
        Description = fund.Description,
    };

    /// <summary>
    /// Converts the provided Fund page to a Fund collection model.
    /// </summary>
    public CollectionModel<FundModel> ToModel(QueryPage<Fund> page) => new()
    {
        Items = page.Items.Select(ToModel).ToList(),
        TotalCount = page.TotalCount,
    };

    /// <summary>
    /// Converts a Fund and its balance to a Fund with Balance model.
    /// </summary>
    public FundWithBalanceModel ToModel(FundBalance balance) => new()
    {
        Id = balance.Fund.Id.Value,
        Name = balance.Fund.Name,
        Description = balance.Fund.Description,
        CurrentBalance = new FundBalanceModel
        {
            PostedBalance = balance.PostedBalance,
            PendingDebitAmount = balance.PendingDebitAmount,
            PendingCreditAmount = balance.PendingCreditAmount,
        },
    };

    /// <summary>
    /// Converts the provided Fund Balance page to a collection model.
    /// </summary>
    public CollectionModel<FundWithBalanceModel> ToModel(QueryPage<FundBalance> page) => new()
    {
        Items = page.Items.Select(ToModel).ToList(),
        TotalCount = page.TotalCount,
    };

    /// <summary>
    /// Converts the provided Fund Filter model to a Domain filter.
    /// </summary>
    private static FundFilter ToDomain(FundFilterModel? model) => new(
        model?.NameSearch,
        model?.Names ?? []);
}