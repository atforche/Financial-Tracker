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
type TransactionSummary = components["schemas"]["TransactionSummaryModel"];

/**
 * Type representing a summary of transactions grouped by type.
 */
type TransactionSummaryByType =
  components["schemas"]["TransactionSummaryByTypeModel"];

/**
 * Type representing a summary of transactions grouped by date.
 */
type TransactionSummaryByDate =
  components["schemas"]["TransactionSummaryByDateModel"];

/**
 * Type representing a summary of transactions grouped by accounting period.
 */
type TransactionSummaryByAccountingPeriod =
  components["schemas"]["TransactionSummaryByAccountingPeriodModel"];

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
  type TransactionSummary,
  type TransactionSummaryByType,
  type TransactionSummaryByDate,
  type TransactionSummaryByAccountingPeriod,
  type CreateTransactionRequest,
  type UpdateTransactionRequest,
  type PostTransactionRequest,
};
