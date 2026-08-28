import {
  type AccountTransaction,
  type FundTransaction,
  type IncomeTransaction,
  type RefundTransaction,
  type SpendingTransaction,
  type Transaction,
  TransactionType,
} from "@/transactions/types";

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

/**
 * Determines if the provided transaction is a refund transaction.
 */
const isRefundTransaction = function (
  transaction: Transaction,
): transaction is RefundTransaction {
  return transaction.transactionType === TransactionType.Refund;
};

/**
 * Converts the provided transaction to a refund transaction.
 */
const asRefundTransaction = function (
  transaction: Transaction,
): RefundTransaction | null {
  return isRefundTransaction(transaction) ? transaction : null;
};

export {
  asAccountTransaction,
  asFundTransaction,
  asIncomeTransaction,
  asSpendingTransaction,
  asRefundTransaction,
  isAccountTransaction,
  isFundTransaction,
  isIncomeTransaction,
  isSpendingTransaction,
  isRefundTransaction,
};
