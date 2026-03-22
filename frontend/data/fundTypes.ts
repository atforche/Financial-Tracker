import {
  AccountingPeriodFundSortOrderModel,
  type components,
} from "@/data/api";

/**
 * Type representing a Fund.
 */
type Fund = components["schemas"]["FundModel"];

/**
 * Type representing a Fund in the context of an Accounting Period.
 */
type AccountingPeriodFund = components["schemas"]["AccountingPeriodFundModel"];

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
 * Type representing a Fund Amount.
 */
type FundAmount = components["schemas"]["FundAmountModel"];

export {
  type Fund,
  type AccountingPeriodFund,
  type FundIdentifier,
  type CreateFundRequest,
  type FundAmount,
  AccountingPeriodFundSortOrderModel as AccountingPeriodFundSortOrder,
};
