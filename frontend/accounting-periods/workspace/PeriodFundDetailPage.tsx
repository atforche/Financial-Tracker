import { Button, Chip, Stack, Typography } from "@mui/material";
import {
  formatCurrency,
  getCurrencyDifference,
} from "@/framework/currencyHelpers";
import BalanceTrendChart from "@/framework/charts/BalanceTrendChart";
import { FundGoalBalanceEventSortModel } from "@/framework/data/api";
import type { JSX } from "react";
import PageLayout from "@/framework/view/PageLayout";
import PeriodFundGoalCard from "@/accounting-periods/workspace/PeriodFundGoalCard";
import PeriodFundTransactions from "@/accounting-periods/workspace/PeriodFundTransactions";
import PeriodNavigation from "@/framework/view/PeriodNavigation";
import ResponsiveGrid from "@/framework/view/ResponsiveGrid";
import ReturnLink from "@/framework/view/ReturnLink";
import SignedChange from "@/framework/view/SignedChange";
import StatCard from "@/framework/view/StatCard";
import { buildDateChartPoints } from "@/framework/charts/balanceTrendHelpers";
import createApiClient from "@/framework/data/createApiClient";
import fundRoutes from "@/funds/routes";
import { redirect } from "next/navigation";
import routes from "@/accounting-periods/routes";
import transactionRoutes from "@/transactions/routes";
import unwrapApiResponse from "@/framework/data/unwrapApiResponse";

/**
 * Search parameters supported by the page for one fund in one accounting period.
 */
interface PeriodFundSearchParams {
  returnUrl?: string;
}

/**
 * Props for the PeriodFundDetailPage component.
 */
interface PeriodFundDetailPageProps {
  readonly params: Promise<{ accountingPeriodId: string; fundId: string }>;
  readonly searchParams: Promise<PeriodFundSearchParams>;
}

/**
 * Displays one fund within one accounting period: its balances, goal, daily balance, and that period's transactions.
 */
const PeriodFundDetailPage = async function ({
  params,
  searchParams,
}: PeriodFundDetailPageProps): Promise<JSX.Element> {
  const { accountingPeriodId, fundId } = await params;
  const resolvedSearchParams = await searchParams;
  const apiClient = await createApiClient();
  const detailResponse = await apiClient.GET(
    "/accounting-periods/{accountingPeriodId}/detail",
    { params: { path: { accountingPeriodId } } },
  );
  if (detailResponse.response.status === 404) {
    redirect(routes.workspace({}));
  }
  const detail = unwrapApiResponse(
    detailResponse,
    "Failed to fetch accounting period",
  );
  const period = detail.accountingPeriod;
  const periodUrl = routes.workspaceDetail(period.id, {});
  const item = detail.funds.find(({ fund }) => fund.id === fundId);
  if (item === undefined) {
    redirect(periodUrl);
  }
  const periodRange = { "Range.Start": period.id, "Range.End": period.id };
  const [goalResponse, eventsResponse, totalsResponse, balanceDatesResponse] =
    await Promise.all([
      apiClient.GET("/fund-goals/fund/{fundId}", {
        params: {
          path: { fundId },
          query: { accountingPeriodId: period.id },
        },
      }),
      apiClient.GET("/fund-goals/balance-events/accounting-period-range", {
        params: {
          query: {
            ...periodRange,
            "Filter.FundIds": [fundId],
            Sort: FundGoalBalanceEventSortModel.DateDescending,
          },
        },
      }),
      apiClient.GET(
        "/fund-goals/balance-events/accounting-period-range/totals",
        {
          params: { query: { ...periodRange, "Filter.FundIds": [fundId] } },
        },
      ),
      apiClient.GET(
        "/funds/{fundId}/accounting-periods/{accountingPeriodId}/balance-dates",
        { params: { path: { fundId, accountingPeriodId: period.id } } },
      ),
    ]);
  const goal =
    goalResponse.response.status === 404
      ? null
      : unwrapApiResponse(goalResponse, "Failed to fetch the fund goal");
  const events = unwrapApiResponse(
    eventsResponse,
    "Failed to fetch fund balance events",
  );
  const totals = unwrapApiResponse(
    totalsResponse,
    "Failed to fetch fund activity totals",
  );
  const balanceDates = unwrapApiResponse(
    balanceDatesResponse,
    "Failed to fetch fund daily balances",
  );
  const currentUrl = routes.fundDetail(period.id, fundId, resolvedSearchParams);
  const { returnUrl } = resolvedSearchParams;
  const neighborParams = returnUrl === undefined ? {} : { returnUrl };
  const getNeighborLink = function (
    neighbor: typeof detail.previousPeriod,
  ): { name: string; href: ReturnType<typeof routes.fundDetail> } | null {
    return neighbor === null
      ? null
      : {
          name: neighbor.name,
          href: routes.fundDetail(neighbor.id, fundId, neighborParams),
        };
  };
  const opening = item.openingBalance;
  const closing = item.closingBalance;

  return (
    <PageLayout>
      <Stack spacing={2}>
        <ReturnLink
          returnUrl={returnUrl}
          fallback={{ href: periodUrl, label: `Back to ${period.name}` }}
        />
        <Stack spacing={0.5}>
          <Typography variant="h4" noWrap>
            {item.fund.name}
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
            <Button
              size="small"
              href={fundRoutes.workspaceDetail(fundId, {})}
              sx={{ whiteSpace: "nowrap" }}
            >
              All Periods
            </Button>
          </Stack>
        </Stack>
      </Stack>
      <Stack spacing={2}>
        <ResponsiveGrid columns={{ xs: 1, sm: 2, lg: 4 }} spacing={2}>
          <StatCard
            label="Opening balance"
            value={formatCurrency(opening)}
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
            value={formatCurrency(closing)}
            caption={
              <>
                <SignedChange
                  amount={getCurrencyDifference(closing, opening)}
                />{" "}
                this period
              </>
            }
          />
        </ResponsiveGrid>
        {goal === null ||
        item.goalProgress === null ||
        item.goalProgress === undefined ? null : (
          <PeriodFundGoalCard
            goal={goal}
            progress={item.goalProgress}
            accountingPeriod={period}
            redirectUrl={currentUrl}
          />
        )}
        <BalanceTrendChart
          chartPoints={buildDateChartPoints(balanceDates.dates)}
          title="Accounting Period Balance"
          variant="surface"
          emptyMessage="No posted balance movement is available in this period."
          xAxisLabel="Date"
          yAxisLabel="Fund Balance"
        />
        <PeriodFundTransactions
          events={events.items}
          addTransactionHref={transactionRoutes.workspaceCreate({
            accountingPeriodIds: [period.id],
            fundIds: [fundId],
            returnUrl: currentUrl,
          })}
          returnUrl={currentUrl}
        />
      </Stack>
    </PageLayout>
  );
};

export type { PeriodFundSearchParams };
export default PeriodFundDetailPage;
