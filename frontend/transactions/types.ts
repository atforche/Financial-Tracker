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
 * Type representing a summary of transactions.
 */
type TransactionSummaryByType =
  components["schemas"]["TransactionSummaryByTypeModel"];

/**
 * Type representing total, tracked, and untracked income amounts.
 */
type IncomeAmount = components["schemas"]["IncomeAmountModel"];

/**
 * Determines whether a transaction is an account transaction.
 */
const isAccountTransaction = (
  transaction: Transaction,
): transaction is AccountTransaction =>
  transaction.transactionType === TransactionTypeModel.Account;

/**
 * Determines whether a transaction is a fund transaction.
 */
const isFundTransaction = (
  transaction: Transaction,
): transaction is FundTransaction =>
  transaction.transactionType === TransactionTypeModel.Fund;

/**
 * Determines whether a transaction is an income transaction.
 */
const isIncomeTransaction = (
  transaction: Transaction,
): transaction is IncomeTransaction =>
  transaction.transactionType === TransactionTypeModel.Income;

/**
 * Determines whether a transaction is a spending transaction.
 */
const isSpendingTransaction = (
  transaction: Transaction,
): transaction is SpendingTransaction =>
  transaction.transactionType === TransactionTypeModel.Spending;

/**
 * Narrows an account transaction, returning null for other types.
 */
const asAccountTransaction = (
  transaction: Transaction,
): AccountTransaction | null =>
  isAccountTransaction(transaction) ? transaction : null;

/**
 * Narrows a fund transaction, returning null for other types.
 */
const asFundTransaction = (transaction: Transaction): FundTransaction | null =>
  isFundTransaction(transaction) ? transaction : null;

/**
 * Narrows an income transaction, returning null for other types.
 */
const asIncomeTransaction = (
  transaction: Transaction,
): IncomeTransaction | null =>
  isIncomeTransaction(transaction) ? transaction : null;

/**
 * Narrows a spending transaction, returning null for other types.
 */
const asSpendingTransaction = (
  transaction: Transaction,
): SpendingTransaction | null =>
  isSpendingTransaction(transaction) ? transaction : null;

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
  | components["schemas"]["UpdateTransactionModelUpdateSpendingTransactionModel"];

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
  type TransactionSummaryByType,
  type IncomeAmount,
  type CreateTransactionRequest,
  type UpdateTransactionRequest,
  type PostTransactionRequest,
  isAccountTransaction,
  isFundTransaction,
  isIncomeTransaction,
  isSpendingTransaction,
  asAccountTransaction,
  asFundTransaction,
  asIncomeTransaction,
  asSpendingTransaction,
};
