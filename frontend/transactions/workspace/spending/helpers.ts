import type { Account, AccountBalanceEventDraft } from "@/accounts/types";
import {
  CreateTransactionModelCreateSpendingTransactionModelType,
  UpdateTransactionModelUpdateSpendingTransactionModelType,
  type components,
} from "@/framework/data/api";
import type {
  CreateTransactionRequest,
  SpendingTransaction,
  SpendingTransactionDestination,
  UpdateTransactionRequest,
} from "@/transactions/types";
import {
  type FundAssignmentDraft,
  getSpendingGoalRemainingAmount,
} from "@/funds/assignmentPlanner/helpers";
import {
  type LocationDraft,
  toLocationDraft,
  toLocationInput,
} from "@/locations/types";
import {
  compareCurrencyAmounts,
  getCurrencyDifference,
  getCurrencyTotal,
} from "@/framework/currencyHelpers";
import {
  hasIncompleteFundAssignments,
  isUnassignedFund,
} from "@/funds/helpers";
import {
  validateDetails,
  validateSummary,
} from "@/transactions/workspace/helpers";
import type { AccountingPeriod } from "@/accounting-periods/types";
import type { Dayjs } from "dayjs";
import type { FundBalanceEvent } from "@/funds/types";
import type { FundGoalWithProgress } from "@/fund-goals/types";
import { getTransactionAccountDraftFromTransactionAccount } from "@/transactions/workspace/accountBalanceEventDraft";
import { isTrackedAccountType } from "@/accounts/helpers";

/**
 * Interface representing a potentially unfinished spending transaction source.
 */
interface SpendingSourceDraft {
  readonly account: AccountBalanceEventDraft | null;
  readonly amount: number | null;
}

/**
 * Interface representing a potentially unfinished spending transaction destination.
 */
interface SpendingDestinationDraft {
  readonly account: AccountBalanceEventDraft | null;
  readonly location: LocationDraft | null;
  readonly amount: number | null;
  readonly fundAssignments: FundAssignmentDraft[];
  readonly baselineFundAssignments: FundAssignmentDraft[];
}

/**
 * Interface representing a potentially unfinished spending transaction request.
 */
type SpendingRequestFields = Omit<
  components["schemas"]["UpdateTransactionModelUpdateSpendingTransactionModel"],
  "type"
>;

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
  if (
    source.account.accountType === null ||
    !isTrackedAccountType(source.account.accountType)
  ) {
    return false;
  }
  return true;
};

/**
 * Validates the fund assignments for a spending destination.
 */
const validateFundAssignments = function (
  destination: SpendingDestinationDraft,
): boolean {
  const explicitFundAssignments = destination.fundAssignments.filter(
    (assignment) => !isUnassignedFund(assignment.fundName),
  );
  const assignedAmount =
    explicitFundAssignments.length === 1
      ? (destination.amount ?? 0)
      : getCurrencyTotal(
          explicitFundAssignments.map((assignment) => assignment.amount),
        );

  return (
    !hasIncompleteFundAssignments(destination.fundAssignments) &&
    getCurrencyDifference(assignedAmount, destination.amount ?? 0) === 0
  );
};

/**
 * Validates the destination of a spending transaction.
 */
const validateDestination = function (
  destination: SpendingDestinationDraft,
  sourceAccount: AccountBalanceEventDraft | null,
): boolean {
  const normalizedLocation = destination.location?.name.trim() ?? "";
  const hasAccount = destination.account !== null;
  const hasLocation = normalizedLocation !== "";
  if (
    destination.amount === null ||
    compareCurrencyAmounts(destination.amount, 0) <= 0
  ) {
    return false;
  }
  if ((hasAccount && hasLocation) || (!hasAccount && !hasLocation)) {
    return false;
  }
  if (
    sourceAccount !== null &&
    destination.account?.accountId === sourceAccount.accountId
  ) {
    return false;
  }
  if (
    hasAccount &&
    destination.account.accountType !== null &&
    isTrackedAccountType(destination.account.accountType)
  ) {
    return false;
  }
  if (!validateFundAssignments(destination)) {
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
  const destinationTotal = getCurrencyTotal(
    destinations.map((destination) => destination.amount),
  );
  return (
    validateDetails(accountingPeriod, date, defaultDate, description) &&
    validateSource(source) &&
    destinations.every((destination) =>
      validateDestination(destination, source.account),
    ) &&
    validateSummary(destinationTotal, destinationTotal, destinations.length)
  );
};

/** Maps a validated spending draft to fields shared by create and update requests. */
const buildRequestFields = function (
  date: Dayjs | null,
  defaultDate: Dayjs | null,
  description: string,
  source: SpendingSourceDraft,
  destinations: SpendingDestinationDraft[],
): SpendingRequestFields {
  const destinationTotal = getCurrencyTotal(
    destinations.map((destination) => destination.amount),
  );
  return {
    date: date?.format("YYYY-MM-DD") ?? defaultDate?.format("YYYY-MM-DD") ?? "",
    description,
    amount: destinationTotal,
    source: {
      accountId: source.account?.accountId ?? "",
    },
    destinations: destinations.map((destination) => ({
      accountId: destination.account?.accountId ?? null,
      location:
        destination.account === null
          ? toLocationInput(destination.location)
          : null,
      amount: destination.amount ?? 0,
      fundAssignments: destination.fundAssignments
        .filter((fundAmount) => !isUnassignedFund(fundAmount.fundName))
        .map((fundAmount, _, assignments) => ({
          fundId: fundAmount.fundId,
          amount:
            assignments.length === 1
              ? (destination.amount ?? 0)
              : fundAmount.amount,
        })),
    })),
  };
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
    ...buildRequestFields(date, defaultDate, description, source, destinations),
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
    ...buildRequestFields(date, null, description, source, destinations),
  };
};

/**
 * Builds a filter callback for the source account dropdown.
 */
const buildSourceAccountFilter = function (
  accounts: Account[],
  destinations: SpendingDestinationDraft[],
) {
  return function (account: Account): boolean {
    const selectedAccount =
      accounts.find((candidate) => candidate.id === account.id) ?? null;
    return (
      selectedAccount !== null &&
      !destinations.some(
        (destination) => destination.account?.accountId === account.id,
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
  sourceAccount: AccountBalanceEventDraft | null,
) {
  return function (account: Account): boolean {
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
    return !isTrackedAccountType(selectedAccount.type);
  };
};

/**
 * Gets the source from the provided spending transaction.
 */
const getSourceFromTransaction = function (
  transaction: SpendingTransaction,
): SpendingSourceDraft {
  return {
    account: getTransactionAccountDraftFromTransactionAccount(
      transaction.source.account,
    ),
    amount: transaction.amount,
  };
};

/**
 * Gets a fund assignment draft from the provided transaction fund.
 */
const getFundAssignmentFromTransactionFund = (
  assignment: FundBalanceEvent,
  fundGoals: FundGoalWithProgress[],
): FundAssignmentDraft => {
  const previousFundBalance = assignment.previousBalance.postedBalance;
  const newFundBalance = assignment.newBalance.postedBalance;

  return {
    fundId: assignment.fund.id,
    fundName: assignment.fund.name,
    amount: assignment.amount,
    isExtraContribution: false,
    previousFundBalance,
    newFundBalance,
    previousGoalAmount: getSpendingGoalRemainingAmount(
      assignment.fund.id,
      fundGoals,
      previousFundBalance,
    ),
    newGoalAmount: getSpendingGoalRemainingAmount(
      assignment.fund.id,
      fundGoals,
      newFundBalance,
    ),
  };
};

/**
 * Gets the collection of destinations from the provided spending transaction.
 */
const getDestinationsFromTransaction = function (
  transaction: SpendingTransaction,
  fundGoals: FundGoalWithProgress[],
): SpendingDestinationDraft[] {
  return transaction.destinations.map(
    (destination: SpendingTransactionDestination) => ({
      account: getTransactionAccountDraftFromTransactionAccount(
        destination.account,
      ),
      location: toLocationDraft(destination.location ?? null),
      amount: destination.amount,
      fundAssignments: destination.fundAssignments.map((assignment) =>
        getFundAssignmentFromTransactionFund(assignment, fundGoals),
      ),
      baselineFundAssignments: destination.fundAssignments.map((assignment) =>
        getFundAssignmentFromTransactionFund(assignment, fundGoals),
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
  getFundAssignmentFromTransactionFund,
  getSourceFromTransaction,
  validateFundAssignments,
  validateDestination,
  validateSource,
};
