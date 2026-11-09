import type { AccountingPeriodProgress } from "@/accounting-periods/workspace/AccountingPeriodProgressCard";
import type { AccountingPeriodWithBalance } from "@/accounting-periods/types";
import type { IncomeAmount } from "@/transactions/types";
import { getCurrencyTotal } from "@/framework/currencyHelpers";

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

export { sumProgress };
