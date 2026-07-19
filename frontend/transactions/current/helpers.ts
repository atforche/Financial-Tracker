import {
  type Transaction,
  asAccountTransaction,
  asFundTransaction,
  asIncomeTransaction,
  asSpendingTransaction,
} from "@/transactions/types";

/**
 * Summarizes the provided values.
 */
const summarizeValues = function (values: string[]): string {
  const meaningfulValues = values.filter((value) => value.trim() !== "");
  if (meaningfulValues.length === 0) {
    return "";
  }
  if (meaningfulValues.length === 1) {
    return meaningfulValues[0] ?? "";
  }
  return `${meaningfulValues[0] ?? ""} +${meaningfulValues.length - 1} more`;
};

/**
 * Gets the source label for the provided transaction.
 */
const getTransactionSourceLabel = function (transaction: Transaction): string {
  const spendingTransaction = asSpendingTransaction(transaction);
  if (spendingTransaction !== null) {
    return spendingTransaction.source.account.account.name;
  }

  const incomeTransaction = asIncomeTransaction(transaction);
  if (incomeTransaction !== null) {
    return (
      incomeTransaction.source.account?.account.name ??
      incomeTransaction.source.location ??
      ""
    );
  }

  const accountTransaction = asAccountTransaction(transaction);
  if (accountTransaction !== null) {
    return (
      accountTransaction.source.account?.account.name ??
      accountTransaction.source.location ??
      ""
    );
  }

  const fundTransaction = asFundTransaction(transaction);
  if (fundTransaction !== null) {
    return fundTransaction.source.fund.fund.name;
  }

  return "";
};

/**
 * Gets the destination label for the provided transaction.
 */
const getTransactionDestinationLabel = function (
  transaction: Transaction,
): string {
  const spendingTransaction = asSpendingTransaction(transaction);
  if (spendingTransaction !== null) {
    return summarizeValues(
      spendingTransaction.destinations.map(
        (destination) =>
          destination.account?.account.name ?? destination.location ?? "",
      ),
    );
  }

  const incomeTransaction = asIncomeTransaction(transaction);
  if (incomeTransaction !== null) {
    return summarizeValues(
      incomeTransaction.destinations.map(
        (destination) => destination.account.account.name,
      ),
    );
  }

  const accountTransaction = asAccountTransaction(transaction);
  if (accountTransaction !== null) {
    return summarizeValues(
      accountTransaction.destinations.map(
        (destination) =>
          destination.account?.account.name ?? destination.location ?? "",
      ),
    );
  }

  const fundTransaction = asFundTransaction(transaction);
  if (fundTransaction !== null) {
    return summarizeValues(
      fundTransaction.destinations.map(
        (destination) => destination.fund.fund.name,
      ),
    );
  }

  return "";
};

export { getTransactionSourceLabel, getTransactionDestinationLabel };
