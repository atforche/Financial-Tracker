import {
  type Account,
  type AccountIdentifier,
  isTrackedAccountType,
} from "@/accounts/types";
import {
  CreateTransactionModelCreateSpendingTransactionModelType,
  UpdateTransactionModelUpdateSpendingTransactionModelType,
} from "@/framework/data/api";
import type {
  CreateTransactionRequest,
  UpdateTransactionRequest,
} from "@/transactions/transaction";
import { type FundAmount, hasIncompleteFundAssignments } from "@/funds/types";
import type {
  SpendingTransaction,
  SpendingTransactionDestination,
} from "@/transactions/spendingTransaction";
import type { AccountingPeriod } from "@/accounting-periods/types";
import type { Dayjs } from "dayjs";

/**
 * Interface representing a potentially unfinished spending transaction source.
 */
interface SpendingSourceDraft {
  readonly account: Account | null;
  readonly amount: number | null;
}

/**
 * Interface representing a potentially unfinished spending transaction destination.
 */
interface SpendingDestinationDraft {
  readonly account: Account | null;
  readonly location: string | null;
  readonly amount: number | null;
  readonly fundAssignments: FundAmount[];
  readonly baselineFundAssignments: FundAmount[];
}

/**
 * Creates an empty source draft.
 */
const createEmptySource = function (): SpendingSourceDraft {
  return {
    account: null,
    amount: null,
  };
};

/**
 * Creates an empty destination draft.
 */
const createEmptyDestination = function (): SpendingDestinationDraft {
  return {
    account: null,
    location: null,
    amount: null,
    fundAssignments: [],
    baselineFundAssignments: [],
  };
};

/**
 * Validates the source of a spending transaction.
 */
const validateSource = function (source: SpendingSourceDraft): boolean {
  if (source.account === null) {
    return false;
  }
  if (!isTrackedAccountType(source.account.type)) {
    return false;
  }
  return source.amount !== null && source.amount > 0;
};

/**
 * Validates the destination of a spending transaction.
 */
const validateDestination = function (
  destination: SpendingDestinationDraft,
  sourceAccount: Account | null,
): boolean {
  const normalizedLocation = destination.location?.trim() ?? "";
  const hasAccount = destination.account !== null;
  const hasLocation = normalizedLocation !== "";
  if (destination.amount === null || destination.amount <= 0) {
    return false;
  }
  if ((hasAccount && hasLocation) || (!hasAccount && !hasLocation)) {
    return false;
  }
  if (destination.account?.id === sourceAccount?.id) {
    return false;
  }
  if (hasAccount && isTrackedAccountType(destination.account.type)) {
    return false;
  }
  if (!hasAccount && destination.fundAssignments.length > 0) {
    return false;
  }
  if (hasAccount && hasIncompleteFundAssignments(destination.fundAssignments)) {
    return false;
  }
  return true;
};

/**
 * Validates the entire spending transaction request.
 */
const validateRequest = function (
  accountingPeriod: AccountingPeriod | null,
  date: Dayjs | null,
  defaultDate: Dayjs | null,
  description: string,
  source: SpendingSourceDraft,
  destinations: SpendingDestinationDraft[],
): boolean {
  const destinationTotal = destinations.reduce(
    (total, destination) => total + (destination.amount ?? 0),
    0,
  );
  return (
    accountingPeriod !== null &&
    (date !== null || defaultDate !== null) &&
    description !== "" &&
    validateSource(source) &&
    destinations.length > 0 &&
    destinations.every((destination) =>
      validateDestination(destination, source.account),
    ) &&
    source.amount === destinationTotal
  );
};

/**
 * Builds the create transaction request object from .
 */
const buildCreateRequest = function (
  accountingPeriod: AccountingPeriod | null,
  date: Dayjs | null,
  defaultDate: Dayjs | null,
  description: string,
  source: SpendingSourceDraft,
  destinations: SpendingDestinationDraft[],
): CreateTransactionRequest | null {
  if (
    !validateRequest(
      accountingPeriod,
      date,
      defaultDate,
      description,
      source,
      destinations,
    )
  ) {
    return null;
  }
  return {
    type: CreateTransactionModelCreateSpendingTransactionModelType.Spending,
    accountingPeriodId: accountingPeriod?.id ?? "",
    date: date?.format("YYYY-MM-DD") ?? defaultDate?.format("YYYY-MM-DD") ?? "",
    description,
    amount: source.amount ?? 0,
    source: {
      accountId: source.account?.id ?? "",
    },
    destinations: destinations.map((destination) => ({
      accountId: destination.account?.id ?? null,
      location:
        destination.account === null
          ? (destination.location?.trim() ?? null)
          : null,
      amount: destination.amount ?? 0,
      fundAssignments: destination.fundAssignments
        .filter((fundAmount) => fundAmount.fundName !== "Unassigned")
        .map((fundAmount) => ({
          fundId: fundAmount.fundId,
          amount: fundAmount.amount,
        })),
    })),
  };
};

/**
 * Builds the update transaction request object from the provided parameters.
 */
const buildUpdateRequest = function (
  accountingPeriod: AccountingPeriod | null,
  date: Dayjs | null,
  description: string,
  source: SpendingSourceDraft,
  destinations: SpendingDestinationDraft[],
): UpdateTransactionRequest | null {
  if (
    !validateRequest(
      accountingPeriod,
      date,
      null,
      description,
      source,
      destinations,
    )
  ) {
    return null;
  }
  return {
    type: UpdateTransactionModelUpdateSpendingTransactionModelType.Spending,
    date: date?.format("YYYY-MM-DD") ?? "",
    description,
    amount: source.amount ?? 0,
    source: {
      accountId: source.account?.id ?? "",
    },
    destinations: destinations.map((destination) => ({
      accountId: destination.account?.id ?? null,
      location:
        destination.account === null
          ? (destination.location?.trim() ?? null)
          : null,
      amount: destination.amount ?? 0,
      fundAssignments: destination.fundAssignments
        .filter((fundAmount) => fundAmount.fundName !== "Unassigned")
        .map((fundAmount) => ({
          fundId: fundAmount.fundId,
          amount: fundAmount.amount,
        })),
    })),
  };
};

/**
 * Builds a filter callback for the source account dropdown.
 */
const buildSourceAccountFilter = function (
  accounts: Account[],
  destinations: SpendingDestinationDraft[],
) {
  return function (account: AccountIdentifier): boolean {
    const selectedAccount =
      accounts.find((candidate) => candidate.id === account.id) ?? null;
    return (
      selectedAccount !== null &&
      !destinations.some(
        (destination) => destination.account?.id === account.id,
      ) &&
      isTrackedAccountType(selectedAccount.type)
    );
  };
};

/**
 * Builds a filter callback for the destination account dropdown.
 */
const buildDestinationAccountFilter = function (
  accounts: Account[],
  destinations: SpendingDestinationDraft[],
  index: number,
  sourceAccount: Account | null,
) {
  return function (account: AccountIdentifier): boolean {
    const selectedAccount =
      accounts.find((candidate) => candidate.id === account.id) ?? null;
    const accountUsedElsewhere = destinations.some(
      (currentDestination, currentIndex) =>
        currentIndex !== index && currentDestination.account?.id === account.id,
    );
    if (selectedAccount === null || accountUsedElsewhere) {
      return false;
    }
    if (account.id === sourceAccount?.id) {
      return false;
    }
    return !isTrackedAccountType(selectedAccount.type);
  };
};

/**
 * Gets the source from the provided spending transaction.
 */
const getSourceFromTransaction = function (
  transaction: SpendingTransaction,
  accounts: Account[],
): SpendingSourceDraft {
  return {
    account:
      accounts.find(
        (account) => account.id === transaction.source.account.accountId,
      ) ?? null,
    amount: transaction.amount,
  };
};

/**
 * Gets the collection of destinations from the provided spending transaction.
 */
const getDestinationsFromTransaction = function (
  transaction: SpendingTransaction,
  accounts: Account[],
): SpendingDestinationDraft[] {
  return transaction.destinations.map(
    (destination: SpendingTransactionDestination) => ({
      account:
        accounts.find(
          (account) => account.id === destination.account?.accountId,
        ) ?? null,
      location: destination.location ?? null,
      amount: destination.amount,
      fundAssignments: destination.fundAssignments.map((assignment) => ({
        fundId: assignment.fundId,
        fundName: assignment.fundName,
        amount: assignment.amount,
      })),
      baselineFundAssignments: destination.fundAssignments.map(
        (assignment) => ({
          fundId: assignment.fundId,
          fundName: assignment.fundName,
          amount: assignment.amount,
        }),
      ),
    }),
  );
};

export type { SpendingSourceDraft, SpendingDestinationDraft };
export {
  CreateTransactionModelCreateSpendingTransactionModelType as CreateSpendingTransactionType,
  UpdateTransactionModelUpdateSpendingTransactionModelType as UpdateSpendingTransactionType,
  buildCreateRequest,
  buildDestinationAccountFilter,
  buildSourceAccountFilter,
  buildUpdateRequest,
  createEmptySource,
  createEmptyDestination,
  getDestinationsFromTransaction,
  getSourceFromTransaction,
};
