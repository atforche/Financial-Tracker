import {
  type Account,
  type AccountIdentifier,
  isTrackedAccountType,
} from "@/accounts/types";
import {
  CreateTransactionModelCreateIncomeTransactionModelType,
  UpdateTransactionModelUpdateIncomeTransactionModelType,
} from "@/framework/data/api";
import type {
  CreateTransactionRequest,
  UpdateTransactionRequest,
} from "@/transactions/transaction";
import type {
  IncomeTransaction,
  IncomeTransactionDestination,
} from "@/transactions/incomeTransaction";
import {
  validateDetails,
  validateSummary,
} from "@/transactions/workspace/helpers";
import type { AccountingPeriod } from "@/accounting-periods/types";
import type { Dayjs } from "dayjs";
import type { FundAmount } from "@/funds/types";
import { hasIncompleteFundAssignments } from "@/funds/helpers";

/**
 * Interface representing a potentially unfinished income line item.
 */
interface IncomeLineDraft {
  readonly description: string | null;
  readonly amount: number | null;
}

/**
 * Interface representing a potentially unfinished income deduction item.
 */
interface IncomeDeductionDraft {
  readonly description: string | null;
  readonly amount: number | null;
}

/**
 * Interface representing a potentially unfinished income transaction source.
 */
interface IncomeSourceDraft {
  readonly account: Account | null;
  readonly location: string | null;
  readonly incomeLines: IncomeLineDraft[];
  readonly incomeDeductions: IncomeDeductionDraft[];
}

/**
 * Interface representing a potentially unfinished income transaction destination.
 */
interface IncomeDestinationDraft {
  readonly account: Account | null;
  readonly amount: number | null;
  readonly fundAssignments: FundAmount[];
  readonly baselineFundAssignments: FundAmount[];
}

/**
 * Creates an empty income line.
 */
const createEmptyLine = function (): IncomeLineDraft {
  return {
    description: null,
    amount: null,
  };
};

/**
 * Creates an empty income deduction.
 */
const createEmptyDeduction = function (): IncomeDeductionDraft {
  return {
    description: null,
    amount: null,
  };
};

/**
 * Creates an empty source draft.
 */
const createEmptySource = function (): IncomeSourceDraft {
  return {
    account: null,
    location: null,
    incomeLines: [],
    incomeDeductions: [],
  };
};

/**
 * Creates an empty destination draft.
 */
const createEmptyDestination = function (): IncomeDestinationDraft {
  return {
    account: null,
    amount: null,
    fundAssignments: [],
    baselineFundAssignments: [],
  };
};

/**
 * Gets the net income amount for an income source.
 */
const getNetIncomeAmount = function (source: IncomeSourceDraft): number {
  const incomeLineTotal = source.incomeLines.reduce(
    (total, line) => total + (line.amount ?? 0),
    0,
  );
  const incomeDeductionTotal = source.incomeDeductions.reduce(
    (total, deduction) => total + (deduction.amount ?? 0),
    0,
  );
  return incomeLineTotal - incomeDeductionTotal;
};

/**
 * Validates an income line.
 */
const validateIncomeLine = function (incomeLine: IncomeLineDraft): boolean {
  return (
    incomeLine.description !== null &&
    incomeLine.description.trim() !== "" &&
    incomeLine.amount !== null &&
    incomeLine.amount > 0
  );
};

/**
 * Validates an income deduction.
 */
const validateIncomeDeduction = function (
  incomeDeduction: IncomeDeductionDraft,
): boolean {
  return (
    incomeDeduction.description !== null &&
    incomeDeduction.description.trim() !== "" &&
    incomeDeduction.amount !== null &&
    incomeDeduction.amount > 0
  );
};

/**
 * Validates the source of an income transaction.
 */
const validateSource = function (source: IncomeSourceDraft): boolean {
  const hasAccount = source.account !== null;
  const hasLocation = source.location !== null && source.location.trim() !== "";
  if (!hasAccount && !hasLocation) {
    return false;
  }
  const hasValidIncomeLines =
    source.incomeLines.length > 0 &&
    source.incomeLines.every(validateIncomeLine);
  const hasValidIncomeDeductions = source.incomeDeductions.every(
    validateIncomeDeduction,
  );
  return (
    hasValidIncomeLines &&
    hasValidIncomeDeductions &&
    getNetIncomeAmount(source) >= 0
  );
};

/**
 * Validates the fund assignments for an income destination.
 */
const validateFundAssignments = function (
  destination: IncomeDestinationDraft,
): boolean {
  return (
    !hasIncompleteFundAssignments(destination.fundAssignments) &&
    destination.amount !== null &&
    destination.fundAssignments.reduce(
      (total, assignment) => total + assignment.amount,
      0,
    ) <= destination.amount
  );
};

/**
 * Validates the destination of an income transaction.
 */
const validateDestination = function (
  destination: IncomeDestinationDraft,
): boolean {
  return (
    destination.account !== null &&
    destination.amount !== null &&
    destination.amount > 0 &&
    validateFundAssignments(destination)
  );
};

/**
 * Validates the entire income transaction request.
 */
const validateRequest = function (
  accountingPeriod: AccountingPeriod | null,
  date: Dayjs | null,
  defaultDate: Dayjs | null,
  description: string,
  source: IncomeSourceDraft,
  destinations: IncomeDestinationDraft[],
): boolean {
  const destinationTotal = destinations.reduce(
    (total, destination) => total + (destination.amount ?? 0),
    0,
  );
  return (
    validateDetails(accountingPeriod, date, defaultDate, description) &&
    validateSource(source) &&
    destinations.every(validateDestination) &&
    validateSummary(
      getNetIncomeAmount(source),
      destinationTotal,
      destinations.length,
    )
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
  source: IncomeSourceDraft,
  destinations: IncomeDestinationDraft[],
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
    type: CreateTransactionModelCreateIncomeTransactionModelType.Income,
    accountingPeriodId: accountingPeriod?.id ?? "",
    date: date?.format("YYYY-MM-DD") ?? defaultDate?.format("YYYY-MM-DD") ?? "",
    description,
    amount: getNetIncomeAmount(source),
    source: {
      accountId: source.account?.id ?? null,
      location:
        source.account === null ? (source.location?.trim() ?? null) : null,
      incomeLines: source.incomeLines.map((line) => ({
        description: line.description?.trim() ?? "",
        amount: line.amount ?? 0,
      })),
      incomeDeductions: source.incomeDeductions.map((deduction) => ({
        description: deduction.description?.trim() ?? "",
        amount: deduction.amount ?? 0,
      })),
    },
    destinations: destinations.map((destination) => ({
      accountId: destination.account?.id ?? "",
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
 * Builds an update transaction request from the provided parameters.
 */
const buildUpdateRequest = function (
  accountingPeriod: AccountingPeriod | null,
  date: Dayjs | null,
  defaultDate: Dayjs | null,
  description: string,
  source: IncomeSourceDraft,
  destinations: IncomeDestinationDraft[],
): UpdateTransactionRequest | null {
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
    type: UpdateTransactionModelUpdateIncomeTransactionModelType.Income,
    date: date?.format("YYYY-MM-DD") ?? defaultDate?.format("YYYY-MM-DD") ?? "",
    description,
    amount: getNetIncomeAmount(source),
    source: {
      accountId: source.account?.id ?? null,
      location:
        source.account === null ? (source.location?.trim() ?? null) : null,
      incomeLines: source.incomeLines.map((line) => ({
        description: line.description?.trim() ?? "",
        amount: line.amount ?? 0,
      })),
      incomeDeductions: source.incomeDeductions.map((deduction) => ({
        description: deduction.description?.trim() ?? "",
        amount: deduction.amount ?? 0,
      })),
    },
    destinations: destinations.map((destination) => ({
      accountId: destination.account?.id ?? "",
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
  destinations: IncomeDestinationDraft[],
) {
  return function (account: AccountIdentifier): boolean {
    const selectedAccount =
      accounts.find((candidate) => candidate.id === account.id) ?? null;
    return (
      selectedAccount !== null &&
      !destinations.some(
        (destination) => destination.account?.id === account.id,
      ) &&
      !isTrackedAccountType(selectedAccount.type)
    );
  };
};

/**
 * Builds a filter callback for the destination account dropdown.
 */
const buildDestinationAccountFilter = function (
  accounts: Account[],
  destinations: IncomeDestinationDraft[],
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
    return true;
  };
};

/**
 * Gets the source from the provided income transaction.
 */
const getSourceFromTransaction = function (
  transaction: IncomeTransaction,
  accounts: Account[],
): IncomeSourceDraft {
  return {
    account:
      typeof transaction.source.account !== "undefined" &&
      transaction.source.account !== null
        ? (accounts.find(
            (account) => account.id === transaction.source.account?.accountId,
          ) ?? null)
        : null,
    location: transaction.source.location ?? "",
    incomeLines: transaction.source.incomeLines.map((line) => ({
      description: line.description,
      amount: line.amount,
    })),
    incomeDeductions: transaction.source.incomeDeductions.map((deduction) => ({
      description: deduction.description,
      amount: deduction.amount,
    })),
  };
};

/**
 * Gets the collection of destinations from the provided income transaction.
 */
const getDestinationsFromTransaction = function (
  transaction: IncomeTransaction,
  accounts: Account[],
): IncomeDestinationDraft[] {
  return transaction.destinations.map(
    (destination: IncomeTransactionDestination) => ({
      account:
        accounts.find(
          (account) => account.id === destination.account.accountId,
        ) ?? null,
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

export type {
  IncomeLineDraft,
  IncomeDeductionDraft,
  IncomeSourceDraft,
  IncomeDestinationDraft,
};
export {
  CreateTransactionModelCreateIncomeTransactionModelType as CreateIncomeTransactionType,
  UpdateTransactionModelUpdateIncomeTransactionModelType as UpdateIncomeTransactionType,
  buildCreateRequest,
  buildDestinationAccountFilter,
  buildSourceAccountFilter,
  buildUpdateRequest,
  createEmptyLine,
  createEmptyDeduction,
  createEmptySource,
  createEmptyDestination,
  getDestinationsFromTransaction,
  getNetIncomeAmount,
  getSourceFromTransaction,
  validateFundAssignments,
  validateDestination,
  validateSource,
};
