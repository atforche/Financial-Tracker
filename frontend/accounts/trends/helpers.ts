import type {
  AccountBalanceEventSort,
  AccountBalanceSummaryByDate,
  AccountBalanceSummaryByPeriod,
  AccountType,
  AccountTypeBalance,
  AccountWithBalanceRangeSort,
} from "@/accounts/types";
import {
  type BarMetricChartPoint,
  getSignedBarColor,
} from "@/framework/charts/barMetricHelpers";
import type { TrendRangeMode } from "@/framework/routes/trendRange";
import dayjs from "dayjs";
import formatLongDate from "@/framework/formatLongDate";
import formatShortDate from "@/framework/formatShortDate";

/**
 * Search parameters supported by the account trends page.
 */
interface AccountTrendsSearchParams {
  sort?: AccountWithBalanceRangeSort;
  page?: number | string | null;
  balanceEventSort?: AccountBalanceEventSort;
  balanceEventPage?: number | string | null;
  mode?: TrendRangeMode;
  accountType?: AccountType | readonly AccountType[];
  accountName?: string | readonly string[];
  startAccountingPeriodId?: string;
  endAccountingPeriodId?: string;
  startDate?: string;
  endDate?: string;
}

/**
 * Mode for the account trends data, either by accounting period or by date.
 */
type AccountTrendsDataMode = "AccountingPeriod" | "Date";

/**
 * Snapshot of account balances and changes for the selected trends range.
 */
interface AccountTrendsSnapshot {
  readonly startLabel: string;
  readonly endLabel: string;
  readonly totalStartingBalance: number;
  readonly totalEndingBalance: number;
  readonly trackedStartingBalance: number;
  readonly trackedEndingBalance: number;
  readonly untrackedStartingBalance: number;
  readonly untrackedEndingBalance: number;
  readonly startingBalancesByType: readonly AccountTypeBalance[];
  readonly endingBalancesByType: readonly AccountTypeBalance[];
}

/**
 * Details of account balances and changes for a specific account type.
 */
interface AccountTypeBreakdownDetail {
  readonly accountType: AccountType;
  readonly startingBalance: number;
  readonly endingBalance: number;
  readonly netChange: number;
}

/**
 * Builds the summary snapshot for the selected account trends range.
 */
const getAccountTrendsSnapshot = function (
  mode: AccountTrendsDataMode,
  accountingPeriods: readonly AccountBalanceSummaryByPeriod[],
  dates: readonly AccountBalanceSummaryByDate[],
): AccountTrendsSnapshot {
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
        trackedStartingBalance: firstPeriod.openingBalance.totalTrackedBalance,
        trackedEndingBalance: lastPeriod.closingBalance.totalTrackedBalance,
        untrackedStartingBalance:
          firstPeriod.openingBalance.totalUntrackedBalance,
        untrackedEndingBalance: lastPeriod.closingBalance.totalUntrackedBalance,
        startingBalancesByType: firstPeriod.openingBalance.balanceByAccountType,
        endingBalancesByType: lastPeriod.closingBalance.balanceByAccountType,
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
    trackedStartingBalance: firstDate?.totalTrackedBalance ?? 0,
    trackedEndingBalance: lastDate?.totalTrackedBalance ?? 0,
    untrackedStartingBalance: firstDate?.totalUntrackedBalance ?? 0,
    untrackedEndingBalance: lastDate?.totalUntrackedBalance ?? 0,
    startingBalancesByType: firstDate?.balanceByAccountType ?? [],
    endingBalancesByType: lastDate?.balanceByAccountType ?? [],
  };
};

/**
 * Merges starting and ending balances into account-type change details.
 */
const getAccountTypeBreakdownDetails = function (
  startingBalancesByType: readonly AccountTypeBalance[],
  endingBalancesByType: readonly AccountTypeBalance[],
): AccountTypeBreakdownDetail[] {
  const starting = new Map(
    startingBalancesByType.map(({ accountType, totalBalance }) => [
      accountType,
      totalBalance,
    ]),
  );
  const ending = new Map(
    endingBalancesByType.map(({ accountType, totalBalance }) => [
      accountType,
      totalBalance,
    ]),
  );

  return Array.from(new Set([...starting.keys(), ...ending.keys()]))
    .map((accountType) => {
      const startingBalance = starting.get(accountType) ?? 0;
      const endingBalance = ending.get(accountType) ?? 0;
      return {
        accountType,
        startingBalance,
        endingBalance,
        netChange: endingBalance - startingBalance,
      };
    })
    .sort(
      (left, right) =>
        Math.abs(right.endingBalance) - Math.abs(left.endingBalance),
    );
};

/**
 * Parameter names used by the account trends page.
 */
const accountTrendsParamNames = {
  sort: "sort",
  page: "page",
  balanceEventSort: "balanceEventSort",
  balanceEventPage: "balanceEventPage",
  mode: "mode",
  accountType: "accountType",
  accountName: "accountName",
  startAccountingPeriodId: "startAccountingPeriodId",
  endAccountingPeriodId: "endAccountingPeriodId",
  startDate: "startDate",
  endDate: "endDate",
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
 * Removes account filters and resets both filtered result lists.
 */
const clearAccountTrendsFilters = function (params: URLSearchParams): void {
  const names = accountTrendsParamNames;
  [
    names.accountType,
    names.accountName,
    names.page,
    names.balanceEventPage,
  ].forEach((name) => {
    params.delete(name);
  });
};

/**
 * Builds chart points for the balance change chart based on the selected mode and provided data.
 */
const buildChartPoints = function (
  mode: AccountTrendsDataMode,
  accountingPeriods: readonly AccountBalanceSummaryByPeriod[],
  dates: readonly AccountBalanceSummaryByDate[],
): BarMetricChartPoint[] {
  if (mode === "AccountingPeriod") {
    return accountingPeriods.map((summary) => {
      const value =
        summary.closingBalance.totalBalance -
        summary.openingBalance.totalBalance;
      return {
        key: summary.accountingPeriod.id,
        tickLabel: summary.accountingPeriod.name,
        tooltipLabel: summary.accountingPeriod.name,
        value,
        fill: getSignedBarColor(value),
      };
    });
  }

  if (dates.length === 1) {
    const [summary] = dates;
    return typeof summary === "undefined"
      ? []
      : [
          {
            key: summary.date,
            tickLabel: dayjs(summary.date).format("MMM D"),
            tooltipLabel: formatLongDate(new Date(`${summary.date}T00:00:00`)),
            value: 0,
            fill: getSignedBarColor(0),
          },
        ];
  }

  return dates.slice(1).map((summary, index) => {
    const value = summary.totalBalance - (dates[index]?.totalBalance ?? 0);
    return {
      key: summary.date,
      tickLabel: dayjs(summary.date).format("MMM D"),
      tooltipLabel: formatLongDate(new Date(`${summary.date}T00:00:00`)),
      value,
      fill: getSignedBarColor(value),
    };
  });
};

export {
  accountTrendsParamNames,
  buildChartPoints,
  clearAccountTrendsFilters,
  getAccountTrendsSnapshot,
  getAccountTypeBreakdownDetails,
  hasActiveAccountTrendsFilters,
};
export type {
  AccountTrendsDataMode,
  AccountTrendsSearchParams,
  AccountTrendsSnapshot,
  AccountTypeBreakdownDetail,
};
