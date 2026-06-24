import { type Transaction, TransactionType } from "@/transactions/transaction";
import type { components } from "@/framework/data/api";

/**
 * Type representing an account transaction.
 */
type AccountTransaction =
  components["schemas"]["TransactionModelAccountTransactionModel"];

/**
 * Type representing an account transaction destination.
 */
type AccountTransactionDestination =
  components["schemas"]["AccountTransactionDestinationModel"];

/**
 * Determines if the provided transaction is an account transaction.
 */
const isAccountTransaction = function (
  transaction: Transaction,
): transaction is AccountTransaction {
  return transaction.transactionType === TransactionType.Account;
};

/**
 * Converts the provided transaction to an account transaction.
 */
const asAccountTransaction = function (
  transaction: Transaction,
): AccountTransaction | null {
  return isAccountTransaction(transaction) ? transaction : null;
};

export type { AccountTransaction, AccountTransactionDestination };
export { isAccountTransaction, asAccountTransaction };
