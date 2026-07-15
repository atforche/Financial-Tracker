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
  TransactionAccountDraft,
  TransactionFund,
  TransactionGoal,
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
import type { FundAssignmentDraft } from "@/funds/assignmentPlanner/helpers";
import { getTransactionAccountDraftFromTransactionAccount } from "@/transactions/workspace/transactionAccountDraft";
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
  readonly account: TransactionAccountDraft | null;
  readonly location: string | null;
  readonly incomeLines: IncomeLineDraft[];
  readonly incomeDeductions: IncomeDeductionDraft[];
}

/**
 * Interface representing a potentially unfinished income transaction destination.
 */
interface IncomeDestinationDraft {
  readonly account: TransactionAccountDraft | null;
  readonly amount: number | null;
  readonly fundAssignments: FundAssignmentDraft[];
  readonly baselineFundAssignments: FundAssignmentDraft[];
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
      accountId: source.account?.accountId ?? null,
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
      accountId: destination.account?.accountId ?? "",
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
      accountId: source.account?.accountId ?? null,
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
      accountId: destination.account?.accountId ?? "",
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
        (destination) => destination.account?.accountId === account.id,
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
    return true;
  };
};

/**
 * Gets the source from the provided income transaction.
 */
const getSourceFromTransaction = function (
  transaction: IncomeTransaction,
): IncomeSourceDraft {
  return {
    account: getTransactionAccountDraftFromTransactionAccount(
      transaction.source.account,
    ),
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
 * Gets a fund assignment draft from the provided transaction fund.
 */
const getFundAssignmentFromTransactionFund = (
  assignment: TransactionFund,
  goal: TransactionGoal | null = null,
): FundAssignmentDraft => ({
  fundId: assignment.fund.id,
  fundName: assignment.fund.name,
  amount: assignment.amount,
  previousFundBalance: assignment.previousBalance.postedBalance,
  newFundBalance: assignment.newBalance.postedBalance,
  previousGoalBalance: {
    remainingAmountToAssignIncludingPending:
      (goal?.previousBalance.amountAssigned ?? 0) +
      (goal?.previousBalance.pendingAmountAssigned ?? 0),
    remainingAmountToSpendIncludingPending:
      (goal?.previousBalance.amountAssigned ?? 0) +
      (goal?.previousBalance.pendingAmountAssigned ?? 0) -
      (goal?.previousBalance.amountSpent ?? 0) -
      (goal?.previousBalance.pendingAmountSpent ?? 0),
  },
  newGoalBalance: {
    remainingAmountToAssignIncludingPending:
      (goal?.newBalance.amountAssigned ?? 0) +
      (goal?.newBalance.pendingAmountAssigned ?? 0),
    remainingAmountToSpendIncludingPending:
      (goal?.newBalance.amountAssigned ?? 0) +
      (goal?.newBalance.pendingAmountAssigned ?? 0) -
      (goal?.newBalance.amountSpent ?? 0) -
      (goal?.newBalance.pendingAmountSpent ?? 0),
  },
});

/**
 * Gets the collection of destinations from the provided income transaction.
 */
const getDestinationsFromTransaction = function (
  transaction: IncomeTransaction,
): IncomeDestinationDraft[] {
  return transaction.destinations.map(
    (destination: IncomeTransactionDestination) => ({
      account: getTransactionAccountDraftFromTransactionAccount(
        destination.account,
      ),
      amount: destination.amount,
      fundAssignments: destination.fundAssignments.map((assignment) =>
        getFundAssignmentFromTransactionFund(
          assignment,
          destination.goals.find(
            (goal) => goal.fund.id === assignment.fund.id,
          ) ?? null,
        ),
      ),
      baselineFundAssignments: destination.fundAssignments.map((assignment) =>
        getFundAssignmentFromTransactionFund(
          assignment,
          destination.goals.find(
            (goal) => goal.fund.id === assignment.fund.id,
          ) ?? null,
        ),
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
  getFundAssignmentFromTransactionFund,
  getNetIncomeAmount,
  getSourceFromTransaction,
  validateFundAssignments,
  validateDestination,
  validateSource,
};
