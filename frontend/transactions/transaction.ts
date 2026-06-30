import {
  TransactionAccountTypeModel,
  TransactionSortOrderModel,
  TransactionTrendsModeModel,
  TransactionTypeModel,
  type components,
} from "@/framework/data/api";

type Transaction = components["schemas"]["TransactionModel"];
type TransactionTrends = components["schemas"]["TransactionTrendsModel"];
type CurrentTransactions = components["schemas"]["CurrentTransactionsModel"];
type TransactionTrendsTransactionTypeSummary =
  components["schemas"]["TransactionTrendsTransactionTypeSummaryModel"];
type TransactionTrendsDateSummary =
  components["schemas"]["TransactionTrendsDateSummaryModel"];
type TransactionTrendsPeriodSummary =
  components["schemas"]["TransactionTrendsPeriodSummaryModel"];
type TransactionAccount = components["schemas"]["TransactionAccountModel"];
type TransactionFund = components["schemas"]["TransactionFundModel"];
type CreateTransactionRequest = components["schemas"]["CreateTransactionModel"];
type UpdateTransactionRequest = components["schemas"]["UpdateTransactionModel"];
type PostTransactionRequest = components["schemas"]["PostTransactionModel"];

export {
  type Transaction,
  type TransactionTrends,
  type CurrentTransactions,
  type TransactionTrendsDateSummary,
  type TransactionTrendsPeriodSummary,
  type TransactionTrendsTransactionTypeSummary,
  type TransactionAccount,
  type TransactionFund,
  type CreateTransactionRequest,
  type UpdateTransactionRequest,
  type PostTransactionRequest,
  TransactionTrendsModeModel as TransactionTrendsMode,
  TransactionSortOrderModel as TransactionSortOrder,
  TransactionAccountTypeModel as TransactionAccountType,
  TransactionTypeModel as TransactionType,
};
