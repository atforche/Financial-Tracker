import {
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
  type AccountBalanceEvent,
  type AccountBalanceEventDraft,
  type AccountsInDateRange,
  type AccountsInAccountingPeriodRange,
  type CreateAccountRequest,
  type OnboardAccountRequest,
  type UpdateAccountRequest,
  AccountTypeModel as AccountType,
};
