import {
  AccountingPeriodTransactionSortOrderModel,
  type components,
} from "@/data/api";

/**
 * Type representing a Transaction.
 */
type Transaction = components["schemas"]["TransactionModel"];

/**
 * Type representing the request to create a transaction.
 */
type CreateTransactionRequest = components["schemas"]["CreateTransactionModel"];

export {
  type Transaction,
  type CreateTransactionRequest,
  AccountingPeriodTransactionSortOrderModel as AccountingPeriodTransactionSortOrder,
};
