import type {
  AccountingPeriodWithBalance,
  AccountingPeriodsInRange,
} from "@/accounting-periods/types";
import {
  type BarMetricChartPoint,
  getSignedChartColor,
} from "@/framework/charts/barMetricHelpers";
import type { BalanceTrendChartPoint } from "@/framework/charts/balanceTrendHelpers";
import { createAccountingPeriodMetricPoint } from "@/framework/charts/chartPointHelpers";
import { getCurrencyDifference } from "@/framework/currencyHelpers";

/**
 * Builds the chart points for the balance trend chart based on the provided accounting periods.
 */
const buildTrendChartPoints = function (
  accountingPeriods: readonly AccountingPeriodWithBalance[],
): BalanceTrendChartPoint[] {
  const openingPoints = accountingPeriods.map((accountingPeriod) => ({
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
      tickLabel: "End",
      tooltipLabel: `${lastAccountingPeriod.name} closing balance`,
      balance: lastAccountingPeriod.closingBalance,
    },
  ];
};

/**
 * Builds the chart points for the balance change chart based on the provided accounting periods.
 */
const buildChangeChartPoints = function (
  accountingPeriods: readonly AccountingPeriodWithBalance[],
): BarMetricChartPoint[] {
  return accountingPeriods.map((accountingPeriod) => {
    const value = getCurrencyDifference(
      accountingPeriod.closingBalance,
      accountingPeriod.openingBalance,
    );

    return createAccountingPeriodMetricPoint(
      { name: accountingPeriod.name, value },
      getSignedChartColor(value),
    );
  });
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

export { buildTrendChartPoints, buildChangeChartPoints, createEmptyTrends };
