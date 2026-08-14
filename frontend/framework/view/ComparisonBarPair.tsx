"use client";

import ComparisonBar from "@/framework/view/ComparisonBar";
import type { JSX } from "react";
import { Stack } from "@mui/material";

/**
 * Describes one amount in a ComparisonBarPair.
 */
interface ComparisonBarPairItem {
  readonly label: string;
  readonly amount: number;
  readonly color: string;
  readonly differenceLabel: string;
  readonly differenceColor: string;
}

/**
 * Props for the ComparisonBarPair component.
 */
interface ComparisonBarPairProps {
  readonly first: ComparisonBarPairItem;
  readonly second: ComparisonBarPairItem;
}

/**
 * Displays two labeled amounts as proportional bars on a shared scale.
 */
const ComparisonBarPair = function ({
  first,
  second,
}: ComparisonBarPairProps): JSX.Element {
  const maxAmount = Math.max(first.amount, second.amount, 1);
  const firstIsLesser = first.amount < second.amount;
  const firstDifference = firstIsLesser ? second.amount - first.amount : 0;
  const secondDifference = firstIsLesser ? 0 : first.amount - second.amount;

  return (
    <Stack spacing={1.5}>
      <ComparisonBar
        label={first.label}
        amount={first.amount}
        amountColor={first.color}
        difference={firstDifference}
        differenceLabel={first.differenceLabel}
        differenceColor={first.differenceColor}
        maxAmount={maxAmount}
      />
      <ComparisonBar
        label={second.label}
        amount={second.amount}
        amountColor={second.color}
        difference={secondDifference}
        differenceLabel={second.differenceLabel}
        differenceColor={second.differenceColor}
        maxAmount={maxAmount}
      />
    </Stack>
  );
};

export type { ComparisonBarPairItem, ComparisonBarPairProps };
export default ComparisonBarPair;
