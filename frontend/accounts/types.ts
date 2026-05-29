import {
  AccountDashboardSortOrderModel,
  AccountSortOrderModel,
  AccountTransactionSortOrderModel,
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
 * Type representing the Accounts dashboard response.
 */
interface AccountDashboard {
  readonly mode: AccountDashboardMode;
  readonly accounts: {
    readonly items: AccountDashboardAccount[];
    readonly totalCount: number;
  };
  readonly balanceEvents: {
    readonly items: AccountDashboardBalanceEvent[];
    readonly totalCount: number;
  };
  readonly availableAccountNames: readonly string[];
  readonly accountingPeriods: readonly AccountDashboardPeriodSummary[] | null;
  readonly dates: readonly AccountDashboardDateSummary[] | null;
}

/**
 * Type representing a row in the Accounts dashboard account table.
 */
interface AccountDashboardAccount {
  readonly id: string;
  readonly name: string;
  readonly type: AccountTypeModel;
  readonly startingBalance: number;
  readonly endingBalance: number;
}

/**
 * Type representing a row in the Accounts dashboard balance event table.
 */
interface AccountDashboardBalanceEvent {
  readonly accountId: string;
  readonly accountName: string;
  readonly date: string;
  readonly accountingPeriodId: string;
  readonly accountingPeriodName: string;
  readonly type: AccountDashboardBalanceEventType;
  readonly isPosted: boolean;
  readonly amount: number;
}

/**
 * Type representing a period summary in the Accounts dashboard response.
 */
interface AccountDashboardPeriodSummary {
  readonly accountingPeriodId: string;
  readonly accountingPeriodName: string;
  readonly year: number;
  readonly month: number;
  readonly totalOpeningBalance: number;
  readonly totalClosingBalance: number;
  readonly trackedOpeningBalance: number;
  readonly trackedClosingBalance: number;
  readonly untrackedOpeningBalance: number;
  readonly untrackedClosingBalance: number;
  readonly openingBalanceByAccountType: readonly AccountTypeBalance[];
  readonly closingBalanceByAccountType: readonly AccountTypeBalance[];
}

/**
 * Type representing a date summary in the Accounts dashboard response.
 */
interface AccountDashboardDateSummary {
  readonly date: string;
  readonly totalBalance: number;
  readonly trackedBalance: number;
  readonly untrackedBalance: number;
  readonly balanceByAccountType: readonly AccountTypeBalance[];
}

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
 * Enum representing the dashboard response time mode.
 */
enum AccountDashboardMode {
  AccountingPeriod = "AccountingPeriod",
  Date = "Date",
}

/**
 * Enum representing balance event types on the account dashboard.
 */
enum AccountDashboardBalanceEventType {
  Debit = "Debit",
  Credit = "Credit",
}

/**
 * Enum representing the supported balance event sort orders on the account dashboard.
 */
enum AccountDashboardBalanceEventSortOrder {
  AccountName = "AccountName",
  AccountNameDescending = "AccountNameDescending",
  AccountingPeriodName = "AccountingPeriodName",
  AccountingPeriodNameDescending = "AccountingPeriodNameDescending",
  Date = "Date",
  DateDescending = "DateDescending",
  Type = "Type",
  TypeDescending = "TypeDescending",
  Amount = "Amount",
  AmountDescending = "AmountDescending",
}

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
  type AccountDashboard,
  type AccountDashboardAccount,
  type AccountDashboardBalanceEvent,
  type AccountDashboardDateSummary,
  type AccountDashboardPeriodSummary,
  type AccountSummary,
  type AccountTypeBalance,
  type AccountIdentifier,
  type CreateAccountRequest,
  type OnboardAccountRequest,
  type UpdateAccountRequest,
  AccountDashboardBalanceEventSortOrder,
  AccountDashboardBalanceEventType,
  AccountDashboardMode,
  AccountDashboardSortOrderModel as AccountDashboardSortOrder,
  AccountSortOrderModel as AccountSortOrder,
  AccountTransactionSortOrderModel as AccountTransactionSortOrder,
  AccountTypeModel as AccountType,
  isTrackedAccountType,
  isDebtAccountType,
  isPositiveChangeInBalance,
  formatAccountType,
};
