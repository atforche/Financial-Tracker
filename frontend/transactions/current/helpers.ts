import type { Transaction } from "@/transactions/transaction";
import { asAccountTransaction } from "@/transactions/accountTransaction";
import { asFundTransaction } from "@/transactions/fundTransaction";
import { asIncomeTransaction } from "@/transactions/incomeTransaction";
import { asSpendingTransaction } from "@/transactions/spendingTransaction";

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
    return spendingTransaction.source.account.accountName;
  }

  const incomeTransaction = asIncomeTransaction(transaction);
  if (incomeTransaction !== null) {
    return (
      incomeTransaction.source.account?.accountName ??
      incomeTransaction.source.location ??
      ""
    );
  }

  const accountTransaction = asAccountTransaction(transaction);
  if (accountTransaction !== null) {
    return (
      accountTransaction.source.account?.accountName ??
      accountTransaction.source.location ??
      ""
    );
  }

  const fundTransaction = asFundTransaction(transaction);
  if (fundTransaction !== null) {
    return fundTransaction.source.fund.fundName;
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
          destination.account?.accountName ?? destination.location ?? "",
      ),
    );
  }

  const incomeTransaction = asIncomeTransaction(transaction);
  if (incomeTransaction !== null) {
    return summarizeValues(
      incomeTransaction.destinations.map(
        (destination) => destination.account.accountName,
      ),
    );
  }

  const accountTransaction = asAccountTransaction(transaction);
  if (accountTransaction !== null) {
    return summarizeValues(
      accountTransaction.destinations.map(
        (destination) =>
          destination.account?.accountName ?? destination.location ?? "",
      ),
    );
  }

  const fundTransaction = asFundTransaction(transaction);
  if (fundTransaction !== null) {
    return summarizeValues(
      fundTransaction.destinations.map(
        (destination) => destination.fund.fundName,
      ),
    );
  }

  return "";
};

export { getTransactionSourceLabel, getTransactionDestinationLabel };
