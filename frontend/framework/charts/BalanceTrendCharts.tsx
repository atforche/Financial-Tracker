import { BalanceGranularityModel } from "@/framework/data/api";
import BalanceTrendChart from "@/framework/charts/BalanceTrendChart";
import BalanceTrendsChangeChart from "@/framework/charts/BalanceTrendsChangeChart";
import type { JSX } from "react";
import ResponsiveGrid from "@/framework/view/ResponsiveGrid";
import { buildBalanceTrendChartPoints } from "@/framework/charts/balanceTrendHelpers";

/**
 * The balance of an account or fund trends range, across either periods or dates.
 */
type BalanceTrendRange =
  | {
      readonly accountingPeriods: readonly {
        readonly accountingPeriod: {
          readonly id: string;
          readonly name: string;
          readonly year: number;
          readonly month: number;
        };
        readonly openingBalance: { readonly totalBalance: number };
        readonly closingBalance: { readonly totalBalance: number };
      }[];
    }
  | {
      readonly dates: readonly {
        readonly date: string;
        readonly totalBalance: number;
      }[];
      readonly openingBalance?: { readonly totalBalance: number } | null;
      readonly granularity: BalanceGranularityModel;
    };

/**
 * Displays a trends range's balance beside how much it changed at each step.
 */
const BalanceTrendCharts = function ({
  trends,
}: {
  readonly trends: BalanceTrendRange;
}): JSX.Element {
  const isDateRange = "dates" in trends;
  const accountingPeriods = isDateRange ? [] : trends.accountingPeriods;
  const dates = isDateRange ? trends.dates : [];
  const granularity = isDateRange
    ? trends.granularity
    : BalanceGranularityModel.Day;
  const mode = isDateRange ? "Date" : "AccountingPeriod";
  return (
    <ResponsiveGrid columns={{ xs: 1, lg: 2 }}>
      <BalanceTrendChart
        chartPoints={buildBalanceTrendChartPoints({
          mode,
          accountingPeriods: accountingPeriods.map((summary) => ({
            accountingPeriodId: summary.accountingPeriod.id,
            accountingPeriodName: summary.accountingPeriod.name,
            year: summary.accountingPeriod.year,
            month: summary.accountingPeriod.month,
            totalOpeningBalance: summary.openingBalance.totalBalance,
            totalClosingBalance: summary.closingBalance.totalBalance,
          })),
          dates,
          granularity,
        })}
        title="Balance"
        xAxisLabel={isDateRange ? "Date" : "Accounting Period"}
      />
      <BalanceTrendsChangeChart
        mode={mode}
        accountingPeriods={accountingPeriods}
        dates={dates}
        openingBalance={
          isDateRange ? (trends.openingBalance?.totalBalance ?? null) : null
        }
        granularity={granularity}
      />
    </ResponsiveGrid>
  );
};

export default BalanceTrendCharts;
