import type {
  FundBalanceEventSort,
  FundBalanceSummaryByDate,
  FundBalanceSummaryByPeriod,
  FundWithBalanceRangeSort,
} from "@/funds/types";
import type { TrendRangeMode } from "@/framework/routes/trendRange";
import { formatShortDate } from "@/framework/dateHelpers";

/**
 * Search parameters supported by the fund trends page.
 */
interface FundTrendsSearchParams {
  sort?: FundWithBalanceRangeSort;
  page?: number | string | null;
  balanceEventSort?: FundBalanceEventSort;
  balanceEventPage?: number | string | null;
  mode?: TrendRangeMode;
  fundName?: string | readonly string[];
  startAccountingPeriodId?: string;
  endAccountingPeriodId?: string;
  startDate?: string;
  endDate?: string;
}

/**
 * Mode for the fund trends data.
 */
type FundTrendsDataMode = "AccountingPeriod" | "Date";

/**
 * Snapshot of the fund trends data for the selected range.
 */
interface FundTrendsSnapshot {
  readonly startLabel: string;
  readonly endLabel: string;
  readonly totalStartingBalance: number;
  readonly totalEndingBalance: number;
  readonly assignedStartingBalance: number;
  readonly assignedEndingBalance: number;
  readonly unassignedStartingBalance: number;
  readonly unassignedEndingBalance: number;
}

/**
 * Builds the summary snapshot for the selected fund trends range.
 */
const getFundTrendsSnapshot = function (
  mode: FundTrendsDataMode,
  accountingPeriods: readonly FundBalanceSummaryByPeriod[],
  dates: readonly FundBalanceSummaryByDate[],
): FundTrendsSnapshot {
  if (mode === "AccountingPeriod") {
    const firstPeriod = accountingPeriods.at(0);
    const lastPeriod = accountingPeriods.at(-1);
    if (
      typeof firstPeriod !== "undefined" &&
      typeof lastPeriod !== "undefined"
    ) {
      return {
        startLabel: firstPeriod.accountingPeriod.name,
        endLabel: lastPeriod.accountingPeriod.name,
        totalStartingBalance: firstPeriod.openingBalance.totalBalance,
        totalEndingBalance: lastPeriod.closingBalance.totalBalance,
        assignedStartingBalance:
          firstPeriod.openingBalance.totalAssignedBalance,
        assignedEndingBalance: lastPeriod.closingBalance.totalAssignedBalance,
        unassignedStartingBalance:
          firstPeriod.openingBalance.totalUnassignedBalance,
        unassignedEndingBalance:
          lastPeriod.closingBalance.totalUnassignedBalance,
      };
    }
  }

  const firstDate = dates.at(0);
  const lastDate = dates.at(-1);
  return {
    startLabel: firstDate
      ? formatShortDate(new Date(`${firstDate.date}T00:00:00`))
      : "Start",
    endLabel: lastDate
      ? formatShortDate(new Date(`${lastDate.date}T00:00:00`))
      : "End",
    totalStartingBalance: firstDate?.totalBalance ?? 0,
    totalEndingBalance: lastDate?.totalBalance ?? 0,
    assignedStartingBalance: firstDate?.totalAssignedBalance ?? 0,
    assignedEndingBalance: lastDate?.totalAssignedBalance ?? 0,
    unassignedStartingBalance: firstDate?.totalUnassignedBalance ?? 0,
    unassignedEndingBalance: lastDate?.totalUnassignedBalance ?? 0,
  };
};

/**
 * Parameter names used by the fund trends page.
 */
const fundTrendsParamNames = {
  sort: "sort",
  page: "page",
  balanceEventSort: "balanceEventSort",
  balanceEventPage: "balanceEventPage",
  mode: "mode",
  fundName: "fundName",
  startAccountingPeriodId: "startAccountingPeriodId",
  endAccountingPeriodId: "endAccountingPeriodId",
  startDate: "startDate",
  endDate: "endDate",
} as const satisfies Record<keyof FundTrendsSearchParams, string>;

/**
 * Represents the URL search parameter methods used by filter detection.
 */
interface ReadonlySearchParams {
  getAll: (name: string) => string[];
}

/**
 * Returns whether a fund-name filter narrows the trends results.
 */
const hasActiveFundTrendsFilters = function (
  searchParams: ReadonlySearchParams,
): boolean {
  return searchParams.getAll(fundTrendsParamNames.fundName).length > 0;
};

/**
 * Removes fund filters and resets both filtered result lists.
 */
const clearFundTrendsFilters = function (params: URLSearchParams): void {
  [
    fundTrendsParamNames.fundName,
    fundTrendsParamNames.page,
    fundTrendsParamNames.balanceEventPage,
  ].forEach((name) => {
    params.delete(name);
  });
};

export {
  clearFundTrendsFilters,
  fundTrendsParamNames,
  getFundTrendsSnapshot,
  hasActiveFundTrendsFilters,
};
export type { FundTrendsDataMode, FundTrendsSearchParams, FundTrendsSnapshot };
