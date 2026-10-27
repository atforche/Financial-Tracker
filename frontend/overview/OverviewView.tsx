import {
  type AccountTrendsDataMode,
  getAccountTrendsSnapshot,
} from "@/accounts/trends/helpers";
import type {
  AccountsInAccountingPeriodRange,
  AccountsInDateRange,
} from "@/accounts/types";
import {
  type FundTrendsDataMode,
  getFundTrendsSnapshot,
} from "@/funds/trends/helpers";
import type {
  FundsInAccountingPeriodRange,
  FundsInDateRange,
} from "@/funds/types";
import {
  type OverviewSearchParams,
  overviewParamNames,
} from "@/overview/helpers";
import { Stack, Typography } from "@mui/material";
import { buildUrl, objectToSearchParams } from "@/framework/routes/helpers";
import {
  matchesTrendRange,
  resolveTrendRange,
} from "@/framework/routes/trendRange";
import AccountBalanceSummaryCards from "@/accounts/AccountBalanceSummaryCards";
import { AccountingPeriodSort } from "@/accounting-periods/types";
import BalanceTrendChart from "@/framework/charts/BalanceTrendChart";
import BalanceTrendsChangeChart from "@/framework/charts/BalanceTrendsChangeChart";
import ConstrainedContent from "@/framework/view/ConstrainedContent";
import ContentSurface from "@/framework/view/ContentSurface";
import FundBalanceSummaryCards from "@/funds/FundBalanceSummaryCards";
import type { JSX } from "react";
import OverviewPageHeader from "@/overview/OverviewPageHeader";
import PageLayout from "@/framework/view/PageLayout";
import ResponsiveGrid from "@/framework/view/ResponsiveGrid";
import { buildBalanceTrendChartPoints } from "@/framework/charts/balanceTrendHelpers";
import createApiClient from "@/framework/data/createApiClient";
import { redirect } from "next/navigation";
import unwrapApiResponse from "@/framework/data/unwrapApiResponse";

/**
 * Props for the OverviewView component.
 */
interface OverviewViewProps {
  readonly searchParams: Promise<OverviewSearchParams>;
}

/**
 * Displays account and fund balance trends for one shared range.
 */
const OverviewView = async function ({
  searchParams,
}: OverviewViewProps): Promise<JSX.Element> {
  const apiClient = await createApiClient();
  const accountingPeriods = unwrapApiResponse(
    await apiClient.GET("/accounting-periods", {
      params: {
        query: {
          Sort: AccountingPeriodSort.DateDescending,
          Limit: 500,
          Offset: 0,
        },
      },
    }),
    "Failed to fetch accounting periods",
  );
  const rangeParams = await searchParams;
  const range = resolveTrendRange(rangeParams, accountingPeriods.items);
  // The default range needs no parameters, but supplied ones that were corrected are rewritten in the URL
  const hasRangeParams = Object.values(overviewParamNames).some(
    (name) => typeof rangeParams[name] !== "undefined",
  );
  if (hasRangeParams && !matchesTrendRange(rangeParams, range)) {
    redirect(buildUrl("/", objectToSearchParams(range)));
  }
  const currentMode = range.mode;
  const accountMode: AccountTrendsDataMode =
    currentMode === "date" ? "Date" : "AccountingPeriod";
  const fundMode: FundTrendsDataMode = accountMode;
  const query = {
    "Range.Start":
      range.mode === "date" ? range.startDate : range.startAccountingPeriodId,
    "Range.End":
      range.mode === "date" ? range.endDate : range.endAccountingPeriodId,
    Limit: 1,
    Offset: 0,
  };
  const [accountTrends, fundTrends] = await Promise.all([
    (async function (): Promise<
      AccountsInDateRange | AccountsInAccountingPeriodRange
    > {
      return currentMode === "date"
        ? unwrapApiResponse(
            await apiClient.GET("/accounts/date-range", {
              params: { query },
            }),
            "Failed to load account overview",
          )
        : unwrapApiResponse(
            await apiClient.GET("/accounts/accounting-period-range", {
              params: { query },
            }),
            "Failed to load account overview",
          );
    })(),
    (async function (): Promise<
      FundsInDateRange | FundsInAccountingPeriodRange
    > {
      return currentMode === "date"
        ? unwrapApiResponse(
            await apiClient.GET("/funds/date-range", {
              params: { query },
            }),
            "Failed to load fund overview",
          )
        : unwrapApiResponse(
            await apiClient.GET("/funds/accounting-period-range", {
              params: { query },
            }),
            "Failed to load fund overview",
          );
    })(),
  ]);
  const accountPeriods =
    "accountingPeriods" in accountTrends ? accountTrends.accountingPeriods : [];
  const accountDates = "dates" in accountTrends ? accountTrends.dates : [];
  const fundPeriods =
    "accountingPeriods" in fundTrends ? fundTrends.accountingPeriods : [];
  const fundDates = "dates" in fundTrends ? fundTrends.dates : [];
  const accountOpeningBalance =
    "dates" in accountTrends ? accountTrends.openingBalance : null;
  const fundOpeningBalance =
    "dates" in fundTrends ? fundTrends.openingBalance : null;
  const accountSnapshot = getAccountTrendsSnapshot(
    accountMode,
    accountPeriods,
    accountDates,
    accountOpeningBalance,
  );
  const fundSnapshot = getFundTrendsSnapshot(
    fundMode,
    fundPeriods,
    fundDates,
    fundOpeningBalance,
  );
  const xAxisLabel = currentMode === "date" ? "Date" : "Accounting Period";
  const accountChartPoints = buildBalanceTrendChartPoints({
    mode: accountMode,
    accountingPeriods: accountPeriods.map((summary) => ({
      accountingPeriodId: summary.accountingPeriod.id,
      accountingPeriodName: summary.accountingPeriod.name,
      year: summary.accountingPeriod.year,
      month: summary.accountingPeriod.month,
      totalOpeningBalance: summary.openingBalance.totalBalance,
      totalClosingBalance: summary.closingBalance.totalBalance,
    })),
    dates: accountDates,
  });
  const fundChartPoints = buildBalanceTrendChartPoints({
    mode: fundMode,
    accountingPeriods: fundPeriods.map((summary) => ({
      accountingPeriodId: summary.accountingPeriod.id,
      accountingPeriodName: summary.accountingPeriod.name,
      year: summary.accountingPeriod.year,
      month: summary.accountingPeriod.month,
      totalOpeningBalance: summary.openingBalance.totalBalance,
      totalClosingBalance: summary.closingBalance.totalBalance,
    })),
    dates: fundDates,
  });

  return (
    <PageLayout>
      <ConstrainedContent>
        <ContentSurface>
          <OverviewPageHeader
            accountingPeriods={accountingPeriods.items}
            range={range}
          />
        </ContentSurface>
      </ConstrainedContent>
      <Stack spacing={2}>
        <Typography variant="h5">Accounts</Typography>
        <AccountBalanceSummaryCards
          startingLabel={accountSnapshot.startLabel}
          endingLabel={accountSnapshot.endLabel}
          startingBalance={accountSnapshot.startingBalance}
          endingBalance={accountSnapshot.endingBalance}
        />
        <ResponsiveGrid columns={{ xs: 1, lg: 2 }}>
          <BalanceTrendChart
            chartPoints={accountChartPoints}
            title="Account Balance Trend"
            xAxisLabel={xAxisLabel}
          />
          <BalanceTrendsChangeChart
            mode={accountMode}
            accountingPeriods={accountPeriods}
            dates={accountDates}
            openingBalance={accountOpeningBalance?.totalBalance ?? null}
            title="Account Balance Change"
          />
        </ResponsiveGrid>
        <Typography variant="h5">Funds</Typography>
        <FundBalanceSummaryCards
          startingLabel={fundSnapshot.startLabel}
          endingLabel={fundSnapshot.endLabel}
          startingBalance={fundSnapshot.startingBalance}
          endingBalance={fundSnapshot.endingBalance}
        />
        <ResponsiveGrid columns={{ xs: 1, lg: 2 }}>
          <BalanceTrendChart
            chartPoints={fundChartPoints}
            title="Fund Balance Trend"
            xAxisLabel={xAxisLabel}
          />
          <BalanceTrendsChangeChart
            mode={fundMode}
            accountingPeriods={fundPeriods}
            dates={fundDates}
            openingBalance={fundOpeningBalance?.totalBalance ?? null}
            title="Fund Balance Change"
          />
        </ResponsiveGrid>
      </Stack>
    </PageLayout>
  );
};

export default OverviewView;
