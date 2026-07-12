import {
  FundSortOrderModel,
  FundTrendsBalanceEventSortOrderModel,
  FundTrendsBalanceEventTypeModel,
  FundTrendsModeModel,
  FundTrendsSortOrderModel,
  type components,
} from "@/framework/data/api";

/**
 * Type representing a Fund.
 */
type Fund = components["schemas"]["FundModel"];

/**
 * Type representing Fund summary balances.
 */
type FundSummary = components["schemas"]["FundSummaryModel"];

/**
 * Type representing a balance event on the Fund workspace page.
 */
type FundWorkspaceBalanceEvent =
  components["schemas"]["FundWorkspaceBalanceEventModel"];

/**
 * Type representing the Funds trends response.
 */
type FundTrends = components["schemas"]["FundTrendsModel"];

/**
 * Type representing a row in the Funds trends fund table.
 */
type FundTrendsFund = components["schemas"]["FundTrendsFundModel"];

/**
 * Type representing a row in the Funds trends balance event table.
 */
type FundTrendsBalanceEvent =
  components["schemas"]["FundTrendsBalanceEventModel"];

/**
 * Type representing a period summary in the Funds trends response.
 */
type FundTrendsPeriodSummary =
  components["schemas"]["FundTrendsPeriodSummaryModel"];

/**
 * Type representing a date summary in the Funds trends response.
 */
type FundTrendsDateSummary =
  components["schemas"]["FundTrendsDateSummaryModel"];

/**
 * Interface representing a Fund identifier with its ID and name.
 */
interface FundIdentifier {
  readonly id: string;
  readonly name: string;
}

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
  type FundSummary,
  type FundWorkspaceBalanceEvent,
  type FundTrends,
  type FundTrendsFund,
  type FundTrendsBalanceEvent,
  type FundTrendsDateSummary,
  type FundTrendsPeriodSummary,
  type FundIdentifier,
  type CreateFundRequest,
  type OnboardFundRequest,
  type UpdateFundRequest,
  type FundAmount,
  FundTrendsBalanceEventSortOrderModel as FundTrendsBalanceEventSortOrder,
  FundTrendsBalanceEventTypeModel as FundTrendsBalanceEventType,
  FundTrendsModeModel as FundTrendsMode,
  FundTrendsSortOrderModel as FundTrendsSortOrder,
  FundSortOrderModel as FundSortOrder,
};
