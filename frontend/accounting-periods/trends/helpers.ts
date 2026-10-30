import {
  type BarMetricChartPoint,
  getSignedChartColor,
} from "@/framework/charts/barMetricHelpers";
import {
  getCurrencyDifference,
  getCurrencyTotal,
} from "@/framework/currencyHelpers";
import type { AccountingPeriodProgress } from "@/accounting-periods/workspace/AccountingPeriodProgressCard";
import type { AccountingPeriodWithBalance } from "@/accounting-periods/types";
import type { BalanceTrendChartPoint } from "@/framework/charts/balanceTrendHelpers";
import type { IncomeAmount } from "@/transactions/types";
import { createAccountingPeriodMetricPoint } from "@/framework/charts/chartPointHelpers";

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
 * Adds up the provided accounting periods' income and Fund Goal contribution totals.
 */
const sumProgress = function (
  accountingPeriods: readonly AccountingPeriodWithBalance[],
): AccountingPeriodProgress {
  const sum = (
    getAmount: (period: AccountingPeriodWithBalance) => number,
  ): number => getCurrencyTotal(accountingPeriods.map(getAmount));
  const sumIncome = (
    getIncome: (period: AccountingPeriodWithBalance) => IncomeAmount,
  ): IncomeAmount => ({
    total: sum((period) => getIncome(period).total),
    tracked: sum((period) => getIncome(period).tracked),
    untracked: sum((period) => getIncome(period).untracked),
  });
  return {
    expectedIncome: sumIncome((period) => period.expectedIncome),
    actualIncome: sumIncome((period) => period.actualIncome),
    unplannedIncome: sumIncome((period) => period.unplannedIncome),
    expectedGoalContributions: sum(
      (period) => period.expectedGoalContributions,
    ),
    actualGoalContributions: sum((period) => period.actualGoalContributions),
    actualUnplannedGoalContributions: sum(
      (period) => period.actualUnplannedGoalContributions,
    ),
  };
};

export { buildTrendChartPoints, buildChangeChartPoints, sumProgress };
