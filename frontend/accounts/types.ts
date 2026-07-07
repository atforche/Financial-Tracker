import {
  AccountSortOrderModel,
  AccountTrendsBalanceEventSortOrderModel,
  AccountTrendsBalanceEventTypeModel,
  AccountTrendsModeModel,
  AccountTrendsSortOrderModel,
  AccountTypeModel,
  type components,
} from "@/framework/data/api";

/**
 * Type representing an Account.
 */
type Account = components["schemas"]["AccountModel"];

/**
 * Type representing Account summary balances.
 */
type AccountSummary = components["schemas"]["AccountSummaryModel"];

/**
 * Type representing the current Accounts response.
 */
type CurrentAccounts = components["schemas"]["CurrentAccountsModel"];

/**
 * Type representing an Account card on the current Accounts page.
 */
type CurrentAccount = components["schemas"]["CurrentAccountModel"];

/**
 * Type representing a recent balance event on the current Accounts page.
 */
type CurrentAccountBalanceEvent =
  components["schemas"]["CurrentAccountBalanceEventModel"];

/**
 * Type representing a balance event on the Account workspace page.
 */
type AccountWorkspaceBalanceEvent =
  components["schemas"]["AccountWorkspaceBalanceEventModel"];

/**
 * Type representing the Accounts trends response.
 */
type AccountTrends = components["schemas"]["AccountTrendsModel"];

/**
 * Type representing a row in the Accounts trends account table.
 */
type AccountTrendsAccount = components["schemas"]["AccountTrendsAccountModel"];

/**
 * Type representing a row in the Accounts trends balance event table.
 */
type AccountTrendsBalanceEvent =
  components["schemas"]["AccountTrendsBalanceEventModel"];

/**
 * Type representing a period summary in the Accounts trends response.
 */
type AccountTrendsPeriodSummary =
  components["schemas"]["AccountTrendsPeriodSummaryModel"];

/**
 * Type representing a date summary in the Accounts trends response.
 */
type AccountTrendsDateSummary =
  components["schemas"]["AccountTrendsDateSummaryModel"];

/**
 * Type representing an Account Type balance summary.
 */
type AccountTypeBalance = components["schemas"]["AccountTypeBalanceModel"];

/**
 * Interface representing an identifier for an Account.
 */
interface AccountIdentifier {
  id: string;
  name: string;
}

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

/**
 * Determines if the provided account type supports tracked fund assignments.
 */
const isTrackedAccountType = function (accountType: AccountTypeModel): boolean {
  switch (accountType) {
    case AccountTypeModel.Standard:
    case AccountTypeModel.CreditCard:
      return true;
    case AccountTypeModel.Investment:
    case AccountTypeModel.Debt:
    case AccountTypeModel.Retirement:
    case AccountTypeModel.Escrow:
      return false;
    default:
      return false;
  }
};

/**
 * Determines if the provided account type is a debt account type.
 */
const isDebtAccountType = function (accountType: AccountTypeModel): boolean {
  return (
    accountType === AccountTypeModel.Debt ||
    accountType === AccountTypeModel.CreditCard
  );
};

/**
 * Determines if the provided change in balance is "positive" based on the provided account type.
 */
const isPositiveChangeInBalance = function (
  accountType: AccountTypeModel,
  changeInBalance: number,
): boolean {
  if (accountType === AccountTypeModel.Debt) {
    return changeInBalance <= 0;
  }
  return changeInBalance >= 0;
};

/**
 * Formats the provided account type into a readable string.
 */
const formatAccountType = function (accountType: AccountTypeModel): string {
  switch (accountType) {
    case AccountTypeModel.Standard:
      return "Standard";
    case AccountTypeModel.CreditCard:
      return "Credit Card";
    case AccountTypeModel.Investment:
      return "Investment";
    case AccountTypeModel.Debt:
      return "Debt";
    case AccountTypeModel.Retirement:
      return "Retirement";
    case AccountTypeModel.Escrow:
      return "Escrow";
    default:
      return accountType;
  }
};

export {
  type Account,
  type AccountSummary,
  type CurrentAccounts,
  type CurrentAccount,
  type CurrentAccountBalanceEvent,
  type AccountWorkspaceBalanceEvent,
  type AccountTrends,
  type AccountTrendsAccount,
  type AccountTrendsBalanceEvent,
  type AccountTrendsDateSummary,
  type AccountTrendsPeriodSummary,
  type AccountTypeBalance,
  type AccountIdentifier,
  type CreateAccountRequest,
  type OnboardAccountRequest,
  type UpdateAccountRequest,
  AccountTrendsBalanceEventSortOrderModel as AccountTrendsBalanceEventSortOrder,
  AccountTrendsBalanceEventTypeModel as AccountTrendsBalanceEventType,
  AccountTrendsModeModel as AccountTrendsMode,
  AccountTrendsSortOrderModel as AccountTrendsSortOrder,
  AccountSortOrderModel as AccountSortOrder,
  AccountTypeModel as AccountType,
  isTrackedAccountType,
  isDebtAccountType,
  isPositiveChangeInBalance,
  formatAccountType,
};
