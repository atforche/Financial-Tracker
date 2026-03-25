import {
  AccountingPeriodTransactionSortOrderModel,
  TransactionAccountTypeModel,
  type components,
} from "@/data/api";

/**
 * Type representing a Transaction.
 */
type Transaction = components["schemas"]["TransactionModel"];

/**
 * Type representing a Transaction Account.
 */
type TransactionAccount = components["schemas"]["TransactionAccountModel"];

/**
 * Type representing the request to create a transaction.
 */
type CreateTransactionRequest = components["schemas"]["CreateTransactionModel"];

export {
  type Transaction,
  type TransactionAccount,
  type CreateTransactionRequest,
  AccountingPeriodTransactionSortOrderModel as AccountingPeriodTransactionSortOrder,
  TransactionAccountTypeModel as TransactionAccountType,
};
