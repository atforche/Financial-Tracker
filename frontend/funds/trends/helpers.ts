import type {
  FundBalanceSummaryByDate,
  FundBalanceSummaryByPeriod,
} from "@/funds/types";
import formatShortDate from "@/framework/formatShortDate";

type FundTrendsDataMode = "AccountingPeriod" | "Date";

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

/** Builds the summary snapshot for the selected fund trends range. */
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

export { getFundTrendsSnapshot };
export type { FundTrendsDataMode, FundTrendsSnapshot };
