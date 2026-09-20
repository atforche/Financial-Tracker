"use client";

import {
  type BalanceTrendDateSummary,
  buildDateChartPoints,
} from "@/framework/charts/balanceTrendHelpers";
import BalanceTrendChart from "@/framework/charts/BalanceTrendChart";
import { Button } from "@mui/material";
import ChangeValue from "@/framework/view/ChangeValue";
import type { JSX } from "react";
import Link from "next/link";
import PageLayout from "@/framework/view/PageLayout";
import type { Route } from "next";
import SummaryCard from "@/framework/view/SummaryCard";
import SummaryCardGrid from "@/framework/view/SummaryCardGrid";
import { formatCurrency } from "@/framework/currencyHelpers";

/**
 * Props for the RecentBalanceActivity component.
 */
interface RecentBalanceActivityProps {
  readonly totals: {
    readonly totalInflow: number;
    readonly totalOutflow: number;
  };
  readonly dailyBalances: readonly BalanceTrendDateSummary[];
  readonly periodOpeningBalance?: number;
  readonly title?: string;
  readonly balanceLabel?: string;
  readonly trendsHref?: Route;
  readonly summaryFirst?: boolean;
}

/**
 * Shows a recent balance trend and its most useful movement summaries.
 */
const RecentBalanceActivity = function ({
  totals,
  dailyBalances,
  periodOpeningBalance,
  title = "Recent Activity",
  balanceLabel = "Posted Balance",
  trendsHref,
  summaryFirst = false,
}: RecentBalanceActivityProps): JSX.Element {
  const firstBalance = periodOpeningBalance ?? dailyBalances[0]?.totalBalance;
  const lastBalance = dailyBalances.at(-1)?.totalBalance;
  const chartPoints = buildDateChartPoints(dailyBalances);

  const summaryCards = (
    <SummaryCardGrid>
      <SummaryCard
        title="Total Inflow"
        value={formatCurrency(totals.totalInflow)}
      />
      <SummaryCard
        title="Total Outflow"
        value={formatCurrency(totals.totalOutflow)}
      />
      <SummaryCard
        title="Net Change"
        value={
          firstBalance === undefined || lastBalance === undefined ? (
            "—"
          ) : (
            <ChangeValue
              startingValue={firstBalance}
              endingValue={lastBalance}
            />
          )
        }
      />
    </SummaryCardGrid>
  );

  return (
    <PageLayout>
      {summaryFirst ? summaryCards : null}
      <BalanceTrendChart
        title={title}
        headerContent={
          trendsHref === undefined ? undefined : (
            <Button
              component={Link}
              href={trendsHref}
              size="small"
              variant="outlined"
            >
              View full trends
            </Button>
          )
        }
        emptyMessage="No posted balance movement is available in this range."
        chartPoints={chartPoints}
        xAxisLabel="Date"
        yAxisLabel={balanceLabel}
      />
      {summaryFirst ? null : summaryCards}
    </PageLayout>
  );
};

export default RecentBalanceActivity;
