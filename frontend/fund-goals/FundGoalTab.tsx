"use client";

import type { FundGoal, FundGoalProgress } from "@/fund-goals/types";
import type { AccountingPeriodWithBalance } from "@/accounting-periods/types";
import FundGoalDetailsButton from "@/fund-goals/FundGoalDetailsButton";
import GoalHistory from "@/framework/goals/GoalHistory";
import type { HistoryPoint } from "@/framework/goals/GoalHistoryListFrame";
import type { JSX } from "react";
import { Stack } from "@mui/material";
import UpdateFundGoalForm from "@/fund-goals/workspace/UpdateFundGoalForm";
import { isFundGoalSatisfied } from "@/fund-goals/trends/fundGoalProgressTrends";

/**
 * Shows how one fund's goals, balance, and contributions changed across a period range, with editing for open periods.
 */
const FundGoalTab = function ({
  entries,
  openPeriods,
  redirectUrl,
}: {
  readonly entries: readonly {
    readonly goal: FundGoal;
    readonly progress: FundGoalProgress;
  }[];
  readonly openPeriods: readonly AccountingPeriodWithBalance[];
  readonly redirectUrl: string;
}): JSX.Element {
  const entriesByPeriod = new Map(
    entries.map((entry) => [entry.goal.accountingPeriod?.id, entry]),
  );
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
        satisfied: isFundGoalSatisfied({ fundGoal: goal, progress }),
      },
    ];
  });
  return (
    <GoalHistory
      subject="fund"
      hasData={points.length > 0}
      points={points}
      paramPrefix="goalHistory"
      showContribution
      renderActions={(point) => {
        const entry = entriesByPeriod.get(point.periodId);
        const period = openPeriods.find((item) => item.id === point.periodId);
        return entry === undefined ? null : (
          <Stack
            direction="row"
            spacing={1}
            sx={{ justifyContent: "flex-end" }}
          >
            <FundGoalDetailsButton
              goal={entry.goal}
              progress={entry.progress}
            />
            {period === undefined ? null : (
              <UpdateFundGoalForm
                fundGoal={entry.goal}
                accountingPeriod={period}
                redirectUrl={redirectUrl}
              />
            )}
          </Stack>
        );
      }}
    />
  );
};

export default FundGoalTab;
