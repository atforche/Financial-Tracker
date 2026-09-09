"use client";

import { type JSX, type ReactNode, useState } from "react";
import ChangeValue from "@/framework/view/ChangeValue";
import type { FundBalanceSummary } from "@/funds/types";
import FundSummaryCard from "@/funds/FundSummaryCard";
import SummaryCardGrid from "@/framework/view/SummaryCardGrid";
import { formatCurrency } from "@/framework/currencyHelpers";

interface FundBalanceSummaryCardsProps {
  readonly startingLabel: string;
  readonly endingLabel: string;
  readonly startingBalance: FundBalanceSummary;
  readonly endingBalance: FundBalanceSummary;
  readonly showLabels?: boolean;
}

interface CardDefinition {
  readonly title: string;
  readonly value: ReactNode;
  readonly assignedValue: ReactNode;
  readonly unassignedValue: ReactNode;
}

/** Displays fund balances for a selected range. */
const FundBalanceSummaryCards = function ({
  startingLabel,
  endingLabel,
  startingBalance,
  endingBalance,
  showLabels = true,
}: FundBalanceSummaryCardsProps): JSX.Element {
  const [expanded, setExpanded] = useState(false);
  const cards: readonly CardDefinition[] = [
    {
      title: showLabels
        ? `Starting Balance (${startingLabel})`
        : "Starting Balance",
      value: formatCurrency(startingBalance.totalBalance),
      assignedValue: formatCurrency(startingBalance.totalAssignedBalance),
      unassignedValue: formatCurrency(startingBalance.totalUnassignedBalance),
    },
    {
      title: showLabels ? `Ending Balance (${endingLabel})` : "Ending Balance",
      value: formatCurrency(endingBalance.totalBalance),
      assignedValue: formatCurrency(endingBalance.totalAssignedBalance),
      unassignedValue: formatCurrency(endingBalance.totalUnassignedBalance),
    },
    {
      title: "Net Change",
      value: (
        <ChangeValue
          startingValue={startingBalance.totalBalance}
          endingValue={endingBalance.totalBalance}
        />
      ),
      assignedValue: (
        <ChangeValue
          startingValue={startingBalance.totalAssignedBalance}
          endingValue={endingBalance.totalAssignedBalance}
        />
      ),
      unassignedValue: (
        <ChangeValue
          startingValue={startingBalance.totalUnassignedBalance}
          endingValue={endingBalance.totalUnassignedBalance}
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
          onToggle={() => {
            setExpanded((value) => !value);
          }}
        />
      ))}
    </SummaryCardGrid>
  );
};

export type { FundBalanceSummaryCardsProps };
export default FundBalanceSummaryCards;
