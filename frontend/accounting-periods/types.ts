import {
  AccountingPeriodSortModel,
  AccountingPeriodWithBalanceSortModel,
  type components,
} from "@/framework/data/api";

/**
 * Type representing an Accounting Period.
 */
type AccountingPeriod = components["schemas"]["AccountingPeriodModel"];

/**
 * Type representing a range of accounting periods.
 */
interface AccountingPeriodRange {
  readonly start: string;
  readonly end: string;
}

/**
 * Type representing an Accounting Period along with its balance.
 */
type AccountingPeriodWithBalance =
  components["schemas"]["AccountingPeriodWithBalanceModel"];

/**
 * Type representing an Accounting Period with its associated Transactions.
 */
type AccountingPeriodWithTransactions =
  components["schemas"]["AccountingPeriodWithTransactionsModel"];

/**
 * Type representing a range of Accounting Periods.
 */
type AccountingPeriodsInRange =
  components["schemas"]["AccountingPeriodsInRangeModel"];

/**
 * Type representing a request to create an Accounting Period.
 */
type CreateAccountingPeriodRequest =
  components["schemas"]["CreateAccountingPeriodModel"];

export {
  type AccountingPeriod,
  type AccountingPeriodRange,
  AccountingPeriodSortModel as AccountingPeriodSort,
  type AccountingPeriodWithBalance,
  AccountingPeriodWithBalanceSortModel as AccountingPeriodWithBalanceSort,
  type AccountingPeriodWithTransactions,
  type AccountingPeriodsInRange,
  type CreateAccountingPeriodRequest,
};
