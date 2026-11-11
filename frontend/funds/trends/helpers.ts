import type { FundWithBalanceRangeSort } from "@/funds/types";
import type { TrendRangeMode } from "@/framework/routes/trendRange";

/**
 * Search parameters supported by the fund trends page.
 */
interface FundTrendsSearchParams {
  sort?: FundWithBalanceRangeSort;
  page?: number | string | null;
  mode?: TrendRangeMode;
  fundName?: string | readonly string[];
  startAccountingPeriodId?: string;
  endAccountingPeriodId?: string;
  startDate?: string;
  endDate?: string;
  returnUrl?: string | undefined;
}

/**
 * Parameter names used by the fund trends page.
 */
const fundTrendsParamNames = {
  sort: "sort",
  page: "page",
  mode: "mode",
  fundName: "fundName",
  startAccountingPeriodId: "startAccountingPeriodId",
  endAccountingPeriodId: "endAccountingPeriodId",
  startDate: "startDate",
  endDate: "endDate",
  returnUrl: "returnUrl",
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
 * Removes fund filters and returns to the first page.
 */
const clearFundTrendsFilters = function (params: URLSearchParams): void {
  [fundTrendsParamNames.fundName, fundTrendsParamNames.page].forEach((name) => {
    params.delete(name);
  });
};

export {
  clearFundTrendsFilters,
  fundTrendsParamNames,
  hasActiveFundTrendsFilters,
};
export type { FundTrendsSearchParams };
