using Domain.AccountingPeriods;

namespace Domain.FundConversions;

/// <summary>
/// Interface representing methods to interact with a collection of <see cref="FundConversions"/>
/// </summary>
public interface IFundConversionRepository
{
    /// <summary>
    /// Determines if an Fund Conversion with the provided ID exists
    /// </summary>
    /// <param name="id">ID of the Fund Conversion</param>
    /// <returns>True if a Fund Conversion with the provided ID exists, false otherwise</returns>
    bool DoesFundConversionWithIdExist(Guid id);

    /// <summary>
    /// Finds the Fund Conversion with the specified ID.
    /// </summary>
    /// <param name="id">ID of the Fund Conversion to find</param>
    /// <returns>The Fund Conversion that was found</returns>
    FundConversion FindById(FundConversionId id);

    /// <summary>
    /// Finds all the Fund Conversions with the specified Accounting Period ID
    /// </summary>
    /// <param name="accountingPeriodId">Accounting Period ID</param>
    /// <returns>All the Fund Conversions with the specified Accounting Period ID</returns>
    IReadOnlyCollection<FundConversion> FindAllByAccountingPeriod(AccountingPeriodId accountingPeriodId);

    /// <summary>
    /// Finds all the Fund Conversions that were added in the specified Date Range
    /// </summary>
    /// <param name="dateRange">Date Range</param>
    /// <returns>All the Fund Conversions that were added or that had balance events in the specified Date Range</returns>
    IReadOnlyCollection<FundConversion> FindAllByDateRange(DateRange dateRange);

    /// <summary>
    /// Adds the provided Fund Conversion to the repository
    /// </summary>
    /// <param name="fundConversion">Fund Conversion that should be added</param>
    void Add(FundConversion fundConversion);
}