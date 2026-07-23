import type {
  Account,
  AccountBalanceEvent,
  AccountBalanceEventDraft,
  AccountWithBalance,
} from "@/accounts/types";
import { isDebtAccountType } from "@/accounts/helpers";
import { isNullOrUndefined } from "@/framework/nullHelpers";

/**
 * Gets the balance change captured in the provided transaction account draft.
 */
const getTransactionAccountDraftBalanceChange = function (
  account: AccountBalanceEventDraft | null,
): number {
  return (
    (account?.newAccountBalance ?? 0) - (account?.previousAccountBalance ?? 0)
  );
};

/**
 * Applies a balance change to an existing transaction account draft.
 */
const setTransactionAccountDraftBalanceChange = function (
  account: AccountBalanceEventDraft | null,
  balanceChange: number | null | undefined,
): AccountBalanceEventDraft | null {
  // eslint-disable-next-line @typescript-eslint/prefer-optional-chain
  if (account === null || account.accountType === null) {
    return null;
  }
  let nextBalanceChange = balanceChange ?? 0;
  if (isDebtAccountType(account.accountType)) {
    nextBalanceChange *= -1;
  }
  return {
    ...account,
    newAccountBalance:
      (account.previousAccountBalance ?? 0) + nextBalanceChange,
  };
};

/**
 * Creates a transaction account draft from an account and balance delta.
 */
const createTransactionAccountDraftFromAccount = function (
  account: AccountWithBalance,
  balanceChange = 0,
): AccountBalanceEventDraft {
  const previousPostedBalance = account.currentBalance.postedBalance;
  return {
    accountId: account.id,
    accountName: account.name,
    accountType: account.type,
    postedDate: null,
    previousAccountBalance: previousPostedBalance,
    newAccountBalance: previousPostedBalance + balanceChange,
  };
};

/**
 * Hydrates a transaction account draft from a persisted transaction account.
 */
const getTransactionAccountDraftFromTransactionAccount = function (
  account: AccountBalanceEvent | null | undefined,
): AccountBalanceEventDraft | null {
  if (isNullOrUndefined(account)) {
    return null;
  }
  return {
    accountId: account.account.id,
    accountName: account.account.name,
    accountType: account.account.type,
    postedDate: account.eventDate ?? null,
    previousAccountBalance: account.previousBalance.postedBalance,
    newAccountBalance: account.newBalance.postedBalance,
  };
};

/**
 * Resolves the next selected transaction account draft from an account picker value.
 */
const getSelectedTransactionAccountDraft = function (
  accounts: AccountWithBalance[],
  nextValue: Account | null,
  currentAccount: AccountBalanceEventDraft | null,
  balanceChange?: number | null,
): AccountBalanceEventDraft | null {
  if (nextValue === null) {
    return null;
  }
  const selectedAccount =
    accounts.find((candidate) => candidate.id === nextValue.id) ?? null;
  if (selectedAccount === null) {
    return null;
  }
  return createTransactionAccountDraftFromAccount(
    selectedAccount,
    balanceChange ?? getTransactionAccountDraftBalanceChange(currentAccount),
  );
};

export {
  createTransactionAccountDraftFromAccount,
  getSelectedTransactionAccountDraft,
  getTransactionAccountDraftBalanceChange,
  getTransactionAccountDraftFromTransactionAccount,
  setTransactionAccountDraftBalanceChange,
};
