"use client";

import BreakdownSection, {
  type BreakdownDetailRow,
} from "@/framework/view/BreakdownSection";
import { Divider, Stack } from "@mui/material";
import type { JSX, ReactNode } from "react";
import ExpandableSummaryCard from "@/framework/view/ExpandableSummaryCard";

/**
 * Props for the AccountSummaryCard component.
 */
interface AccountSummaryCardProps {
  readonly title: string;
  readonly value: ReactNode;
  readonly trackedValue: ReactNode;
  readonly untrackedValue: ReactNode;
  readonly trackedDetailRows: readonly BreakdownDetailRow[];
  readonly untrackedDetailRows: readonly BreakdownDetailRow[];
  readonly expanded: boolean;
  readonly onToggle: () => void;
  readonly trackedExpanded: boolean;
  readonly onTrackedToggle: () => void;
  readonly untrackedExpanded: boolean;
  readonly onUntrackedToggle: () => void;
}

/**
 * Displays an expandable account summary with tracked and untracked details.
 */
const AccountSummaryCard = function ({
  title,
  value,
  trackedValue,
  untrackedValue,
  trackedDetailRows,
  untrackedDetailRows,
  expanded,
  onToggle,
  trackedExpanded,
  onTrackedToggle,
  untrackedExpanded,
  onUntrackedToggle,
}: AccountSummaryCardProps): JSX.Element {
  return (
    <ExpandableSummaryCard
      title={title}
      value={value}
      expanded={expanded}
      onToggle={onToggle}
    >
      <Stack spacing={1.25} divider={<Divider flexItem />}>
        <BreakdownSection
          label="Tracked"
          value={trackedValue}
          detailRows={trackedDetailRows}
          expanded={trackedExpanded}
          onToggle={onTrackedToggle}
        />
        <BreakdownSection
          label="Untracked"
          value={untrackedValue}
          detailRows={untrackedDetailRows}
          expanded={untrackedExpanded}
          onToggle={onUntrackedToggle}
        />
      </Stack>
    </ExpandableSummaryCard>
  );
};

export default AccountSummaryCard;
