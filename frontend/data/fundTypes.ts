import {
  AccountingPeriodFundSortOrderModel,
  type components,
} from "@/data/api";

/**
 * Type representing a Fund.
 */
type Fund = components["schemas"]["FundModel"];

/**
 * Type representing a request to create a Fund.
 */
type CreateFundRequest = components["schemas"]["CreateFundModel"];

export {
  type Fund,
  type CreateFundRequest,
  AccountingPeriodFundSortOrderModel as AccountingPeriodFundSortOrder,
};
