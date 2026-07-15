import {
  AccountingPeriodWithBalanceSortModel,
  type components,
} from "@/framework/data/api";

/**
 * Type representing an Accounting Period.
 */
type AccountingPeriod = components["schemas"]["AccountingPeriodModel"];

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
  type AccountingPeriodWithBalance,
  type AccountingPeriodWithTransactions,
  type AccountingPeriodsInRange,
  AccountingPeriodWithBalanceSortModel as AccountingPeriodWithBalanceSort,
  type CreateAccountingPeriodRequest,
};
