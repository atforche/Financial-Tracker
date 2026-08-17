"use client";

import type { IncomeAmount } from "@/transactions/types";
import IncomeBreakdownBar from "@/framework/view/IncomeBreakdownBar";
import type { JSX } from "react";
import SummaryCard from "@/framework/view/SummaryCard";

/**
 * Props for the ActualIncomeCard component.
 */
interface ActualIncomeCardProps {
  readonly totalIncome: IncomeAmount | undefined;
}

/**
 * Displays actual income as tracked and untracked sections of a stacked bar.
 */
const ActualIncomeCard = function ({
  totalIncome,
}: ActualIncomeCardProps): JSX.Element {
  const income = totalIncome ?? { total: 0, tracked: 0, untracked: 0 };

  return (
    <SummaryCard title="Actual Income">
      <IncomeBreakdownBar
        total={income.total}
        tracked={income.tracked}
        untracked={income.untracked}
      />
    </SummaryCard>
  );
};

export type { ActualIncomeCardProps };
export default ActualIncomeCard;
