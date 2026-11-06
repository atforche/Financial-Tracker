import { Button, Chip, Stack, Typography } from "@mui/material";
import type { JSX, ReactNode } from "react";
import {
  formatCurrency,
  getCurrencyDifference,
} from "@/framework/currencyHelpers";
import BalanceTrendChart from "@/framework/charts/BalanceTrendChart";
import Link from "next/link";
import PageLayout from "@/framework/view/PageLayout";
import PeriodNavigation from "@/framework/view/PeriodNavigation";
import ResponsiveGrid from "@/framework/view/ResponsiveGrid";
import ReturnLink from "@/framework/view/ReturnLink";
import type { Route } from "next";
import SignedChange from "@/framework/view/SignedChange";
import StatCard from "@/framework/view/StatCard";
import { buildDateChartPoints } from "@/framework/charts/balanceTrendHelpers";
import type { components } from "@/framework/data/api";

/**
 * Props for the PeriodEntityLayout component.
 */
interface PeriodEntityLayoutProps {
  readonly name: string;
  readonly detail: Pick<
    components["schemas"]["PeriodFundDetailModel"],
    | "accountingPeriod"
    | "previousPeriod"
    | "nextPeriod"
    | "totalInflow"
    | "totalOutflow"
    | "dailyBalances"
  >;
  readonly getPeriodHref: (accountingPeriodId: string) => Route;
  readonly periodHref: Route;
  readonly returnUrl: string | undefined;
  readonly allPeriodsHref: Route;
  readonly openingBalance: number;
  readonly closingBalance: number;
  readonly isIncreaseGood?: boolean;
  readonly goalCard: ReactNode;
  readonly balanceLabel: string;
  readonly transactions: ReactNode;
}

/**
 * Lays out one account or fund within one accounting period: its balances, goal, balance over the period, and
 * transactions.
 */
const PeriodEntityLayout = function ({
  name,
  detail,
  getPeriodHref,
  periodHref,
  returnUrl,
  allPeriodsHref,
  openingBalance,
  closingBalance,
  isIncreaseGood = true,
  goalCard,
  balanceLabel,
  transactions,
}: PeriodEntityLayoutProps): JSX.Element {
  const period = detail.accountingPeriod;
  const getNeighborLink = (
    neighbor: typeof detail.previousPeriod,
  ): { name: string; href: Route } | null =>
    neighbor === null
      ? null
      : { name: neighbor.name, href: getPeriodHref(neighbor.id) };
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
              previousPeriod={getNeighborLink(detail.previousPeriod)}
              nextPeriod={getNeighborLink(detail.nextPeriod)}
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
            value={formatCurrency(detail.totalInflow)}
            caption="posted this period"
          />
          <StatCard
            label="Total outflow"
            value={formatCurrency(detail.totalOutflow)}
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
          chartPoints={buildDateChartPoints(detail.dailyBalances.dates)}
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
