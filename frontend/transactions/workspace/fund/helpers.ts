import {
  CreateTransactionModelCreateFundTransactionModelType,
  UpdateTransactionModelUpdateFundTransactionModelType,
} from "@/framework/data/api";
import type {
  CreateTransactionRequest,
  UpdateTransactionRequest,
} from "@/transactions/transaction";
import type { Fund, FundIdentifier } from "@/funds/types";
import {
  validateDetails,
  validateSummary,
} from "@/transactions/workspace/helpers";
import type { AccountingPeriod } from "@/accounting-periods/types";
import type { Dayjs } from "dayjs";
import type { FundTransaction } from "@/transactions/fundTransaction";

/**
 * Interface representing a potentially unfinished fund transaction source.
 */
interface FundSourceDraft {
  readonly fund: Fund | null;
  readonly amount: number | null;
}

/**
 * Interface representing a potentially unfinished fund transaction destination.
 */
interface FundDestinationDraft {
  readonly fund: Fund | null;
  readonly amount: number | null;
}

/**
 * Creates an empty fund transaction source draft.
 */
const createEmptySource = function (): FundSourceDraft {
  return {
    fund: null,
    amount: null,
  };
};

/**
 * Creates an empty fund transaction destination draft.
 */
const createEmptyDestination = function (): FundDestinationDraft {
  return {
    fund: null,
    amount: null,
  };
};

/**
 * Validates the source of a fund transaction.
 */
const validateSource = function (source: FundSourceDraft): boolean {
  return source.fund !== null && source.amount !== null && source.amount > 0;
};

/**
 * Validates the destination of a fund transaction.
 */
const validateDestination = function (
  destination: FundDestinationDraft,
  sourceFund: Fund | null,
): boolean {
  return (
    destination.fund !== null &&
    destination.amount !== null &&
    destination.amount > 0 &&
    destination.fund.id !== sourceFund?.id
  );
};

/**
 * Validates the entire fund transaction request.
 */
const validateRequest = function (
  accountingPeriod: AccountingPeriod | null,
  date: Dayjs | null,
  defaultDate: Dayjs | null,
  description: string,
  source: FundSourceDraft,
  destinations: FundDestinationDraft[],
): boolean {
  const destinationTotal = destinations.reduce(
    (total, destination) => total + (destination.amount ?? 0),
    0,
  );
  const destinationFundIds = destinations
    .map((destination) => destination.fund?.id ?? null)
    .filter((fundId): fundId is string => fundId !== null);
  const hasUniqueDestinationFunds =
    new Set(destinationFundIds).size === destinationFundIds.length;
  const areDestinationsComplete = destinations.every((destination) =>
    validateDestination(destination, source.fund),
  );

  return (
    validateDetails(accountingPeriod, date, defaultDate, description) &&
    validateSource(source) &&
    validateSummary(source.amount, destinationTotal, destinations.length) &&
    hasUniqueDestinationFunds &&
    areDestinationsComplete
  );
};

/**
 * Builds the create transaction request for a fund transaction.
 */
const buildCreateRequest = function (
  accountingPeriod: AccountingPeriod | null,
  date: Dayjs | null,
  defaultDate: Dayjs | null,
  description: string,
  source: FundSourceDraft,
  destinations: FundDestinationDraft[],
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
      type: CreateTransactionModelCreateFundTransactionModelType.Fund,
      accountingPeriodId: accountingPeriod?.id ?? "",
      date:
        date?.format("YYYY-MM-DD") ?? defaultDate?.format("YYYY-MM-DD") ?? "",
      description,
      amount: source.amount ?? 0,
      source: {
        fundId: source.fund?.id ?? "",
      },
      destinations: destinations.map((destination) => ({
        fundId: destination.fund?.id ?? "",
        amount: destination.amount ?? 0,
      })),
    };
  }
  return null;
};

/**
 * Builds the update transaction request for a fund transaction.
 */
const buildUpdateRequest = function (
  accountingPeriod: AccountingPeriod | null,
  date: Dayjs | null,
  description: string,
  source: FundSourceDraft,
  destinations: FundDestinationDraft[],
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
      type: UpdateTransactionModelUpdateFundTransactionModelType.Fund,
      date: date?.format("YYYY-MM-DD") ?? "",
      description,
      amount: source.amount ?? 0,
      source: {
        fundId: source.fund?.id ?? "",
      },
      destinations: destinations.map((destination) => ({
        fundId: destination.fund?.id ?? "",
        amount: destination.amount ?? 0,
      })),
    };
  }
  return null;
};

/**
 * Builds a filter callback for the source fund dropdown.
 */
const buildSourceFundFilter = function (
  destinations: FundDestinationDraft[],
): (fund: FundIdentifier) => boolean {
  return function (fund: FundIdentifier): boolean {
    return !destinations.some(
      (destination) => destination.fund?.id === fund.id,
    );
  };
};

/**
 * Builds a filter callback for a destination fund dropdown.
 */
const buildDestinationFundFilter = function (
  destinations: FundDestinationDraft[],
  index: number,
  sourceFund: Fund | null,
): (fund: FundIdentifier) => boolean {
  return function (fund: FundIdentifier): boolean {
    const fundUsedElsewhere = destinations.some(
      (currentDestination, currentIndex) =>
        currentIndex !== index && currentDestination.fund?.id === fund.id,
    );
    if (fundUsedElsewhere) {
      return false;
    }
    return fund.id !== sourceFund?.id;
  };
};

/**
 * Gets the source from the provided fund transaction.
 */
const getSourceFromTransaction = function (
  transaction: FundTransaction,
  funds: Fund[],
): FundSourceDraft {
  return {
    fund:
      funds.find((fund) => fund.id === transaction.source.fund.fundId) ?? null,
    amount: transaction.amount,
  };
};

/**
 * Gets the collection of destinations from the provided fund transaction.
 */
const getDestinationsFromTransaction = function (
  transaction: FundTransaction,
  funds: Fund[],
): FundDestinationDraft[] {
  return transaction.destinations.map((destination) => ({
    fund: funds.find((fund) => fund.id === destination.fund.fundId) ?? null,
    amount: destination.fund.amount,
  }));
};

export type { FundSourceDraft, FundDestinationDraft };
export {
  CreateTransactionModelCreateFundTransactionModelType as CreateFundTransactionType,
  UpdateTransactionModelUpdateFundTransactionModelType as UpdateFundTransactionType,
  buildCreateRequest,
  buildDestinationFundFilter,
  buildSourceFundFilter,
  buildUpdateRequest,
  createEmptySource,
  createEmptyDestination,
  getDestinationsFromTransaction,
  getSourceFromTransaction,
  validateSource,
  validateDestination,
};
