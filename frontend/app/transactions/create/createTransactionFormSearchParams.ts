import type { AccountIdentifier } from "@/data/accountTypes";
import type { AccountingPeriod } from "@/data/accountingPeriodTypes";
import ToggleState from "@/app/transactions/create/toggleState";

/**
 * Type representing the search params for the Create Transaction Form.
 */
interface CreateTransactionFormSearchParams {
  accountingPeriodId?: string;
  debitAccountId?: string;
  creditAccountId?: string;
  debitFundId?: string;
  creditFundId?: string;
}

/**
 * Gets the default accounting period for this form.
 */
const getDefaultAccountingPeriod = function (
  accountingPeriods: AccountingPeriod[],
  searchParams: CreateTransactionFormSearchParams,
): AccountingPeriod | null {
  if (typeof searchParams.accountingPeriodId !== "undefined") {
    return (
      accountingPeriods.find(
        (ap) => ap.id === searchParams.accountingPeriodId,
      ) ?? null
    );
  }
  return null;
};

/**
 * Gets the default debit account for this form.
 */
const getDefaultDebitAccount = function (
  accounts: AccountIdentifier[],
  searchParams: CreateTransactionFormSearchParams,
): AccountIdentifier | null {
  if (typeof searchParams.debitAccountId !== "undefined") {
    return (
      accounts.find((account) => account.id === searchParams.debitAccountId) ??
      null
    );
  }
  return null;
};

/**
 * Gets the default credit account for this form.
 */
const getDefaultCreditAccount = function (
  accounts: AccountIdentifier[],
  searchParams: CreateTransactionFormSearchParams,
): AccountIdentifier | null {
  if (typeof searchParams.creditAccountId !== "undefined") {
    return (
      accounts.find((account) => account.id === searchParams.creditAccountId) ??
      null
    );
  }
  return null;
};

/**
 * Gets the initial toggle state for the CreateTransactionAccountFrame based on the required accounts and funds.
 */
const getInitialToggleState = function (
  searchParams: CreateTransactionFormSearchParams,
): ToggleState {
  if (
    typeof searchParams.debitAccountId !== "undefined" ||
    typeof searchParams.debitFundId !== "undefined"
  ) {
    return ToggleState.Debit;
  } else if (
    typeof searchParams.creditAccountId !== "undefined" ||
    typeof searchParams.creditFundId !== "undefined"
  ) {
    return ToggleState.Credit;
  }
  return ToggleState.Debit;
};

export {
  type CreateTransactionFormSearchParams,
  getDefaultAccountingPeriod,
  getDefaultDebitAccount,
  getDefaultCreditAccount,
  getInitialToggleState,
};
