"use client";

import type { AccountGoal, AccountGoalProgress } from "@/account-goals/types";
import GoalHistory from "@/framework/goals/GoalHistory";
import type { HistoryPoint } from "@/framework/goals/GoalHistoryListFrame";
import type { JSX } from "react";
import UpdateAccountGoalForm from "@/account-goals/workspace/UpdateAccountGoalForm";

/**
 * Shows how one account's goals and balance changed across a period range, with editing for open periods.
 */
const AccountGoalTab = function ({
  entries,
  redirectUrl,
}: {
  readonly entries: readonly {
    readonly goal: AccountGoal;
    readonly progress: AccountGoalProgress;
  }[];
  readonly redirectUrl: string;
}): JSX.Element {
  const goals = new Map(
    entries.map(({ goal }) => [goal.accountingPeriod?.id, goal]),
  );
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
        satisfied: progress.isSatisfied,
      },
    ];
  });
  return (
    <GoalHistory
      subject="account"
      hasData={points.length > 0}
      points={points}
      paramPrefix="goalHistory"
      showContribution={false}
      renderActions={(point) => {
        const goal = goals.get(point.periodId);
        return goal === undefined || !point.isOpen ? null : (
          <UpdateAccountGoalForm accountGoal={goal} redirectUrl={redirectUrl} />
        );
      }}
    />
  );
};

export default AccountGoalTab;
