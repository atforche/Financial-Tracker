import type { AccountingPeriodWithBalance, AccountingPeriodsInRange } from "@/accounting-periods/types";
import type { BalanceTrendChartPoint } from "@/framework/charts/helpers";

/**
 * Builds the chart points for the balance trend chart based on the provided accounting periods.
 */
const buildChartPoints = function (
  accountingPeriods: readonly AccountingPeriodWithBalance[],
): BalanceTrendChartPoint[] {
  const openingPoints = accountingPeriods.map((accountingPeriod) => ({
    key: `${accountingPeriod.id}-opening`,
    tickLabel: accountingPeriod.name,
    tooltipLabel: `${accountingPeriod.name} opening balance`,
    balance: accountingPeriod.openingBalance,
  }));

  const lastAccountingPeriod = accountingPeriods.at(-1);
  if (typeof lastAccountingPeriod === "undefined") {
    return openingPoints;
  }

  return [
    ...openingPoints,
    {
      key: `${lastAccountingPeriod.id}-closing`,
      tickLabel: "End",
      tooltipLabel: `${lastAccountingPeriod.name} closing balance`,
      balance: lastAccountingPeriod.closingBalance,
    },
  ];
};

/**
 * Creates an empty AccountingPeriodsInRange object with default values.
 */
const createEmptyTrends = function (): AccountingPeriodsInRange {
  return {
    accountingPeriods: {
      items: [],
      totalCount: 0,
    },
    totalIncome: {
      total: 0,
      tracked: 0,
      untracked: 0,
    },
    totalSpending: 0,
  };
};

export { buildChartPoints, createEmptyTrends };