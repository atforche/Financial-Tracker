import { type Transaction, TransactionType } from "@/transactions/transaction";
import type { components } from "@/framework/data/api";

/**
 * Type representing an income transaction.
 */
type IncomeTransaction =
  components["schemas"]["TransactionModelIncomeTransactionModel"];

/**
 * Type representing a destination of an income transaction.
 */
type IncomeTransactionDestination =
  components["schemas"]["TransactionModelIncomeTransactionModel"]["destinations"][number];

/**
 * Determines if the provided transaction is an income transaction.
 */
const isIncomeTransaction = function (
  transaction: Transaction,
): transaction is IncomeTransaction {
  return transaction.transactionType === TransactionType.Income;
};

/**
 * Converts the provided transaction to an income transaction.
 */
const asIncomeTransaction = function (
  transaction: Transaction,
): IncomeTransaction | null {
  return isIncomeTransaction(transaction) ? transaction : null;
};

export type { IncomeTransaction, IncomeTransactionDestination };
export { isIncomeTransaction, asIncomeTransaction };
