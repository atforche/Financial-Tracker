"use client";

import { type JSX, useState } from "react";
import type { FundOverviewSummary } from "@/overview/types";
import FundSummaryCard from "@/funds/FundSummaryCard";
import { formatCurrency } from "@/framework/currencyHelpers";

/**
 * Props for the FundOverview component.
 */
interface FundOverviewProps {
  readonly summary: FundOverviewSummary;
}

/**
 * Overview components for Funds.
 */
const FundOverview = function ({ summary }: FundOverviewProps): JSX.Element {
  const [expanded, setExpanded] = useState(false);

  return (
    <FundSummaryCard
      title="Current Total Fund Balances"
      value={formatCurrency(summary.totalBalance)}
      assignedValue={formatCurrency(summary.totalAssignedBalance)}
      unassignedValue={formatCurrency(summary.totalUnassignedBalance)}
      expanded={expanded}
      onToggle={() => {
        setExpanded((current) => !current);
      }}
    />
  );
};

export default FundOverview;
