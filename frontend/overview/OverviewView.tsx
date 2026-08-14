import { Stack, Typography } from "@mui/material";
import { summarizeAccounts, summarizeFunds } from "@/overview/helpers";
import AccountOverview from "@/overview/AccountOverview";
import AccountingPeriodOverview from "@/overview/AccountingPeriodOverview";
import { AccountingPeriodSortModel } from "@/framework/data/api";
import BalanceTrendChart from "@/framework/charts/BalanceTrendChart";
import ConstrainedContent from "@/framework/view/ConstrainedContent";
import ContentSurface from "@/framework/view/ContentSurface";
import FundOverview from "@/overview/FundOverview";
import type { JSX } from "react";
import type { OverviewData } from "@/overview/types";
import OverviewPageHeader from "@/overview/OverviewPageHeader";
import PageLayout from "@/framework/view/PageLayout";
import ResponsiveGrid from "@/framework/view/ResponsiveGrid";
import { buildDateChartPoints } from "@/framework/charts/balanceTrendHelpers";
import createApiClient from "@/framework/data/createApiClient";
import dayjs from "dayjs";
import loadAllPages from "@/framework/data/loadAllPages";
import unwrapApiResponse from "@/framework/data/unwrapApiResponse";

/**
 * Loads all data required by the overview page.
 */
const getOverviewData = async function (): Promise<OverviewData> {
  const apiClient = await createApiClient();
  const recentActivityEndDate = dayjs().format("YYYY-MM-DD");
  const recentActivityStartDate = dayjs()
    .subtract(60, "day")
    .format("YYYY-MM-DD");
  const recentActivityRange = {
    "Range.Start": recentActivityStartDate,
    "Range.End": recentActivityEndDate,
    Limit: 1,
    Offset: 0,
  };
  const accountSummaryPromise = apiClient.GET("/accounts/with-balances");
  const fundSummaryPromise = apiClient.GET("/funds/with-balances");
  const accountTrendPromise = apiClient.GET("/accounts/date-range", {
    params: { query: recentActivityRange },
  });
  const fundTrendPromise = apiClient.GET("/funds/date-range", {
    params: { query: recentActivityRange },
  });
  const accountingPeriodsPromise = loadAllPages(async (limit, offset) =>
    unwrapApiResponse(
      await apiClient.GET("/accounting-periods", {
        params: {
          query: {
            Sort: AccountingPeriodSortModel.DateDescending,
            Limit: limit,
            Offset: offset,
          },
        },
      }),
      "Failed to fetch accounting periods",
    ),
  );

  const [
    accountSummaryResponse,
    fundSummaryResponse,
    accountTrendResponse,
    fundTrendResponse,
    accountingPeriods,
  ] = await Promise.all([
    accountSummaryPromise,
    fundSummaryPromise,
    accountTrendPromise,
    fundTrendPromise,
    accountingPeriodsPromise,
  ]);

  const accounts = unwrapApiResponse(
    accountSummaryResponse,
    "Failed to fetch account summary",
  );
  const funds = unwrapApiResponse(
    fundSummaryResponse,
    "Failed to fetch fund summary",
  );
  const accountTrend = unwrapApiResponse(
    accountTrendResponse,
    "Failed to fetch account balance trend",
  );
  const fundTrend = unwrapApiResponse(
    fundTrendResponse,
    "Failed to fetch fund balance trend",
  );

  return {
    accountSummary: summarizeAccounts(accounts.items),
    fundSummary: summarizeFunds(funds.items),
    accountBalanceTrend: buildDateChartPoints(accountTrend.dates),
    fundBalanceTrend: buildDateChartPoints(fundTrend.dates),
    latestAccountingPeriod: accountingPeriods[0] ?? null,
    currentAccountingPeriod:
      accountingPeriods.find((period) => period.isOpen) ?? null,
  };
};

/**
 * Component that displays the Overview view.
 */
const OverviewView = async function (): Promise<JSX.Element> {
  const data = await getOverviewData();
  const displayedAccountingPeriod =
    data.currentAccountingPeriod ?? data.latestAccountingPeriod;

  return (
    <PageLayout>
      <ConstrainedContent>
        <ContentSurface>
          <OverviewPageHeader title="Overview" />
        </ContentSurface>
      </ConstrainedContent>

      <Stack spacing={2}>
        <Typography variant="h5">
          Current Period
          {displayedAccountingPeriod === null
            ? ""
            : ` (${displayedAccountingPeriod.name})`}
        </Typography>
        <AccountingPeriodOverview
          currentAccountingPeriod={data.currentAccountingPeriod}
          latestAccountingPeriod={data.latestAccountingPeriod}
        />
        <Typography variant="h5">Recent Activity</Typography>
        <ResponsiveGrid columns={{ xs: 1, md: 2 }}>
          <AccountOverview summary={data.accountSummary} />
          <FundOverview summary={data.fundSummary} />
        </ResponsiveGrid>
        <ResponsiveGrid columns={{ xs: 1, lg: 2 }}>
          <BalanceTrendChart
            chartPoints={data.accountBalanceTrend}
            title="Account Balance Trend"
            xAxisLabel="Date"
          />
          <BalanceTrendChart
            chartPoints={data.fundBalanceTrend}
            title="Fund Balance Trend"
            xAxisLabel="Date"
          />
        </ResponsiveGrid>
      </Stack>
    </PageLayout>
  );
};

export default OverviewView;
