import {
  AccountingPeriodFundGoalSortOrderModel,
  AccountingPeriodFundSortOrderModel,
  FundGoalSortOrderModel,
  FundGoalTypeModel,
  FundSortOrderModel,
  FundTransactionSortOrderModel,
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
 * Type representing a Fund Goal.
 */
type FundGoal = components["schemas"]["FundGoalModel"];

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
 * Type representing a request to create a Fund Goal.
 */
type CreateFundGoalRequest = components["schemas"]["CreateFundGoalModel"];

/**
 * Type representing a request to update a Fund.
 */
type UpdateFundRequest = components["schemas"]["UpdateFundModel"];

/**
 * Type representing a request to update a Fund Goal.
 */
type UpdateFundGoalRequest = components["schemas"]["UpdateFundGoalModel"];

/**
 * Type representing a Fund Amount.
 */
type FundAmount = components["schemas"]["FundAmountModel"];

export {
  type Fund,
  type AccountingPeriodFund,
  type FundGoal,
  type FundIdentifier,
  type CreateFundRequest,
  type CreateFundGoalRequest,
  type UpdateFundRequest,
  type UpdateFundGoalRequest,
  type FundAmount,
  AccountingPeriodFundSortOrderModel as AccountingPeriodFundSortOrder,
  AccountingPeriodFundGoalSortOrderModel as AccountingPeriodFundGoalSortOrder,
  FundGoalSortOrderModel as FundGoalSortOrder,
  FundGoalTypeModel as FundGoalType,
  FundSortOrderModel as FundSortOrder,
  FundTransactionSortOrderModel as FundTransactionSortOrder,
};
