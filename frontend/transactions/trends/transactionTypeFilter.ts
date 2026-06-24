import { TransactionType } from "@/transactions/transaction";

const transactionTypeValues = Object.values(
  TransactionType,
) as readonly TransactionType[];

const transactionTypeSet = new Set<string>(transactionTypeValues);

/**
 * Normalizes raw transaction-type values from the URL into a canonical ordered list.
 */
const normalizeTransactionTypes = function (
  values: readonly string[],
): readonly TransactionType[] {
  const seenTransactionTypes = new Set<string>();
  const normalizedTransactionTypes: TransactionType[] = [];

  values.forEach((value) => {
    const nextValue = value.trim();
    if (nextValue === "" || seenTransactionTypes.has(nextValue)) {
      return;
    }
    if (!transactionTypeSet.has(nextValue)) {
      return;
    }
    seenTransactionTypes.add(nextValue);
    // eslint-disable-next-line @typescript-eslint/no-unsafe-type-assertion
    normalizedTransactionTypes.push(nextValue as TransactionType);
  });
  if (normalizedTransactionTypes.length === 0) {
    return [];
  }
  return transactionTypeValues.filter((transactionType) =>
    normalizedTransactionTypes.includes(transactionType),
  );
};

/**
 * Determines whether selected transaction types should be written into the URL.
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
