using System.Globalization;
using Domain.Funds;
using Models;
using Models.Funds;

namespace Rest.Funds;

/// <summary>
/// Class that handles retrieving Funds based on specified criteria
/// </summary>
public class FundGetter(IFundRepository fundRepository, FundConverter fundConverter)
{
    /// <summary>
    /// Gets the Funds that match the specified criteria
    /// </summary>
    public CollectionModel<FundModel> Get(FundQueryParameterModel request)
    {
        var results = fundRepository.GetAll().Select(fundConverter.ToModel).ToList();

        if (request.Search != null)
        {
            results = results.Where(fund =>
                fund.Name.Contains(request.Search, StringComparison.OrdinalIgnoreCase) ||
                fund.Description.Contains(request.Search, StringComparison.OrdinalIgnoreCase) ||
                fund.CurrentBalance.PostedBalance.ToString(CultureInfo.InvariantCulture).Contains(request.Search, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
        if (request.Sort is null or FundSortOrderModel.Name)
        {
            results = results.OrderBy(fund => fund.Name).ToList();
        }
        else if (request.Sort == FundSortOrderModel.NameDescending)
        {
            results = results.OrderByDescending(fund => fund.Name).ToList();
        }
        else if (request.Sort == FundSortOrderModel.Description)
        {
            results = results.OrderBy(fund => fund.Description).ThenBy(fund => fund.Name).ToList();
        }
        else if (request.Sort == FundSortOrderModel.DescriptionDescending)
        {
            results = results.OrderByDescending(fund => fund.Description).ThenBy(fund => fund.Name).ToList();
        }
        else if (request.Sort == FundSortOrderModel.Balance)
        {
            results = results.OrderBy(fund => fund.CurrentBalance.PostedBalance).ThenBy(fund => fund.Name).ToList();
        }
        else if (request.Sort == FundSortOrderModel.BalanceDescending)
        {
            results = results.OrderByDescending(fund => fund.CurrentBalance.PostedBalance).ThenBy(fund => fund.Name).ToList();
        }
        return new CollectionModel<FundModel>
        {
            Items = results.Skip(request.Offset ?? 0).Take(request.Limit ?? int.MaxValue).ToList(),
            TotalCount = results.Count,
        };
    }
}