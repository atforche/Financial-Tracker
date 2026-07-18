import { AccountType, type CreateAccountRequest } from "@/accounts/types";
import {
  getDefaultDate,
  getMaximumDate,
  getMinimumDate,
} from "@/accounting-periods/helpers";
import type { AccountingPeriod } from "@/accounting-periods/types";
import type { Dayjs } from "dayjs";
import type { FrameColor } from "@/framework/view/Frame";

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
 * Validates a request to create an account.
 */
const validateCreateRequest = function (
  name: string,
  accountType: AccountType | null,
  accountingPeriod: AccountingPeriod | null,
  dateOpened: Dayjs | null,
): boolean {
  return (
    name !== "" &&
    accountType !== null &&
    accountingPeriod !== null &&
    dateOpened !== null
  );
};

/**
 * Builds a request to create an account.
 */
const buildCreateRequest = function (
  name: string,
  accountType: AccountType | null,
  accountingPeriod: AccountingPeriod | null,
  dateOpened: Dayjs | null,
): CreateAccountRequest | null {
  if (!validateCreateRequest(name, accountType, accountingPeriod, dateOpened)) {
    return null;
  }
  return {
    name,
    type: accountType ?? AccountType.Standard,
    openingAccountingPeriodId: accountingPeriod?.id ?? "",
    dateOpened: dateOpened?.format("YYYY-MM-DD") ?? "",
  };
};

/**
 * Validates a request to onboard an account.
 */
const validateOnboardRequest = function (
  name: string,
  accountType: AccountType | null,
  onboardedBalance: number | null,
): boolean {
  return name !== "" && accountType !== null && onboardedBalance !== null;
};

/**
 * Builds a request to onboard an account.
 */
const buildOnboardRequest = function (
  name: string,
  accountType: AccountType | null,
  onboardedBalance: number | null,
): { name: string; type: AccountType; onboardedBalance: number } | null {
  if (!validateOnboardRequest(name, accountType, onboardedBalance)) {
    return null;
  }
  return {
    name,
    type: accountType ?? AccountType.Standard,
    onboardedBalance: onboardedBalance ?? 0,
  };
};

/**
 * Gets the account card color to display for the account type.
 */
const getAccountCardColor = function (
  accountType: AccountType | null,
): FrameColor {
  // eslint-disable-next-line @typescript-eslint/switch-exhaustiveness-check
  switch (accountType) {
    case AccountType.CreditCard:
      return "warning";
    case AccountType.Debt:
      return "error";
    default:
      return "primary";
  }
};

export {
  getNormalizedDateOpened,
  validateCreateRequest,
  buildCreateRequest,
  validateOnboardRequest,
  buildOnboardRequest,
  getAccountCardColor,
};
