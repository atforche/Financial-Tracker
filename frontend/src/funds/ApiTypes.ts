import {
  FundSortOrderModel,
  FundTransactionSortOrderModel,
  type components,
} from "@data/api";

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
 * Type representing a request to create or update a Fund.
 */
type CreateOrUpdateFundRequest =
  components["schemas"]["CreateOrUpdateFundModel"];

/**
 * Type representing a Fund Amount.
 */
type FundAmount = components["schemas"]["FundAmountModel"];

export {
  type Fund,
  type FundIdentifier,
  type CreateOrUpdateFundRequest,
  type FundAmount,
  FundSortOrderModel as FundSortOrder,
  FundTransactionSortOrderModel as FundTransactionSortOrder,
};
