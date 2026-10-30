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
 * Type representing a range of Accounting Periods.
 */
type AccountingPeriodsInRange =
  components["schemas"]["AccountingPeriodsInRangeModel"];

/**
 * Type representing a request to create an Accounting Period.
 */
type CreateAccountingPeriodRequest =
  components["schemas"]["CreateAccountingPeriodModel"];

/**
 * Type representing a configured expected-income source.
 */
type ExpectedIncomeSource = components["schemas"]["ExpectedIncomeSourceModel"];

/**
 * Expected Income Sources keyed by the ID of the accounting period they belong to.
 */
type ExpectedIncomeSourcesByPeriod = Readonly<
  Record<string, readonly ExpectedIncomeSource[]>
>;

/**
 * Supported Expected Income Source sort orders on the planning page.
 */
enum ExpectedIncomeSourceSort {
  Source = "Source",
  SourceDescending = "SourceDescending",
  ExpectedTrackedIncome = "ExpectedTrackedIncome",
  ExpectedTrackedIncomeDescending = "ExpectedTrackedIncomeDescending",
  ExpectedUntrackedIncome = "ExpectedUntrackedIncome",
  ExpectedUntrackedIncomeDescending = "ExpectedUntrackedIncomeDescending",
}

/**
 * Type representing an expected-income source request.
 */
type ExpectedIncomeSourceRequest =
  components["schemas"]["ExpectedIncomeSourceRequestModel"];

export {
  type AccountingPeriod,
  type AccountingPeriodRange,
  AccountingPeriodSortModel as AccountingPeriodSort,
  type AccountingPeriodWithBalance,
  AccountingPeriodWithBalanceSortModel as AccountingPeriodWithBalanceSort,
  type AccountingPeriodsInRange,
  type CreateAccountingPeriodRequest,
  type ExpectedIncomeSource,
  type ExpectedIncomeSourcesByPeriod,
  ExpectedIncomeSourceSort,
  type ExpectedIncomeSourceRequest,
};
