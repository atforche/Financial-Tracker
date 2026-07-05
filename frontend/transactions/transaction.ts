import {
  TransactionAccountTypeModel,
  TransactionSortOrderModel,
  TransactionTrendsModeModel,
  TransactionTypeModel,
  type components,
} from "@/framework/data/api";
import type { AccountType } from "@/accounts/types";

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

/**
 * Interface representing a draft of a transaction account.
 */
interface TransactionAccountDraft {
  readonly accountId: string | null;
  readonly accountName: string | null;
  readonly accountType: AccountType | null;
  readonly postedDate: string | null;
  readonly previousAccountBalance: number | null;
  readonly newAccountBalance: number | null;
}

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
  type TransactionAccountDraft,
  TransactionTrendsModeModel as TransactionTrendsMode,
  TransactionSortOrderModel as TransactionSortOrder,
  TransactionAccountTypeModel as TransactionAccountType,
  TransactionTypeModel as TransactionType,
};
