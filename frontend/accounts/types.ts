import {
  AccountBalanceEventSortModel,
  AccountTypeModel,
  AccountWithBalanceRangeSortModel,
  type components,
} from "@/framework/data/api";

/**
 * Type representing an Account.
 */
type Account = components["schemas"]["AccountModel"];

/**
 * Type representing an Account along with its current balance.
 */
type AccountWithBalance = components["schemas"]["AccountWithBalanceModel"];

/**
 * Type representing an Account with a balance range.
 */
type AccountWithBalanceRange =
  components["schemas"]["AccountWithBalanceRangeModel"];

/**
 * Type representing an Account balance summary.
 */
type AccountBalanceSummary =
  components["schemas"]["AccountBalanceSummaryModel"];

/**
 * Type representing an Account balance summary by date.
 */
type AccountBalanceSummaryByDate =
  components["schemas"]["AccountBalanceSummaryByDateModel"];

/**
 * Type representing an Account balance summary by period.
 */
type AccountBalanceSummaryByPeriod =
  components["schemas"]["AccountBalanceSummaryByPeriodModel"];

/**
 * Type representing an account balance event..
 */
type AccountBalanceEvent = components["schemas"]["AccountBalanceEventModel"];

/**
 * Interface representing a draft of an account balance event.
 */
interface AccountBalanceEventDraft {
  readonly accountId: string | null;
  readonly accountName: string | null;
  readonly accountType: AccountTypeModel | null;
  readonly postedDate: string | null;
  readonly previousAccountBalance: number | null;
  readonly newAccountBalance: number | null;
}

/**
 * Type representing a collection of accounts in a date range.
 */
type AccountsInDateRange = components["schemas"]["AccountsInDateRangeModel"];

/**
 * Type representing a collection of accounts in an accounting period range.
 */
type AccountsInAccountingPeriodRange =
  components["schemas"]["AccountsInAccountingPeriodRangeModel"];

/**
 * Type representing an Account Type balance summary.
 */
type AccountTypeBalance = components["schemas"]["AccountTypeBalanceModel"];

/**
 * Type representing a request to create an account.
 */
type CreateAccountRequest = components["schemas"]["CreateAccountModel"];

/**
 * Type representing a request to onboard an account.
 */
type OnboardAccountRequest = components["schemas"]["OnboardAccountModel"];

/**
 * Type representing a request to update an account.
 */
type UpdateAccountRequest = components["schemas"]["UpdateAccountModel"];

export {
  type Account,
  type AccountWithBalance,
  type AccountWithBalanceRange,
  AccountWithBalanceRangeSortModel as AccountWithBalanceRangeSort,
  type AccountBalanceSummary,
  type AccountBalanceSummaryByDate,
  type AccountBalanceSummaryByPeriod,
  type AccountBalanceEvent,
  type AccountBalanceEventDraft,
  AccountBalanceEventSortModel as AccountBalanceEventSort,
  type AccountsInDateRange,
  type AccountsInAccountingPeriodRange,
  type AccountTypeBalance,
  type CreateAccountRequest,
  type OnboardAccountRequest,
  type UpdateAccountRequest,
  AccountTypeModel as AccountType,
};
