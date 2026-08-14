"use client";

import type {
  FundBalanceSummaryByDate,
  FundBalanceSummaryByPeriod,
} from "@/funds/types";
import {
  type FundTrendsDataMode,
  getFundTrendsSnapshot,
} from "@/funds/trends/helpers";
import { type JSX, type ReactNode, useState } from "react";
import ChangeValue from "@/framework/view/ChangeValue";
import FundSummaryCard from "@/funds/FundSummaryCard";
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
 * Defines the structure of a summary card, including its title, value, and breakdowns.
 */
interface CardDefinition {
  readonly title: string;
  readonly value: ReactNode;
  readonly assignedValue: ReactNode;
  readonly unassignedValue: ReactNode;
}

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
      title: `Starting Balance (${snapshot.startLabel})`,
      value: formatCurrency(snapshot.totalStartingBalance),
      assignedValue: formatCurrency(snapshot.assignedStartingBalance),
      unassignedValue: formatCurrency(snapshot.unassignedStartingBalance),
    },
    {
      title: `Ending Balance (${snapshot.endLabel})`,
      value: formatCurrency(snapshot.totalEndingBalance),
      assignedValue: formatCurrency(snapshot.assignedEndingBalance),
      unassignedValue: formatCurrency(snapshot.unassignedEndingBalance),
    },
    {
      title: "Net Change",
      value: (
        <ChangeValue
          startingValue={snapshot.totalStartingBalance}
          endingValue={snapshot.totalEndingBalance}
        />
      ),
      assignedValue: (
        <ChangeValue
          startingValue={snapshot.assignedStartingBalance}
          endingValue={snapshot.assignedEndingBalance}
        />
      ),
      unassignedValue: (
        <ChangeValue
          startingValue={snapshot.unassignedStartingBalance}
          endingValue={snapshot.unassignedEndingBalance}
        />
      ),
    },
  ];

  return (
    <SummaryCardGrid>
      {cards.map((card) => (
        <FundSummaryCard
          key={card.title}
          {...card}
          expanded={expanded}
          onToggle={toggleExpanded}
        />
      ))}
    </SummaryCardGrid>
  );
};

export default FundTrendsSummaryCards;
