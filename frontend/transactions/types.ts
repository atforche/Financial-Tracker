import {
  TransactionAccountTypeModel,
  type components,
} from "@/framework/data/api";

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

/**
 * Type representing the request to update a transaction.
 */
type UpdateTransactionRequest = components["schemas"]["UpdateTransactionModel"];

/**
 * Type representing the request to post a transaction to an account.
 */
type PostTransactionRequest = components["schemas"]["PostTransactionModel"];

export {
  type Transaction,
  type TransactionAccount,
  type CreateTransactionRequest,
  type UpdateTransactionRequest,
  type PostTransactionRequest,
  TransactionAccountTypeModel as TransactionAccountType,
};
