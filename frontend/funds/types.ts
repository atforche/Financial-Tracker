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
 * Type representing the current Funds response.
 */
type CurrentFunds = components["schemas"]["CurrentFundsModel"];

/**
 * Type representing a Fund card on the current Funds page.
 */
type CurrentFund = components["schemas"]["CurrentFundModel"];

/**
 * Type representing a recent balance event on the current Funds page.
 */
type CurrentFundBalanceEvent =
  components["schemas"]["CurrentFundBalanceEventModel"];

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

/**
 * Determines whether any fund assignments are incomplete.
 */
const hasIncompleteFundAssignments = function (
  fundAssignments: FundAmount[],
): boolean {
  return fundAssignments.some(
    (fundAmount) =>
      fundAmount.fundId === "" ||
      fundAmount.fundName === "" ||
      fundAmount.amount < 0 ||
      (fundAmount.amount === 0 && fundAmount.fundName !== "Unassigned"),
  );
};

export {
  type Fund,
  type FundSummary,
  type CurrentFunds,
  type CurrentFund,
  type CurrentFundBalanceEvent,
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
  hasIncompleteFundAssignments,
};
