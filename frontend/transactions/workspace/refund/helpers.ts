import type { Account, AccountBalanceEventDraft } from "@/accounts/types";
import {
  CreateTransactionModelCreateRefundTransactionModelType,
  UpdateTransactionModelUpdateRefundTransactionModelType,
  type components,
} from "@/framework/data/api";
import type {
  CreateTransactionRequest,
  RefundTransaction,
  RefundTransactionSource,
  UpdateTransactionRequest,
} from "@/transactions/types";
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
import type { FundAssignmentDraft } from "@/funds/assignmentPlanner/helpers";
import type { FundBalanceEvent } from "@/funds/types";
import { getTransactionAccountDraftFromTransactionAccount } from "@/transactions/workspace/accountBalanceEventDraft";
import { isTrackedAccountType } from "@/accounts/helpers";

/**
 * Interface representing a potentially unfinished refund transaction source.
 */
interface RefundSourceDraft {
  readonly account: AccountBalanceEventDraft | null;
  readonly location: LocationDraft | null;
  readonly amount: number | null;
  readonly fundAssignments: FundAssignmentDraft[];
}

/**
 * Interface representing a potentially unfinished refund transaction destination.
 */
interface RefundDestinationDraft {
  readonly account: AccountBalanceEventDraft | null;
}

/**
 * Interface representing a potentially unfinished refund transaction request.
 */
type RefundRequestFields = Omit<
  components["schemas"]["UpdateTransactionModelUpdateRefundTransactionModel"],
  "type"
>;

/**
 * Creates an empty source draft.
 */
const createEmptySource = function (): RefundSourceDraft {
  return {
    account: null,
    location: null,
    amount: null,
    fundAssignments: [],
  };
};

/**
 * Creates an empty destination draft.
 */
const createEmptyDestination = function (): RefundDestinationDraft {
  return {
    account: null,
  };
};

/**
 * Validates the fund assignments for a refund source.
 */
const validateFundAssignments = function (source: RefundSourceDraft): boolean {
  return (
    !hasIncompleteFundAssignments(source.fundAssignments) &&
    getCurrencyDifference(
      getCurrencyTotal(
        source.fundAssignments
          .filter((item) => !isUnassignedFund(item.fundName))
          .map((item) => item.amount),
      ),
      source.amount ?? 0,
    ) === 0
  );
};

/**
 * Validates the source of a refund transaction.
 */
const validateSource = function (source: RefundSourceDraft): boolean {
  const hasAccount = source.account !== null;
  const hasLocation = (source.location?.name.trim() ?? "") !== "";
  return (
    hasAccount !== hasLocation &&
    (!hasAccount ||
      (source.account.accountType !== null &&
        !isTrackedAccountType(source.account.accountType))) &&
    source.amount !== null &&
    compareCurrencyAmounts(source.amount, 0) > 0 &&
    validateFundAssignments(source)
  );
};

/**
 * Validates the destination of a refund transaction.
 */
const validateDestination = function (
  destination: RefundDestinationDraft,
  sources: RefundSourceDraft[],
): boolean {
  return (
    destination.account !== null &&
    (destination.account.accountType === null ||
      isTrackedAccountType(destination.account.accountType)) &&
    !sources.some(
      (source) => source.account?.accountId === destination.account?.accountId,
    )
  );
};

/**
 * Validates the entire refund transaction request.
 */
const validateRequest = function (
  period: AccountingPeriod | null,
  date: Dayjs | null,
  fallback: Dayjs | null,
  description: string,
  sources: RefundSourceDraft[],
  destination: RefundDestinationDraft,
): boolean {
  const total = getCurrencyTotal(sources.map((source) => source.amount));
  return (
    validateDetails(period, date, fallback, description) &&
    sources.every(validateSource) &&
    validateDestination(destination, sources) &&
    validateSummary(total, total, sources.length)
  );
};

/**
 * Maps a validated refund draft to fields shared by create and update requests.
 */
const buildRequestFields = function (
  date: Dayjs | null,
  fallback: Dayjs | null,
  description: string,
  sources: RefundSourceDraft[],
  destination: RefundDestinationDraft,
): RefundRequestFields {
  return {
    date: date?.format("YYYY-MM-DD") ?? fallback?.format("YYYY-MM-DD") ?? "",
    description,
    amount: getCurrencyTotal(sources.map((source) => source.amount)),
    sources: sources.map((source) => ({
      accountId: source.account?.accountId ?? null,
      location:
        source.account === null ? toLocationInput(source.location) : null,
      amount: source.amount ?? 0,
      fundAssignments: source.fundAssignments
        .filter((item) => !isUnassignedFund(item.fundName))
        .map((item) => ({ fundId: item.fundId, amount: item.amount })),
    })),
    destination: { accountId: destination.account?.accountId ?? "" },
  };
};

/**
 * Builds the create transaction request object from the provided parameters.
 */
const buildCreateRequest = function (
  period: AccountingPeriod | null,
  date: Dayjs | null,
  fallback: Dayjs | null,
  description: string,
  sources: RefundSourceDraft[],
  destination: RefundDestinationDraft,
): CreateTransactionRequest | null {
  return validateRequest(
    period,
    date,
    fallback,
    description,
    sources,
    destination,
  )
    ? {
        type: CreateTransactionModelCreateRefundTransactionModelType.Refund,
        accountingPeriodId: period?.id ?? "",
        ...buildRequestFields(
          date,
          fallback,
          description,
          sources,
          destination,
        ),
      }
    : null;
};

/**
 * Builds the update transaction request object from the provided parameters.
 */
const buildUpdateRequest = function (
  period: AccountingPeriod | null,
  date: Dayjs | null,
  description: string,
  sources: RefundSourceDraft[],
  destination: RefundDestinationDraft,
): UpdateTransactionRequest | null {
  return validateRequest(period, date, null, description, sources, destination)
    ? {
        type: UpdateTransactionModelUpdateRefundTransactionModelType.Refund,
        ...buildRequestFields(date, null, description, sources, destination),
      }
    : null;
};

/**
 * Builds a filter callback for the source account dropdown.
 */
const buildSourceAccountFilter = function (
  accounts: Account[],
  sources: RefundSourceDraft[],
  index: number,
  destination: RefundDestinationDraft,
) {
  return (account: Account): boolean =>
    !isTrackedAccountType(
      accounts.find((item) => item.id === account.id)?.type ?? account.type,
    ) &&
    destination.account?.accountId !== account.id &&
    !sources.some(
      (source, i) => i !== index && source.account?.accountId === account.id,
    );
};

/**
 * Builds a filter callback for the destination account dropdown.
 */
const buildDestinationAccountFilter = function (
  accounts: Account[],
  sources: RefundSourceDraft[],
) {
  return (account: Account): boolean =>
    isTrackedAccountType(
      accounts.find((item) => item.id === account.id)?.type ?? account.type,
    ) && !sources.some((source) => source.account?.accountId === account.id);
};

/**
 * Gets a fund assignment draft from the provided transaction fund.
 */
const toFundDraft = function (
  assignment: FundBalanceEvent,
): FundAssignmentDraft {
  return {
    fundId: assignment.fund.id,
    fundName: assignment.fund.name,
    amount: assignment.amount,
    isExtraContribution: false,
  };
};

/**
 * Gets the sources from the provided refund transaction.
 */
const getSourcesFromTransaction = function (
  transaction: RefundTransaction,
): RefundSourceDraft[] {
  return transaction.sources.map((source: RefundTransactionSource) => {
    const assignments = source.fundAssignments.map(toFundDraft);
    return {
      account: getTransactionAccountDraftFromTransactionAccount(source.account),
      location: toLocationDraft(source.location ?? null),
      amount: source.amount,
      fundAssignments: assignments,
    };
  });
};

/**
 * Gets the destination from the provided refund transaction.
 */
const getDestinationFromTransaction = function (
  transaction: RefundTransaction,
): RefundDestinationDraft {
  return {
    account: getTransactionAccountDraftFromTransactionAccount(
      transaction.destination.account,
    ),
  };
};

export type { RefundSourceDraft, RefundDestinationDraft };
export {
  buildCreateRequest,
  buildUpdateRequest,
  buildSourceAccountFilter,
  buildDestinationAccountFilter,
  createEmptySource,
  createEmptyDestination,
  getSourcesFromTransaction,
  getDestinationFromTransaction,
  validateDestination,
  validateSource,
};
