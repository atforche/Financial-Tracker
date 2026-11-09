import {
  AccountType,
  type CreateAccountRequest,
  type OnboardAccountRequest,
} from "@/accounts/types";
import {
  getDefaultDate,
  getMaximumDate,
  getMinimumDate,
} from "@/accounting-periods/helpers";
import type { AccountingPeriod } from "@/accounting-periods/types";
import type { Dayjs } from "dayjs";
import isAccountGoalRangeValid from "@/account-goals/isAccountGoalRangeValid";

/**
 * Gets the normalized date opened for creating an account.
 */
const getNormalizedDateOpened = function (
  accountingPeriod: AccountingPeriod | null,
  dateOpened: Dayjs | null,
): Dayjs | null {
  if (accountingPeriod === null) {
    return null;
  }
  const minimumDate = getMinimumDate(accountingPeriod);
  const maximumDate = getMaximumDate(accountingPeriod);
  if (
    dateOpened === null ||
    dateOpened.isBefore(minimumDate) ||
    dateOpened.isAfter(maximumDate)
  ) {
    return getDefaultDate(accountingPeriod);
  }
  return dateOpened;
};

/**
 * An account goal's bounds as entered on the account form.
 */
interface AccountGoalDraft {
  readonly minimumEndingBalance: number;
  readonly maximumEndingBalance: number | null;
}

/**
 * Gets the goal bounds to send with a new account: none unless it is a standard account, which has goals, and null
 * when they are out of order.
 */
const getGoalBounds = function (
  accountType: AccountType,
  { minimumEndingBalance, maximumEndingBalance }: AccountGoalDraft,
): Pick<
  CreateAccountRequest,
  "minimumEndingBalance" | "maximumEndingBalance"
> | null {
  if (accountType !== AccountType.Standard) {
    return {};
  }
  return isAccountGoalRangeValid(minimumEndingBalance, maximumEndingBalance)
    ? { minimumEndingBalance, maximumEndingBalance }
    : null;
};

/**
 * Validates a request to create an account.
 */
const buildCreateRequest = function (
  name: string,
  financialInstitution: string | null,
  accountType: AccountType | null,
  accountingPeriod: AccountingPeriod | null,
  dateOpened: Dayjs | null,
  goal: AccountGoalDraft,
): CreateAccountRequest | null {
  if (
    name === "" ||
    accountType === null ||
    accountingPeriod === null ||
    dateOpened === null
  ) {
    return null;
  }
  const goalBounds = getGoalBounds(accountType, goal);
  return goalBounds === null
    ? null
    : {
        name,
        financialInstitution,
        type: accountType,
        openingAccountingPeriodId: accountingPeriod.id,
        dateOpened: dateOpened.format("YYYY-MM-DD"),
        ...goalBounds,
      };
};

/**
 * Builds a request to onboard an account.
 */
const buildOnboardRequest = function (
  name: string,
  financialInstitution: string | null,
  accountType: AccountType | null,
  onboardedBalance: number | null,
  goal: AccountGoalDraft,
): OnboardAccountRequest | null {
  if (name === "" || accountType === null || onboardedBalance === null) {
    return null;
  }
  const goalBounds = getGoalBounds(accountType, goal);
  return goalBounds === null
    ? null
    : {
        name,
        financialInstitution,
        type: accountType,
        onboardedBalance,
        ...goalBounds,
      };
};

/**
 * Gets the card color to display for the account type.
 */
const getAccountCardColor = function (
  accountType: AccountType,
): "primary" | "warning" | "error" {
  if (accountType === AccountType.CreditCard) {
    return "warning";
  }
  return accountType === AccountType.Debt ? "error" : "primary";
};

export {
  buildCreateRequest,
  buildOnboardRequest,
  getAccountCardColor,
  getNormalizedDateOpened,
};
