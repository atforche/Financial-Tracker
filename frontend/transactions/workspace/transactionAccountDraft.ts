import {
  type AccountIdentifier,
  type AccountWithBalance,
  isDebtAccountType,
} from "@/accounts/types";
import type {
  TransactionAccount,
  TransactionAccountDraft,
} from "@/transactions/transaction";

/**
 * Gets the balance change captured in the provided transaction account draft.
 */
const getTransactionAccountDraftBalanceChange = function (
  account: TransactionAccountDraft | null,
): number {
  return (
    (account?.newAccountBalance ?? 0) - (account?.previousAccountBalance ?? 0)
  );
};

/**
 * Applies a balance change to an existing transaction account draft.
 */
const setTransactionAccountDraftBalanceChange = function (
  account: TransactionAccountDraft | null,
  balanceChange: number | null | undefined,
): TransactionAccountDraft | null {
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
): TransactionAccountDraft {
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
  account: TransactionAccount | null | undefined,
): TransactionAccountDraft | null {
  if (typeof account === "undefined" || account === null) {
    return null;
  }
  return {
    accountId: account.account.id,
    accountName: account.account.name,
    accountType: account.account.type,
    postedDate: account.date ?? null,
    previousAccountBalance: account.previousBalance.postedBalance,
    newAccountBalance: account.newBalance.postedBalance,
  };
};

/**
 * Resolves the next selected transaction account draft from an account picker value.
 */
const getSelectedTransactionAccountDraft = function (
  accounts: AccountWithBalance[],
  nextValue: AccountIdentifier | null,
  currentAccount: TransactionAccountDraft | null,
  balanceChange?: number | null,
): TransactionAccountDraft | null {
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
