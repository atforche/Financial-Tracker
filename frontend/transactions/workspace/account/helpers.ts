import {
  type Account,
  type AccountIdentifier,
  isTrackedAccountType,
} from "@/accounts/types";
import type {
  AccountTransaction,
  AccountTransactionDestination,
} from "@/transactions/accountTransaction";
import {
  CreateTransactionModelCreateAccountTransactionModelType,
  UpdateTransactionModelUpdateAccountTransactionModelType,
} from "@/framework/data/api";
import type {
  CreateTransactionRequest,
  TransactionAccountDraft,
  UpdateTransactionRequest,
} from "@/transactions/transaction";
import {
  validateDetails,
  validateSummary,
} from "@/transactions/workspace/helpers";
import type { AccountingPeriod } from "@/accounting-periods/types";
import type { Dayjs } from "dayjs";
import { getTransactionAccountDraftFromTransactionAccount } from "@/transactions/workspace/transactionAccountDraft";

/**
 * Interface representing a potentially unfinished account transaction source.
 */
interface AccountSourceDraft {
  readonly account: TransactionAccountDraft | null;
  readonly location: string;
  readonly amount: number | null;
  readonly postedDate: string | null;
  readonly previousAccountBalance: number | null;
  readonly newAccountBalance: number | null;
}

/**
 * Interface representing a potentially unfinished account transaction destination.
 */
interface AccountDestinationDraft {
  readonly account: TransactionAccountDraft | null;
  readonly location: string;
  readonly amount: number | null;
  readonly postedDate: string | null;
  readonly previousAccountBalance: number | null;
  readonly newAccountBalance: number | null;
}

/**
 * Creates an empty source draft.
 */
const createEmptySource = function (): AccountSourceDraft {
  return {
    account: null,
    location: "",
    amount: null,
    postedDate: null,
    previousAccountBalance: null,
    newAccountBalance: null,
  };
};

/**
 * Creates an empty destination draft.
 */
const createEmptyDestination = function (): AccountDestinationDraft {
  return {
    account: null,
    location: "",
    amount: null,
    postedDate: null,
    previousAccountBalance: null,
    newAccountBalance: null,
  };
};

/**
 * Validates the source of an account transaction.
 */
const validateSource = function (source: AccountSourceDraft): boolean {
  const hasAccount = source.account !== null;
  const hasLocation = source.location.trim() !== "";
  if (!hasAccount && !hasLocation) {
    return false;
  }
  return source.amount !== null && source.amount > 0;
};

/**
 * Validates the destination of an account transaction.
 */
const validateDestination = function (
  destination: AccountDestinationDraft,
  sourceAccount: TransactionAccountDraft | null,
): boolean {
  const normalizedLocation = destination.location.trim();
  const hasAccount = destination.account !== null;
  const hasLocation = normalizedLocation !== "";
  const destinationIsTracked =
    // eslint-disable-next-line @typescript-eslint/prefer-optional-chain
    destination.account !== null &&
    destination.account.accountType !== null &&
    isTrackedAccountType(destination.account.accountType);

  if (destination.amount === null || destination.amount <= 0) {
    return false;
  }
  if ((hasAccount && hasLocation) || (!hasAccount && !hasLocation)) {
    return false;
  }
  if (destination.account?.accountId === sourceAccount?.accountId) {
    return false;
  }
  if (
    // eslint-disable-next-line @typescript-eslint/prefer-optional-chain
    sourceAccount !== null &&
    sourceAccount.accountType !== null &&
    isTrackedAccountType(sourceAccount.accountType)
  ) {
    return hasAccount && destinationIsTracked;
  }
  if (sourceAccount === null) {
    return !hasAccount || !destinationIsTracked;
  }
  return !hasAccount || !destinationIsTracked;
};

/**
 * Validates the entire account transaction request.
 */
const validateRequest = function (
  accountingPeriod: AccountingPeriod | null,
  date: Dayjs | null,
  defaultDate: Dayjs | null,
  description: string,
  source: AccountSourceDraft,
  destinations: AccountDestinationDraft[],
): boolean {
  const destinationTotal = destinations.reduce(
    (total, destination) => total + (destination.amount ?? 0),
    0,
  );
  const hasUniqueDestinationAccounts =
    new Set(destinations.map((d) => d.account?.accountId ?? null)).size ===
    destinations.length;
  const hasUniqueDestinationLocations =
    new Set(destinations.map((d) => d.location.trim() || null)).size ===
    destinations.length;
  const areDestinationsComplete = destinations.every((d) =>
    validateDestination(d, source.account),
  );
  return (
    validateDetails(accountingPeriod, date, defaultDate, description) &&
    validateSource(source) &&
    validateSummary(source.amount, destinationTotal, destinations.length) &&
    hasUniqueDestinationAccounts &&
    hasUniqueDestinationLocations &&
    areDestinationsComplete
  );
};

/**
 * Builds a create transaction request from the provided parameters.
 */
const buildCreateRequest = function (
  accountingPeriod: AccountingPeriod | null,
  date: Dayjs | null,
  defaultDate: Dayjs | null,
  description: string,
  source: AccountSourceDraft,
  destinations: AccountDestinationDraft[],
): CreateTransactionRequest | null {
  if (
    validateRequest(
      accountingPeriod,
      date,
      defaultDate,
      description,
      source,
      destinations,
    )
  ) {
    return {
      type: CreateTransactionModelCreateAccountTransactionModelType.Account,
      accountingPeriodId: accountingPeriod?.id ?? "",
      date:
        date?.format("YYYY-MM-DD") ?? defaultDate?.format("YYYY-MM-DD") ?? "",
      description,
      amount: source.amount ?? 0,
      source: {
        accountId: source.account?.accountId ?? null,
        location:
          source.account === null ? source.location.trim() || null : null,
      },
      destinations: destinations.map((destination) => ({
        accountId: destination.account?.accountId ?? null,
        location:
          destination.account === null
            ? destination.location.trim() || null
            : null,
        amount: destination.amount ?? 0,
      })),
    };
  }
  return null;
};

/**
 * Builds an update transaction request from the provided parameters.
 */
const buildUpdateRequest = function (
  accountingPeriod: AccountingPeriod | null,
  date: Dayjs | null,
  description: string,
  source: AccountSourceDraft,
  destinations: AccountDestinationDraft[],
): UpdateTransactionRequest | null {
  if (
    validateRequest(
      accountingPeriod,
      date,
      null,
      description,
      source,
      destinations,
    )
  ) {
    return {
      type: UpdateTransactionModelUpdateAccountTransactionModelType.Account,
      date: date?.format("YYYY-MM-DD") ?? "",
      description,
      amount: source.amount ?? 0,
      source: {
        accountId: source.account?.accountId ?? null,
        location:
          source.account === null ? source.location.trim() || null : null,
      },
      destinations: destinations.map((destination) => ({
        accountId: destination.account?.accountId ?? null,
        location:
          destination.account === null
            ? destination.location.trim() || null
            : null,
        amount: destination.amount ?? 0,
      })),
    };
  }
  return null;
};

/**
 * Builds a filter callback for the source account dropdown.
 */
const buildSourceAccountFilter = function (
  accounts: Account[],
  destinations: AccountDestinationDraft[],
) {
  return function (account: AccountIdentifier): boolean {
    const selectedAccount =
      accounts.find((candidate) => candidate.id === account.id) ?? null;
    return (
      selectedAccount !== null &&
      !destinations.some(
        (destination) => destination.account?.accountId === account.id,
      )
    );
  };
};

/**
 * Builds a filter callback for the destination account dropdown.
 */
const buildDestinationAccountFilter = function (
  accounts: Account[],
  destinations: AccountDestinationDraft[],
  index: number,
  sourceAccount: TransactionAccountDraft | null,
) {
  return function (account: AccountIdentifier): boolean {
    const selectedAccount =
      accounts.find((candidate) => candidate.id === account.id) ?? null;
    const accountUsedElsewhere = destinations.some(
      (currentDestination, currentIndex) =>
        currentIndex !== index &&
        currentDestination.account?.accountId === account.id,
    );
    if (selectedAccount === null || accountUsedElsewhere) {
      return false;
    }
    if (account.id === sourceAccount?.accountId) {
      return false;
    }
    if (
      // eslint-disable-next-line @typescript-eslint/prefer-optional-chain
      sourceAccount !== null &&
      sourceAccount.accountType !== null &&
      isTrackedAccountType(sourceAccount.accountType)
    ) {
      return isTrackedAccountType(selectedAccount.type);
    }
    return !isTrackedAccountType(selectedAccount.type);
  };
};

/**
 * Gets the source from the provided account transaction.
 */
const getSourceFromTransaction = function (
  transaction: AccountTransaction,
): AccountSourceDraft {
  return {
    account: getTransactionAccountDraftFromTransactionAccount(
      transaction.source.account,
    ),
    location: transaction.source.location ?? "",
    amount: transaction.amount,
    postedDate: transaction.source.account?.postedDate ?? null,
    previousAccountBalance:
      transaction.source.account?.previousAccountBalance.postedBalance ?? null,
    newAccountBalance:
      transaction.source.account?.newAccountBalance.postedBalance ?? null,
  };
};

/**
 * Gets the collection of destinations from the provided account transaction.
 */
const getDestinationsFromTransaction = function (
  transaction: AccountTransaction,
): AccountDestinationDraft[] {
  return transaction.destinations.map(
    (destination: AccountTransactionDestination) => ({
      account: getTransactionAccountDraftFromTransactionAccount(
        destination.account,
      ),
      location: destination.location ?? "",
      amount: destination.amount,
      postedDate: destination.account?.postedDate ?? null,
      previousAccountBalance:
        destination.account?.previousAccountBalance.postedBalance ?? null,
      newAccountBalance:
        destination.account?.newAccountBalance.postedBalance ?? null,
    }),
  );
};

export type { AccountDestinationDraft, AccountSourceDraft };
export {
  CreateTransactionModelCreateAccountTransactionModelType as CreateAccountTransactionType,
  UpdateTransactionModelUpdateAccountTransactionModelType as UpdateAccountTransactionType,
  buildCreateRequest,
  buildUpdateRequest,
  buildSourceAccountFilter,
  buildDestinationAccountFilter,
  createEmptyDestination,
  createEmptySource,
  getSourceFromTransaction,
  getDestinationsFromTransaction,
  validateDestination,
  validateSource,
};
