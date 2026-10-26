"use client";

import type { AccountGoalPeriodProgress } from "@/account-goals/trends/accountGoalProgressTrends";
import type { AccountingPeriod } from "@/accounting-periods/types";
import GoalHistory from "@/framework/goals/GoalHistory";
import type { HistoryPoint } from "@/framework/goals/GoalHistoryListFrame";
import type { JSX } from "react";

/**
 * Shows how one account's bounds and balance changed across periods.
 */
const AccountGoalHistory = function ({
  periods,
  entries,
  accountName,
}: {
  readonly periods: readonly AccountingPeriod[];
  readonly entries: readonly AccountGoalPeriodProgress[];
  readonly accountName: string;
}): JSX.Element {
  const byPeriod = new Map(
    entries.map((entry) => [entry.accountGoal.accountingPeriod?.id, entry]),
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
      expected: null,
      assigned: null,
      minimum,
      maximum,
      span: minimum === null || maximum === null ? null : maximum - minimum,
      balance: entry?.progress.endingBalance.currentBalance ?? null,
      satisfied: entry?.progress.isSatisfied ?? null,
    };
  });
  return (
    <GoalHistory
      name={accountName}
      subject="account"
      hasData={entries.length > 0}
      points={points}
      paramPrefix="goalHistory"
      showContribution={false}
    />
  );
};

export default AccountGoalHistory;
