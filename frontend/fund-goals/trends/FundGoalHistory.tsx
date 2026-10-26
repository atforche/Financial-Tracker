"use client";

import {
  type FundGoalPeriodProgress,
  isFundGoalSatisfied,
} from "@/fund-goals/trends/fundGoalProgressTrends";
import type { AccountingPeriod } from "@/accounting-periods/types";
import GoalHistory from "@/framework/goals/GoalHistory";
import type { HistoryPoint } from "@/framework/goals/GoalHistoryListFrame";
import type { JSX } from "react";

/**
 * Shows how one fund's bounds, balance, and contributions changed across periods.
 */
const FundGoalHistory = function ({
  periods,
  entries,
  fundName,
}: {
  readonly periods: readonly AccountingPeriod[];
  readonly entries: readonly FundGoalPeriodProgress[];
  readonly fundName: string;
}): JSX.Element {
  const byPeriod = new Map(
    entries.map((entry) => [entry.fundGoal.accountingPeriod?.id, entry]),
  );
  const points: HistoryPoint[] = periods.map((period) => {
    const entry = byPeriod.get(period.id);
    const minimum = entry?.progress.endingBalance.minimumBalance ?? null;
    const maximum = entry?.progress.endingBalance.maximumBalance ?? null;
    return {
      periodId: period.id,
      period: period.name,
      periodOrder: period.year * 12 + period.month,
      isOpen: period.isOpen,
      expected: entry?.progress.contribution?.expectedAmount ?? null,
      assigned: entry?.progress.contribution?.assignedAmount ?? null,
      minimum,
      maximum,
      span: minimum === null || maximum === null ? null : maximum - minimum,
      balance: entry?.progress.endingBalance.endingBalance ?? null,
      satisfied: entry === undefined ? null : isFundGoalSatisfied(entry),
    };
  });
  return (
    <GoalHistory
      name={fundName}
      subject="fund"
      hasData={entries.length > 0}
      points={points}
      paramPrefix="fundGoalHistory"
      showContribution
    />
  );
};

export default FundGoalHistory;
