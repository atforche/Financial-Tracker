"use client";

import type { AccountGoal, AccountGoalProgress } from "@/account-goals/types";
import GoalHistory from "@/framework/goals/GoalHistory";
import type { HistoryPoint } from "@/framework/goals/GoalHistoryTable";
import type { JSX } from "react";
import accountingPeriodRoutes from "@/accounting-periods/routes";

/**
 * Shows how one account's goals and balance changed across a period range.
 */
const AccountGoalHistory = function ({
  entries,
  changes,
  redirectUrl,
}: {
  readonly entries: readonly {
    readonly goal: AccountGoal;
    readonly progress: AccountGoalProgress;
  }[];
  readonly changes: ReadonlyMap<string, number>;
  readonly redirectUrl: string;
}): JSX.Element {
  const points = entries.flatMap(({ goal, progress }): HistoryPoint[] => {
    const period = goal.accountingPeriod;
    if (period === null || period === undefined) {
      return [];
    }
    const { minimumBalance, maximumBalance, currentBalance } =
      progress.endingBalance;
    return [
      {
        periodId: period.id,
        href: accountingPeriodRoutes.accountDetail(period.id, goal.account.id, {
          returnUrl: redirectUrl,
        }),
        period: period.name,
        periodOrder: period.year * 12 + period.month,
        isOpen: period.isOpen,
        expected: null,
        assigned: null,
        minimum: minimumBalance,
        maximum: maximumBalance ?? null,
        span:
          maximumBalance === null || maximumBalance === undefined
            ? null
            : maximumBalance - minimumBalance,
        balance: currentBalance,
        change: changes.get(period.id) ?? null,
        satisfied: progress.isSatisfied,
      },
    ];
  });
  return (
    <GoalHistory
      subject="account"
      hasData={points.length > 0}
      points={points}
      showContribution={false}
    />
  );
};

export default AccountGoalHistory;
