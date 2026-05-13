import {
  FundSortOrderModel,
  FundTransactionSortOrderModel,
  type components,
} from "@/framework/data/api";

/**
 * Type representing a Fund.
 */
type Fund = components["schemas"]["FundModel"];

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
  type FundIdentifier,
  type CreateFundRequest,
  type OnboardFundRequest,
  type UpdateFundRequest,
  type FundAmount,
  FundSortOrderModel as FundSortOrder,
  FundTransactionSortOrderModel as FundTransactionSortOrder,
  hasIncompleteFundAssignments,
};
