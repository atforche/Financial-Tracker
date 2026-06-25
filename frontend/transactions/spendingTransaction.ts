import { type Transaction, TransactionType } from "@/transactions/transaction";
import type { components } from "@/framework/data/api";

/**
 * Type representing a spending transaction.
 */
type SpendingTransaction =
  components["schemas"]["TransactionModelSpendingTransactionModel"];

/**
 * Determines if the provided transaction is a spending transaction.
 */
const isSpendingTransaction = function (
  transaction: Transaction,
): transaction is SpendingTransaction {
  return transaction.transactionType === TransactionType.Spending;
};

/**
 * Converts the provided transaction to a spending transaction.
 */
const asSpendingTransaction = function (
  transaction: Transaction,
): SpendingTransaction | null {
  return isSpendingTransaction(transaction) ? transaction : null;
};

export type { SpendingTransaction };
export { isSpendingTransaction, asSpendingTransaction };
