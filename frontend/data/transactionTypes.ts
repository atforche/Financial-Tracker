import {
  AccountingPeriodTransactionSortOrderModel,
  type components,
} from "@/data/api";

/**
 * Type representing a Transaction.
 */
type Transaction = components["schemas"]["TransactionModel"];

export {
  type Transaction,
  AccountingPeriodTransactionSortOrderModel as AccountingPeriodTransactionSortOrder,
};
