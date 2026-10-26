import {
  TransactionSortModel,
  TransactionTypeModel,
  type components,
} from "@/framework/data/api";

/**
 * Type representing a transaction.
 */
type Transaction = components["schemas"]["TransactionModel"];

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
 * Type representing a fund transaction.
 */
type FundTransaction =
  components["schemas"]["TransactionModelFundTransactionModel"];

/**
 * Type representing an income transaction.
 */
type IncomeTransaction =
  components["schemas"]["TransactionModelIncomeTransactionModel"];

/**
 * Type representing a line item of an income transaction.
 */
type IncomeLine = IncomeTransaction["source"]["incomeLines"][number];

/**
 * Type representing a deduction of an income transaction.
 */
type IncomeDeduction = IncomeTransaction["source"]["incomeDeductions"][number];

/**
 * Type representing a destination of an income transaction.
 */
type IncomeTransactionDestination =
  components["schemas"]["TransactionModelIncomeTransactionModel"]["destinations"][number];

/**
 * Type representing a spending transaction.
 */
type SpendingTransaction =
  components["schemas"]["TransactionModelSpendingTransactionModel"];

/**
 * Type representing a spending transaction destination.
 */
type SpendingTransactionDestination =
  components["schemas"]["TransactionModelSpendingTransactionModel"]["destinations"][number];

/**
 * Type representing a refund transaction.
 */
type RefundTransaction =
  components["schemas"]["TransactionModelRefundTransactionModel"];

/**
 * Type representing a source of a refund transaction.
 */
type RefundTransactionSource = RefundTransaction["sources"][number];

/**
 * Type representing total, tracked, and untracked income amounts.
 */
type IncomeAmount = components["schemas"]["IncomeAmountModel"];

/**
 * Determines whether a transaction is an account transaction.
 */
const isAccountTransaction = function (
  transaction: Transaction,
): transaction is AccountTransaction {
  return transaction.transactionType === TransactionTypeModel.Account;
};

/**
 * Determines whether a transaction is a fund transaction.
 */
const isFundTransaction = function (
  transaction: Transaction,
): transaction is FundTransaction {
  return transaction.transactionType === TransactionTypeModel.Fund;
};

/**
 * Determines whether a transaction is an income transaction.
 */
const isIncomeTransaction = function (
  transaction: Transaction,
): transaction is IncomeTransaction {
  return transaction.transactionType === TransactionTypeModel.Income;
};

/**
 * Determines whether a transaction is a spending transaction.
 */
const isSpendingTransaction = function (
  transaction: Transaction,
): transaction is SpendingTransaction {
  return transaction.transactionType === TransactionTypeModel.Spending;
};

/**
 * Determines whether a transaction is a refund transaction.
 */
const isRefundTransaction = function (
  transaction: Transaction,
): transaction is RefundTransaction {
  return transaction.transactionType === TransactionTypeModel.Refund;
};

/**
 * Narrows an account transaction, returning null for other types.
 */
const asAccountTransaction = function (
  transaction: Transaction,
): AccountTransaction | null {
  return isAccountTransaction(transaction) ? transaction : null;
};

/**
 * Narrows a fund transaction, returning null for other types.
 */
const asFundTransaction = function (
  transaction: Transaction,
): FundTransaction | null {
  return isFundTransaction(transaction) ? transaction : null;
};

/**
 * Narrows an income transaction, returning null for other types.
 */
const asIncomeTransaction = function (
  transaction: Transaction,
): IncomeTransaction | null {
  return isIncomeTransaction(transaction) ? transaction : null;
};

/**
 * Narrows a spending transaction, returning null for other types.
 */
const asSpendingTransaction = function (
  transaction: Transaction,
): SpendingTransaction | null {
  return isSpendingTransaction(transaction) ? transaction : null;
};

/**
 * Narrows a refund transaction, returning null for other types.
 */
const asRefundTransaction = function (
  transaction: Transaction,
): RefundTransaction | null {
  return isRefundTransaction(transaction) ? transaction : null;
};

/**
 * Type representing a request to create a transaction.
 */
type CreateTransactionRequest = components["schemas"]["CreateTransactionModel"];

/**
 * Type representing a request to update a transaction.
 */
type UpdateTransactionRequest =
  | components["schemas"]["UpdateTransactionModelUpdateAccountTransactionModel"]
  | components["schemas"]["UpdateTransactionModelUpdateFundTransactionModel"]
  | components["schemas"]["UpdateTransactionModelUpdateIncomeTransactionModel"]
  | components["schemas"]["UpdateTransactionModelUpdateSpendingTransactionModel"]
  | components["schemas"]["UpdateTransactionModelUpdateRefundTransactionModel"];

/**
 * Type representing a request to post a transaction.
 */
type PostTransactionRequest = components["schemas"]["PostTransactionModel"];

export {
  type Transaction,
  TransactionSortModel as TransactionSort,
  TransactionTypeModel as TransactionType,
  type AccountTransaction,
  type AccountTransactionDestination,
  type FundTransaction,
  type IncomeTransaction,
  type IncomeLine,
  type IncomeDeduction,
  type IncomeTransactionDestination,
  type SpendingTransaction,
  type SpendingTransactionDestination,
  type RefundTransaction,
  type RefundTransactionSource,
  type IncomeAmount,
  type CreateTransactionRequest,
  type UpdateTransactionRequest,
  type PostTransactionRequest,
  isAccountTransaction,
  isFundTransaction,
  isIncomeTransaction,
  isSpendingTransaction,
  isRefundTransaction,
  asAccountTransaction,
  asFundTransaction,
  asIncomeTransaction,
  asSpendingTransaction,
  asRefundTransaction,
};
