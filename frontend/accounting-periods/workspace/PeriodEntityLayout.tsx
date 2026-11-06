import {
  type BalanceTrendDateSummary,
  buildDateChartPoints,
} from "@/framework/charts/balanceTrendHelpers";
import { Button, Chip, Stack, Typography } from "@mui/material";
import type { JSX, ReactNode } from "react";
import {
  formatCurrency,
  getCurrencyDifference,
} from "@/framework/currencyHelpers";
import type { BalanceEventTotals } from "@/balance-events/types";
import BalanceTrendChart from "@/framework/charts/BalanceTrendChart";
import Link from "next/link";
import PageLayout from "@/framework/view/PageLayout";
import PeriodNavigation from "@/framework/view/PeriodNavigation";
import ResponsiveGrid from "@/framework/view/ResponsiveGrid";
import ReturnLink from "@/framework/view/ReturnLink";
import type { Route } from "next";
import SignedChange from "@/framework/view/SignedChange";
import StatCard from "@/framework/view/StatCard";

/**
 * A link to the same account or fund in an adjacent period.
 */
interface NeighborLink {
  readonly name: string;
  readonly href: Route;
}

/**
 * Props for the PeriodEntityLayout component.
 */
interface PeriodEntityLayoutProps {
  readonly name: string;
  readonly period: { readonly name: string; readonly isOpen: boolean };
  readonly previousPeriod: NeighborLink | null;
  readonly nextPeriod: NeighborLink | null;
  readonly periodHref: Route;
  readonly returnUrl: string | undefined;
  readonly allPeriodsHref: Route;
  readonly openingBalance: number;
  readonly closingBalance: number;
  readonly totals: BalanceEventTotals;
  readonly isIncreaseGood?: boolean;
  readonly goalCard: ReactNode;
  readonly dailyBalances: readonly BalanceTrendDateSummary[];
  readonly balanceLabel: string;
  readonly transactions: ReactNode;
}

/**
 * Lays out one account or fund within one accounting period: its balances, goal, balance over the period, and
 * transactions.
 */
const PeriodEntityLayout = function ({
  name,
  period,
  previousPeriod,
  nextPeriod,
  periodHref,
  returnUrl,
  allPeriodsHref,
  openingBalance,
  closingBalance,
  totals,
  isIncreaseGood = true,
  goalCard,
  dailyBalances,
  balanceLabel,
  transactions,
}: PeriodEntityLayoutProps): JSX.Element {
  return (
    <PageLayout>
      <Stack spacing={2}>
        <ReturnLink
          returnUrl={returnUrl}
          fallback={{ href: periodHref, label: `Back to ${period.name}` }}
        />
        <Stack spacing={0.5}>
          <Typography variant="h4" noWrap>
            {name}
          </Typography>
          <Stack
            direction="row"
            useFlexGap
            spacing={1}
            sx={{ alignItems: "center", flexWrap: "wrap", ml: -1 }}
          >
            <PeriodNavigation
              previousPeriod={previousPeriod}
              nextPeriod={nextPeriod}
            >
              {/* Wide enough for the longest period name so the arrows stay in place between periods */}
              <Typography
                variant="subtitle1"
                noWrap
                sx={{ width: "9.5rem", textAlign: "center" }}
              >
                {period.name}
              </Typography>
            </PeriodNavigation>
            <Chip
              label={period.isOpen ? "Open" : "Closed"}
              color={period.isOpen ? "success" : "default"}
              size="small"
              variant="outlined"
            />
            <Link href={allPeriodsHref} style={{ textDecoration: "none" }}>
              <Button
                component="span"
                size="small"
                sx={{ whiteSpace: "nowrap" }}
              >
                All Periods
              </Button>
            </Link>
          </Stack>
        </Stack>
      </Stack>
      <Stack spacing={2}>
        <ResponsiveGrid columns={{ xs: 1, sm: 2, lg: 4 }} spacing={2}>
          <StatCard
            label="Opening balance"
            value={formatCurrency(openingBalance)}
            caption="at the start of the period"
          />
          <StatCard
            label="Total inflow"
            value={formatCurrency(totals.totalInflow)}
            caption="posted this period"
          />
          <StatCard
            label="Total outflow"
            value={formatCurrency(totals.totalOutflow)}
            caption="posted this period"
          />
          <StatCard
            label="Closing balance"
            value={formatCurrency(closingBalance)}
            caption={
              <>
                <SignedChange
                  amount={getCurrencyDifference(closingBalance, openingBalance)}
                  isIncreaseGood={isIncreaseGood}
                />{" "}
                this period
              </>
            }
          />
        </ResponsiveGrid>
        {goalCard}
        <BalanceTrendChart
          chartPoints={buildDateChartPoints(dailyBalances)}
          title="Accounting Period Balance"
          variant="surface"
          emptyMessage="No posted balance movement is available in this period."
          xAxisLabel="Date"
          yAxisLabel={balanceLabel}
        />
        {transactions}
      </Stack>
    </PageLayout>
  );
};

export default PeriodEntityLayout;
