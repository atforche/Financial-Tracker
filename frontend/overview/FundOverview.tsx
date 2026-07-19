"use client";

import { Divider, Stack, Typography } from "@mui/material";
import { type JSX, useState } from "react";
import ExpandableSummaryCard from "@/framework/view/ExpandableSummaryCard";
import type { FundOverviewSummary } from "@/overview/types";
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
    <ExpandableSummaryCard
      title="Current Total Fund Balances"
      value={formatCurrency(summary.totalBalance)}
      expanded={expanded}
      onToggle={() => {
        setExpanded((current) => !current);
      }}
    >
      <Stack spacing={1.25} divider={<Divider flexItem />}>
        <Stack direction="row" justifyContent="space-between" gap={2}>
          <Typography variant="body2">Assigned</Typography>
          <Typography variant="body2" fontWeight={600}>
            {formatCurrency(summary.totalAssignedBalance)}
          </Typography>
        </Stack>
        <Stack direction="row" justifyContent="space-between" gap={2}>
          <Typography variant="body2">Unassigned</Typography>
          <Typography variant="body2" fontWeight={600}>
            {formatCurrency(summary.totalUnassignedBalance)}
          </Typography>
        </Stack>
      </Stack>
    </ExpandableSummaryCard>
  );
};

export default FundOverview;
