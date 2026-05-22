import { Box } from "@mui/material";
import type { JSX } from "react";
import type { OverviewData } from "@/overview/types";
import SummaryCard from "@/framework/view/SummaryCard";
import formatCurrency from "@/framework/formatCurrency";

/**
 * Props for the OverviewMetrics component.
 */
interface OverviewMetricsProps {
  readonly data: OverviewData;
}

/**
 * Displays the top-level summary metrics for the overview page.
 */
const OverviewMetrics = function ({ data }: OverviewMetricsProps): JSX.Element {
  const {
    accountSummary,
    fundSummary,
    currentAccountingPeriod,
    openAccountingPeriods,
    totalAccountingPeriods,
  } = data;

  return (
    <Box
      sx={{
        display: "grid",
        gap: 2,
        gridTemplateColumns: {
          xs: "1fr",
          md: "repeat(2, minmax(0, 1fr))",
          xl: "repeat(4, minmax(0, 1fr))",
        },
      }}
    >
      <SummaryCard
        title="Total Balance"
        value={formatCurrency(accountSummary.totalBalance)}
        description="Combined balance across all tracked and untracked accounts."
      />
      <SummaryCard
        title="Tracked Balance"
        value={formatCurrency(fundSummary.totalTrackedBalance)}
        description="Balance currently represented in fund tracking."
      />
      <SummaryCard
        title="Assigned Fund Balance"
        value={formatCurrency(fundSummary.totalAssignedBalance)}
        description="Tracked dollars already assigned outside the Unassigned fund."
      />
      <SummaryCard
        title="Accounting Periods"
        value={openAccountingPeriods.length}
        description={
          currentAccountingPeriod === null
            ? `${totalAccountingPeriods} total periods recorded.`
            : `${currentAccountingPeriod.name} is active across ${totalAccountingPeriods} total periods.`
        }
      />
    </Box>
  );
};

export default OverviewMetrics;
