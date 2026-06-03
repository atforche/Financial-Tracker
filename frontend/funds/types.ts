import {
  FundDashboardBalanceEventSortOrderModel,
  FundDashboardBalanceEventTypeModel,
  FundDashboardModeModel,
  FundDashboardSortOrderModel,
  FundSortOrderModel,
  FundTransactionSortOrderModel,
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
 * Type representing the Funds dashboard response.
 */
type FundDashboard = components["schemas"]["FundDashboardModel"];

/**
 * Type representing a row in the Funds dashboard fund table.
 */
type FundDashboardFund = components["schemas"]["FundDashboardFundModel"];

/**
 * Type representing a row in the Funds dashboard balance event table.
 */
type FundDashboardBalanceEvent =
  components["schemas"]["FundDashboardBalanceEventModel"];

/**
 * Type representing a period summary in the Funds dashboard response.
 */
type FundDashboardPeriodSummary =
  components["schemas"]["FundDashboardPeriodSummaryModel"];

/**
 * Type representing a date summary in the Funds dashboard response.
 */
type FundDashboardDateSummary =
  components["schemas"]["FundDashboardDateSummaryModel"];

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
  type FundDashboard,
  type FundDashboardFund,
  type FundDashboardBalanceEvent,
  type FundDashboardDateSummary,
  type FundDashboardPeriodSummary,
  type FundSummary,
  type FundIdentifier,
  type CreateFundRequest,
  type OnboardFundRequest,
  type UpdateFundRequest,
  type FundAmount,
  FundDashboardBalanceEventSortOrderModel as FundDashboardBalanceEventSortOrder,
  FundDashboardBalanceEventTypeModel as FundDashboardBalanceEventType,
  FundDashboardModeModel as FundDashboardMode,
  FundDashboardSortOrderModel as FundDashboardSortOrder,
  FundSortOrderModel as FundSortOrder,
  FundTransactionSortOrderModel as FundTransactionSortOrder,
  hasIncompleteFundAssignments,
};
