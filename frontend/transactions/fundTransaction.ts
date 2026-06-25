import { type Transaction, TransactionType } from "@/transactions/transaction";
import type { components } from "@/framework/data/api";

/**
 * Type representing a fund transaction.
 */
type FundTransaction =
  components["schemas"]["TransactionModelFundTransactionModel"];

/**
 * Determines if the provided transaction is a fund transaction.
 */
const isFundTransaction = function (
  transaction: Transaction,
): transaction is FundTransaction {
  return transaction.transactionType === TransactionType.Fund;
};

/**
 * Converts the provided transaction to a fund transaction.
 */
const asFundTransaction = function (
  transaction: Transaction,
): FundTransaction | null {
  return isFundTransaction(transaction) ? transaction : null;
};

export type { FundTransaction };
export { isFundTransaction, asFundTransaction };
