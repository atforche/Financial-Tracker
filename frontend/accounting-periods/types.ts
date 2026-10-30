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
 * Type representing an accounting period with its Expected Income Sources.
 */
type AccountingPeriodWithExpectedIncomeSources =
  components["schemas"]["AccountingPeriodWithExpectedIncomeSourcesModel"];

/**
 * Type representing an Income Transaction linked to an Expected Income Source.
 */
type ExpectedIncomeSourceIncome =
  components["schemas"]["ExpectedIncomeSourceIncomeModel"];

/**
 * Expected Income Sources keyed by the ID of the accounting period they belong to.
 */
type ExpectedIncomeSourcesByPeriod = Readonly<
  Record<string, readonly ExpectedIncomeSource[]>
>;

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
  type AccountingPeriodWithExpectedIncomeSources,
  AccountingPeriodWithBalanceSortModel as AccountingPeriodWithBalanceSort,
  type AccountingPeriodsInRange,
  type CreateAccountingPeriodRequest,
  type ExpectedIncomeSource,
  type ExpectedIncomeSourceIncome,
  type ExpectedIncomeSourcesByPeriod,
  type ExpectedIncomeSourceRequest,
};
