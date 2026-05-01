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
 * Type representing a request to update a Fund.
 */
type UpdateFundRequest = components["schemas"]["UpdateFundModel"];

/**
 * Type representing a Fund Amount.
 */
type FundAmount = components["schemas"]["FundAmountModel"];

export {
  type Fund,
  type FundIdentifier,
  type CreateFundRequest,
  type UpdateFundRequest,
  type FundAmount,
  FundSortOrderModel as FundSortOrder,
  FundTransactionSortOrderModel as FundTransactionSortOrder,
};
