import type {
  AccountType,
  AccountWithBalanceRangeSort,
} from "@/accounts/types";
import type { TrendRangeMode } from "@/framework/routes/trendRange";

/**
 * Search parameters supported by the account trends page.
 */
interface AccountTrendsSearchParams {
  sort?: AccountWithBalanceRangeSort;
  page?: number | string | null;
  pageSize?: number | string | null;
  mode?: TrendRangeMode;
  accountType?: AccountType | readonly AccountType[];
  accountName?: string | readonly string[];
  startAccountingPeriodId?: string;
  endAccountingPeriodId?: string;
  startDate?: string;
  endDate?: string;
  returnUrl?: string | undefined;
}

/**
 * Parameter names used by the account trends page.
 */
const accountTrendsParamNames = {
  sort: "sort",
  page: "page",
  pageSize: "pageSize",
  mode: "mode",
  accountType: "accountType",
  accountName: "accountName",
  startAccountingPeriodId: "startAccountingPeriodId",
  endAccountingPeriodId: "endAccountingPeriodId",
  startDate: "startDate",
  endDate: "endDate",
  returnUrl: "returnUrl",
} as const satisfies Record<keyof AccountTrendsSearchParams, string>;

/**
 * Represents a read-only view of URL search parameters.
 */
interface ReadonlySearchParams {
  getAll: (name: string) => string[];
}

/**
 * Returns whether account filters narrow the trends results.
 */
const hasActiveAccountTrendsFilters = function (
  searchParams: ReadonlySearchParams,
): boolean {
  const names = accountTrendsParamNames;
  return (
    searchParams.getAll(names.accountType).length > 0 ||
    searchParams.getAll(names.accountName).length > 0
  );
};

/**
 * Removes account filters and returns to the first page.
 */
const clearAccountTrendsFilters = function (params: URLSearchParams): void {
  const names = accountTrendsParamNames;
  [names.accountType, names.accountName, names.page].forEach((name) => {
    params.delete(name);
  });
};

export {
  accountTrendsParamNames,
  clearAccountTrendsFilters,
  hasActiveAccountTrendsFilters,
};
export type { AccountTrendsSearchParams };
