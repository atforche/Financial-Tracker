"use client";

import type { FundGoal, FundGoalProgress } from "@/fund-goals/types";
import GoalHistory from "@/framework/goals/GoalHistory";
import type { HistoryPoint } from "@/framework/goals/GoalHistoryTable";
import type { JSX } from "react";
import accountingPeriodRoutes from "@/accounting-periods/routes";
import isFundGoalMet from "@/fund-goals/isFundGoalMet";

/**
 * Shows how one fund's goals, balance, and contributions changed across a period range.
 */
const FundGoalHistory = function ({
  entries,
  changes,
  redirectUrl,
}: {
  readonly entries: readonly {
    readonly goal: FundGoal;
    readonly progress: FundGoalProgress;
  }[];
  readonly changes: ReadonlyMap<string, number>;
  readonly redirectUrl: string;
}): JSX.Element {
  const points = entries.flatMap(({ goal, progress }): HistoryPoint[] => {
    const period = goal.accountingPeriod;
    if (period === null || period === undefined) {
      return [];
    }
    const { minimumBalance, maximumBalance, endingBalance } =
      progress.endingBalance;
    return [
      {
        periodId: period.id,
        href: accountingPeriodRoutes.fundDetail(period.id, goal.fund.id, {
          returnUrl: redirectUrl,
        }),
        period: period.name,
        periodOrder: period.year * 12 + period.month,
        isOpen: period.isOpen,
        expected: progress.contribution?.expectedAmount ?? null,
        assigned: progress.contribution?.assignedAmount ?? null,
        minimum: minimumBalance,
        maximum: maximumBalance ?? null,
        span:
          maximumBalance === null || maximumBalance === undefined
            ? null
            : maximumBalance - minimumBalance,
        balance: endingBalance,
        change: changes.get(period.id) ?? null,
        satisfied: isFundGoalMet(progress),
      },
    ];
  });
  return (
    <GoalHistory
      subject="fund"
      hasData={points.length > 0}
      points={points}
      showContribution
    />
  );
};

export default FundGoalHistory;
