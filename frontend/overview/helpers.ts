import {
  type BalanceTrendChartPoint,
  buildBalanceTrendChartPoints,
} from "@/framework/charts/balanceTrendHelpers";
import type { TrendRangeMode } from "@/framework/routes/trendRange";
import type { components } from "@/framework/data/api";
import { formatShortDate } from "@/framework/dateHelpers";

/**
 * Search params for the overview page.
 */
interface OverviewSearchParams {
  mode?: TrendRangeMode;
  startAccountingPeriodId?: string;
  endAccountingPeriodId?: string;
  startDate?: string;
  endDate?: string;
}

const overviewParamNames = {
  mode: "mode",
  startAccountingPeriodId: "startAccountingPeriodId",
  endAccountingPeriodId: "endAccountingPeriodId",
  startDate: "startDate",
  endDate: "endDate",
} as const satisfies Record<keyof OverviewSearchParams, string>;

/**
 * A total balance at the end of a date.
 */
type OverviewDateBalance = components["schemas"]["OverviewDateBalanceModel"];

/**
 * Total opening and closing balances for an accounting period.
 */
type OverviewPeriodBalance =
  components["schemas"]["OverviewPeriodBalanceModel"];

/**
 * Everything needed to display the account or fund section of the overview.
 */
interface OverviewSection<TSummary> {
  readonly startLabel: string;
  readonly endLabel: string;
  readonly openingBalance: TSummary;
  readonly closingBalance: TSummary;
  readonly trendPoints: BalanceTrendChartPoint[];
  readonly changeDates: readonly OverviewDateBalance[];
  readonly changePeriods: readonly {
    readonly accountingPeriod: { readonly name: string };
    readonly openingBalance: { readonly totalBalance: number };
    readonly closingBalance: { readonly totalBalance: number };
  }[];
}

/**
 * Builds an overview section from daily balances.
 */
const buildDateSection = function <TSummary>(
  balances: readonly OverviewDateBalance[],
  openingBalance: TSummary,
  closingBalance: TSummary,
): OverviewSection<TSummary> {
  const formatLabel = function (
    balance: OverviewDateBalance | undefined,
    fallback: string,
  ): string {
    return typeof balance === "undefined"
      ? fallback
      : formatShortDate(new Date(`${balance.date}T00:00:00`));
  };
  return {
    startLabel: formatLabel(balances.at(0), "Start"),
    endLabel: formatLabel(balances.at(-1), "End"),
    openingBalance,
    closingBalance,
    trendPoints: buildBalanceTrendChartPoints({
      mode: "Date",
      accountingPeriods: null,
      dates: balances,
    }),
    changeDates: balances,
    changePeriods: [],
  };
};

/**
 * Builds an overview section from accounting period balances.
 */
const buildPeriodSection = function <TSummary>(
  balances: readonly OverviewPeriodBalance[],
  openingBalance: TSummary,
  closingBalance: TSummary,
): OverviewSection<TSummary> {
  return {
    startLabel: balances.at(0)?.accountingPeriod.name ?? "Start",
    endLabel: balances.at(-1)?.accountingPeriod.name ?? "End",
    openingBalance,
    closingBalance,
    trendPoints: buildBalanceTrendChartPoints({
      mode: "AccountingPeriod",
      accountingPeriods: balances.map((balance) => ({
        accountingPeriodId: balance.accountingPeriod.id,
        accountingPeriodName: balance.accountingPeriod.name,
        year: balance.accountingPeriod.year,
        month: balance.accountingPeriod.month,
        totalOpeningBalance: balance.openingBalance,
        totalClosingBalance: balance.closingBalance,
      })),
      dates: null,
    }),
    changeDates: [],
    changePeriods: balances.map((balance) => ({
      accountingPeriod: balance.accountingPeriod,
      openingBalance: { totalBalance: balance.openingBalance },
      closingBalance: { totalBalance: balance.closingBalance },
    })),
  };
};

export { buildDateSection, buildPeriodSection, overviewParamNames };
export type { OverviewSearchParams, OverviewSection };
