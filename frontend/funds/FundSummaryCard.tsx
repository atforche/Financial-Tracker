"use client";

import { Divider, Stack } from "@mui/material";
import type { JSX, ReactNode } from "react";
import BreakdownSection from "@/framework/view/BreakdownSection";
import ExpandableSummaryCard from "@/framework/view/ExpandableSummaryCard";

/**
 * Props for the FundSummaryCard component.
 */
interface FundSummaryCardProps {
  readonly title: string;
  readonly value: ReactNode;
  readonly assignedValue: ReactNode;
  readonly unassignedValue: ReactNode;
  readonly expanded: boolean;
  readonly onToggle: () => void;
}

/** Displays an expandable fund summary with assigned and unassigned balances. */
const FundSummaryCard = function ({
  title,
  value,
  assignedValue,
  unassignedValue,
  expanded,
  onToggle,
}: FundSummaryCardProps): JSX.Element {
  return (
    <ExpandableSummaryCard
      title={title}
      value={value}
      expanded={expanded}
      onToggle={onToggle}
    >
      <Stack spacing={1.25} divider={<Divider flexItem />}>
        <BreakdownSection label="Assigned" value={assignedValue} />
        <BreakdownSection label="Unassigned" value={unassignedValue} />
      </Stack>
    </ExpandableSummaryCard>
  );
};

export default FundSummaryCard;
