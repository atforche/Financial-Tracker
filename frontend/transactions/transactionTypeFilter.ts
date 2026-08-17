import { TransactionType } from "@/transactions/types";
import enumValues from "@/framework/data/enumValues";

/**
 * Set of all valid transaction types, derived from the TransactionType enum.
 */
const transactionTypeValues = enumValues(TransactionType);

const transactionTypeSet = new Set<string>(transactionTypeValues);

/**
 * Normalizes raw transaction-type values from the URL into a canonical order.
 */
const normalizeTransactionTypes = function (
  values: readonly string[],
): readonly TransactionType[] {
  const requested = new Set(
    values
      .map((value) => value.trim())
      .filter((value) => transactionTypeSet.has(value)),
  );
  return transactionTypeValues.filter((type) => requested.has(type));
};

/**
 * Returns whether the selected values narrow the transaction set.
 */
const shouldPersistTransactionTypes = function (
  values: readonly TransactionType[],
): boolean {
  return values.length > 0 && values.length < transactionTypeValues.length;
};

export {
  transactionTypeValues,
  normalizeTransactionTypes,
  shouldPersistTransactionTypes,
};
