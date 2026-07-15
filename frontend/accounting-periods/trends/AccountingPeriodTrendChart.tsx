"use client";

import type { AccountingPeriodWithBalance } from "@/accounting-periods/types";
import BalanceTrendChart from "@/framework/charts/BalanceTrendChart";
import type { JSX } from "react";
import { buildChartPoints } from "@/accounting-periods/trends/helpers";

/**
 * Props for the AccountingPeriodTrendChart component.
 */
interface AccountingPeriodTrendChartProps {
  readonly accountingPeriods: readonly AccountingPeriodWithBalance[] | null;
}

/**
 * Renders the balance trend for the accounting periods trends.
 */
const AccountingPeriodTrendChart = function ({
  accountingPeriods,
}: AccountingPeriodTrendChartProps): JSX.Element {
  return (
    <BalanceTrendChart
      chartPoints={buildChartPoints(accountingPeriods ?? [])}
      color="secondary"
      xAxisLabel="Accounting Period"
    />
  );
};

export default AccountingPeriodTrendChart;
