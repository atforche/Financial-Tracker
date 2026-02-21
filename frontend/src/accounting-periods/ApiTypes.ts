import {
  AccountingPeriodSortOrderModel,
  AccountingPeriodTransactionSortOrderModel,
  type components,
} from "@data/api";

/**
 * Type representing an Accounting Period.
 */
type AccountingPeriod = components["schemas"]["AccountingPeriodModel"];

/**
 * Interface representing an identifier for an Accounting Period.
 */
interface AccountingPeriodIdentifier {
  id: string;
  name: string;
}

/**
 * Type representing a request to create an Accounting Period.
 */
type CreateAccountingPeriodRequest =
  components["schemas"]["CreateAccountingPeriodModel"];

export {
  type AccountingPeriod,
  type CreateAccountingPeriodRequest,
  type AccountingPeriodIdentifier,
  AccountingPeriodSortOrderModel as AccountingPeriodSortOrder,
  AccountingPeriodTransactionSortOrderModel as AccountingPeriodTransactionSortOrder,
};
