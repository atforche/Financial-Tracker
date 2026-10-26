import type {
  Account,
  AccountBalanceEvent,
  AccountBalanceEventDraft,
  AccountWithBalance,
} from "@/accounts/types";
import { isNullOrUndefined } from "@/framework/nullHelpers";

/**
 * Creates a transaction account draft from an account.
 */
const createTransactionAccountDraftFromAccount = function (
  account: AccountWithBalance,
): AccountBalanceEventDraft {
  return {
    accountId: account.id,
    accountName: account.name,
    accountType: account.type,
    postedDate: null,
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
  };
};

/**
 * Resolves the next selected transaction account draft from an account picker value.
 */
const getSelectedTransactionAccountDraft = function (
  accounts: AccountWithBalance[],
  nextValue: Account | null,
): AccountBalanceEventDraft | null {
  if (nextValue === null) {
    return null;
  }
  const selectedAccount =
    accounts.find((candidate) => candidate.id === nextValue.id) ?? null;
  if (selectedAccount === null) {
    return null;
  }
  return createTransactionAccountDraftFromAccount(selectedAccount);
};

export {
  getSelectedTransactionAccountDraft,
  getTransactionAccountDraftFromTransactionAccount,
};
