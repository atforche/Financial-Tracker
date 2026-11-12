import { type Account, AccountType } from "@/accounts/types";
import type {
  BalanceAdjustment,
  CreateTransactionRequest,
  UpdateTransactionRequest,
} from "@/transactions/types";
import {
  CreateTransactionModelCreateBalanceAdjustmentModelType,
  UpdateTransactionModelUpdateBalanceAdjustmentModelType,
} from "@/framework/data/api";
import type { AccountingPeriod } from "@/accounting-periods/types";
import type { Dayjs } from "dayjs";
import { getCurrencyDifference } from "@/framework/currencyHelpers";
import { validateDetails } from "@/transactions/workspace/helpers";

/**
 * Account types whose balance can be adjusted.
 */
const adjustableAccountTypes: readonly AccountType[] = [
  AccountType.Investment,
  AccountType.Retirement,
  AccountType.Debt,
];

/**
 * Determines whether an account's balance can be adjusted.
 */
const isAdjustableAccount = function (account: Account): boolean {
  return adjustableAccountTypes.includes(account.type);
};

/**
 * Gets the posted balance that a balance adjustment is measured against: the account's posted balance through the
 * date, without the adjustment being edited, since a saved adjustment is already posted on its own date.
 */
const getBalanceWithoutAdjustment = function (
  postedBalance: number | null,
  accountId: string | null,
  date: string | null,
  savedAdjustment: BalanceAdjustment | null,
): number | null {
  if (postedBalance === null) {
    return null;
  }
  if (
    savedAdjustment === null ||
    date === null ||
    savedAdjustment.account.account.id !== accountId ||
    savedAdjustment.date > date
  ) {
    return postedBalance;
  }
  return getCurrencyDifference(postedBalance, savedAdjustment.amount);
};

/**
 * Gets the change that brings the posted balance to the institution balance, or null while either is unknown.
 */
const getChange = function (
  institutionBalance: number | null,
  postedBalance: number | null,
): number | null {
  return institutionBalance === null || postedBalance === null
    ? null
    : getCurrencyDifference(institutionBalance, postedBalance);
};

/**
 * Validates everything a balance adjustment request needs. A change of zero would not adjust anything.
 */
const validateRequest = function (
  accountingPeriod: AccountingPeriod | null,
  date: Dayjs | null,
  defaultDate: Dayjs | null,
  description: string,
  accountId: string | null,
  change: number | null,
): boolean {
  return (
    validateDetails(accountingPeriod, date, defaultDate, description) &&
    accountId !== null &&
    change !== null &&
    change !== 0
  );
};

/**
 * Builds the create transaction request for a balance adjustment.
 */
const buildCreateRequest = function (
  accountingPeriod: AccountingPeriod | null,
  date: Dayjs | null,
  defaultDate: Dayjs | null,
  description: string,
  accountId: string | null,
  change: number | null,
): CreateTransactionRequest | null {
  if (
    !validateRequest(
      accountingPeriod,
      date,
      defaultDate,
      description,
      accountId,
      change,
    )
  ) {
    return null;
  }
  return {
    type: CreateTransactionModelCreateBalanceAdjustmentModelType.BalanceAdjustment,
    accountingPeriodId: accountingPeriod?.id ?? "",
    date: date?.format("YYYY-MM-DD") ?? defaultDate?.format("YYYY-MM-DD") ?? "",
    description,
    amount: change ?? 0,
    accountId: accountId ?? "",
  };
};

/**
 * Builds the update transaction request for a balance adjustment.
 */
const buildUpdateRequest = function (
  accountingPeriod: AccountingPeriod,
  date: Dayjs | null,
  description: string,
  accountId: string | null,
  change: number | null,
): UpdateTransactionRequest | null {
  if (
    !validateRequest(
      accountingPeriod,
      date,
      null,
      description,
      accountId,
      change,
    )
  ) {
    return null;
  }
  return {
    type: UpdateTransactionModelUpdateBalanceAdjustmentModelType.BalanceAdjustment,
    date: date?.format("YYYY-MM-DD") ?? "",
    description,
    amount: change ?? 0,
    accountId: accountId ?? "",
  };
};

export {
  buildCreateRequest,
  buildUpdateRequest,
  getBalanceWithoutAdjustment,
  getChange,
  isAdjustableAccount,
};
