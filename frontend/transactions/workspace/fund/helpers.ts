import {
  CreateTransactionModelCreateFundTransactionModelType,
  UpdateTransactionModelUpdateFundTransactionModelType,
} from "@/framework/data/api";
import type {
  CreateTransactionRequest,
  FundTransaction,
  UpdateTransactionRequest,
} from "@/transactions/types";
import type { Fund, FundBalanceEventDraft } from "@/funds/types";
import {
  compareCurrencyAmounts,
  getCurrencyTotal,
} from "@/framework/currencyHelpers";
import {
  validateDetails,
  validateSummary,
} from "@/transactions/workspace/helpers";
import type { AccountingPeriod } from "@/accounting-periods/types";
import type { Dayjs } from "dayjs";
import { getTransactionFundDraftFromTransactionFund } from "@/transactions/workspace/fundBalanceEventDraft";
import { isNotNullOrUndefined } from "@/framework/nullHelpers";

/**
 * Interface representing a potentially unfinished fund transaction source.
 */
interface FundSourceDraft {
  readonly fund: FundBalanceEventDraft | null;
  readonly amount: number | null;
}

/**
 * Interface representing a potentially unfinished fund transaction destination.
 */
interface FundDestinationDraft {
  readonly fund: FundBalanceEventDraft | null;
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
  return source.fund !== null;
};

/**
 * Validates the destination of a fund transaction.
 */
const validateDestination = function (
  destination: FundDestinationDraft,
  sourceFund: FundBalanceEventDraft | null,
): boolean {
  return (
    destination.fund !== null &&
    destination.amount !== null &&
    compareCurrencyAmounts(destination.amount, 0) > 0 &&
    destination.fund.fundId !== sourceFund?.fundId
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
  const destinationTotal = getCurrencyTotal(
    destinations.map((destination) => destination.amount),
  );
  const destinationFundIds = destinations
    .map((destination) => destination.fund?.fundId ?? null)
    .filter(isNotNullOrUndefined);
  const hasUniqueDestinationFunds =
    new Set(destinationFundIds).size === destinationFundIds.length;
  const areDestinationsComplete = destinations.every((destination) =>
    validateDestination(destination, source.fund),
  );

  return (
    validateDetails(accountingPeriod, date, defaultDate, description) &&
    validateSource(source) &&
    validateSummary(destinationTotal, destinationTotal, destinations.length) &&
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
  const destinationTotal = getCurrencyTotal(
    destinations.map((destination) => destination.amount),
  );
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
      amount: destinationTotal,
      source: {
        fundId: source.fund?.fundId ?? "",
      },
      destinations: destinations.map((destination) => ({
        fundId: destination.fund?.fundId ?? "",
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
  const destinationTotal = getCurrencyTotal(
    destinations.map((destination) => destination.amount),
  );
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
      amount: destinationTotal,
      source: {
        fundId: source.fund?.fundId ?? "",
      },
      destinations: destinations.map((destination) => ({
        fundId: destination.fund?.fundId ?? "",
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
): (fund: Fund) => boolean {
  return function (fund: Fund): boolean {
    return !destinations.some(
      (destination) => destination.fund?.fundId === fund.id,
    );
  };
};

/**
 * Builds a filter callback for a destination fund dropdown.
 */
const buildDestinationFundFilter = function (
  destinations: FundDestinationDraft[],
  index: number,
  sourceFund: FundBalanceEventDraft | null,
): (fund: Fund) => boolean {
  return function (fund: Fund): boolean {
    const fundUsedElsewhere = destinations.some(
      (currentDestination, currentIndex) =>
        currentIndex !== index && currentDestination.fund?.fundId === fund.id,
    );
    if (fundUsedElsewhere) {
      return false;
    }
    return fund.id !== sourceFund?.fundId;
  };
};

/**
 * Gets the source from the provided fund transaction.
 */
const getSourceFromTransaction = function (
  transaction: FundTransaction,
): FundSourceDraft {
  return {
    fund: getTransactionFundDraftFromTransactionFund(transaction.source.fund),
    amount: transaction.amount,
  };
};

/**
 * Gets the collection of destinations from the provided fund transaction.
 */
const getDestinationsFromTransaction = function (
  transaction: FundTransaction,
): FundDestinationDraft[] {
  return transaction.destinations.map((destination) => ({
    fund: getTransactionFundDraftFromTransactionFund(destination.fund),
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
