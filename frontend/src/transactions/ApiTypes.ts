import type { components } from "@data/api";

/**
 * Type representing a Transaction.
 */
type Transaction = components["schemas"]["TransactionModel"];

/**
 * Type representing a Transaction Account.
 */
type TransactionAccount = components["schemas"]["TransactionAccountModel"];

/**
 * Type representing a request to create an Transaction.
 */
type CreateTransactionRequest = components["schemas"]["CreateTransactionModel"];

/**
 * Enum representing the different types of transaction accounts.
 */
enum TransactionAccountType {
  Debit = "Debit",
  Credit = "Credit",
}

export {
  type Transaction,
  type TransactionAccount,
  type CreateTransactionRequest,
  TransactionAccountType,
};
