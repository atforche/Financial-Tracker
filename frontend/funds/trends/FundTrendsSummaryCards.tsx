"use client";

import { type ComponentProps, type JSX, useState } from "react";
import { Divider, Stack } from "@mui/material";
import type {
  FundBalanceSummaryByDate,
  FundBalanceSummaryByPeriod,
} from "@/funds/types";
import {
  type FundTrendsDataMode,
  getFundTrendsSnapshot,
} from "@/funds/trends/helpers";
import BreakdownSection from "@/framework/view/BreakdownSection";
import ChangeValue from "@/framework/view/ChangeValue";
import ExpandableSummaryCard from "@/framework/view/ExpandableSummaryCard";
import SummaryCardGrid from "@/framework/view/SummaryCardGrid";
import { formatCurrency } from "@/framework/currencyHelpers";

/**
 * Props for the FundTrendsSummaryCards component.
 */
interface FundTrendsSummaryCardsProps {
  readonly mode: FundTrendsDataMode;
  readonly accountingPeriods: readonly FundBalanceSummaryByPeriod[];
  readonly dates: readonly FundBalanceSummaryByDate[];
}

/**
 * Defines the breakdown of a summary card, including its label and value.
 */
type BreakdownDefinition = Pick<
  ComponentProps<typeof BreakdownSection>,
  "label" | "value"
>;

/**
 * Defines the structure of a summary card, including its title, value, and breakdowns.
 */
type CardDefinition = Pick<
  ComponentProps<typeof ExpandableSummaryCard>,
  "title" | "value"
> & {
  readonly breakdowns: readonly BreakdownDefinition[];
};

/** Displays fund balances for the selected trends range. */
const FundTrendsSummaryCards = function ({
  mode,
  accountingPeriods,
  dates,
}: FundTrendsSummaryCardsProps): JSX.Element {
  const snapshot = getFundTrendsSnapshot(mode, accountingPeriods, dates);
  const [expanded, setExpanded] = useState(false);
  const toggleExpanded = function (): void {
    setExpanded((value) => !value);
  };

  const cards: readonly CardDefinition[] = [
    {
      title: `Starting balance (${snapshot.startLabel})`,
      value: formatCurrency(snapshot.totalStartingBalance),
      breakdowns: [
        {
          label: "Assigned",
          value: formatCurrency(snapshot.assignedStartingBalance),
        },
        {
          label: "Unassigned",
          value: formatCurrency(snapshot.unassignedStartingBalance),
        },
      ],
    },
    {
      title: `Ending balance (${snapshot.endLabel})`,
      value: formatCurrency(snapshot.totalEndingBalance),
      breakdowns: [
        {
          label: "Assigned",
          value: formatCurrency(snapshot.assignedEndingBalance),
        },
        {
          label: "Unassigned",
          value: formatCurrency(snapshot.unassignedEndingBalance),
        },
      ],
    },
    {
      title: "Net change",
      value: (
        <ChangeValue
          startingValue={snapshot.totalStartingBalance}
          endingValue={snapshot.totalEndingBalance}
        />
      ),
      breakdowns: [
        {
          label: "Assigned",
          value: (
            <ChangeValue
              startingValue={snapshot.assignedStartingBalance}
              endingValue={snapshot.assignedEndingBalance}
            />
          ),
        },
        {
          label: "Unassigned",
          value: (
            <ChangeValue
              startingValue={snapshot.unassignedStartingBalance}
              endingValue={snapshot.unassignedEndingBalance}
            />
          ),
        },
      ],
    },
  ];

  return (
    <SummaryCardGrid>
      {cards.map((card) => (
        <ExpandableSummaryCard
          key={card.title}
          title={card.title}
          value={card.value}
          expanded={expanded}
          onToggle={toggleExpanded}
        >
          <Stack spacing={1.25} divider={<Divider flexItem />}>
            {card.breakdowns.map((breakdown) => (
              <BreakdownSection key={breakdown.label} {...breakdown} />
            ))}
          </Stack>
        </ExpandableSummaryCard>
      ))}
    </SummaryCardGrid>
  );
};

export default FundTrendsSummaryCards;
