import type { components } from "@data/api";

/**
 * Type representing a Transaction.
 */
type Transaction = components["schemas"]["TransactionModel"];

/**
 * Type representing a request to create an Transaction.
 */
type CreateTransactionRequest = components["schemas"]["CreateTransactionModel"];

export { type Transaction, type CreateTransactionRequest };
