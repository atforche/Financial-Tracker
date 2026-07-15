import {
  FundBalanceEventSortModel,
  FundWithBalanceRangeSortModel,
  type components,
} from "@/framework/data/api";

/**
 * Type representing a Fund.
 */
type Fund = components["schemas"]["FundModel"];

/**
 * Type representing a Fund along with its current balance.
 */
type FundWithBalance = components["schemas"]["FundWithBalanceModel"];

/**
 * Type representing a Fund with a balance range.
 */
type FundWithBalanceRange = components["schemas"]["FundWithBalanceRangeModel"];

/**
 * Type representing Fund summary balances.
 */
type FundBalanceSummary = components["schemas"]["FundBalanceSummaryModel"];

/**
 * Type representing a Fund summary balance for a specific date.
 */
type FundBalanceSummaryByDate =
  components["schemas"]["FundBalanceSummaryByDateModel"];

/**
 * Type representing a Fund summary balance for a specific accounting period.
 */
type FundBalanceSummaryByPeriod =
  components["schemas"]["FundBalanceSummaryByPeriodModel"];

/**
 * Type representing a balance event on the Fund workspace page.
 */
type FundBalanceEvent = components["schemas"]["FundBalanceEventModel"];

/**
 * Type representing a collection of Funds within a specified date range.
 */
type FundsInDateRange = components["schemas"]["FundsInDateRangeModel"];

/**
 * Type representing a collection of Funds within a specified accounting period range.
 */
type FundsInAccountingPeriodRange =
  components["schemas"]["FundsInAccountingPeriodRangeModel"];

/**
 * Type representing a request to create a Fund.
 */
type CreateFundRequest = components["schemas"]["CreateFundModel"];

/**
 * Type representing a request to onboard a Fund.
 */
type OnboardFundRequest = components["schemas"]["OnboardFundModel"];

/**
 * Type representing a request to update a Fund.
 */
type UpdateFundRequest = components["schemas"]["UpdateFundModel"];

/**
 * Type representing a Fund Amount.
 */
type FundAmount = components["schemas"]["FundAmountModel"];

export {
  type Fund,
  type FundWithBalance,
  type FundWithBalanceRange,
  FundWithBalanceRangeSortModel as FundWithBalanceRangeSort,
  type FundBalanceSummary,
  type FundBalanceSummaryByDate,
  type FundBalanceSummaryByPeriod,
  type FundBalanceEvent,
  FundBalanceEventSortModel as FundBalanceEventSort,
  type FundsInDateRange,
  type FundsInAccountingPeriodRange,
  type CreateFundRequest,
  type OnboardFundRequest,
  type UpdateFundRequest,
  type FundAmount,
};
