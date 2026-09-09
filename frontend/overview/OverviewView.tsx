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
import { Stack, Typography } from "@mui/material";
import {
  type TrendRangeMode,
  getDefaultTrendAccountingPeriodRange,
  getDefaultTrendDateRange,
} from "@/framework/routes/trendRange";
import AccountBalanceSummaryCards from "@/accounts/AccountBalanceSummaryCards";
import AccountTrendsChangeChart from "@/accounts/trends/AccountTrendsChangeChart";
import { AccountingPeriodSort } from "@/accounting-periods/types";
import BalanceTrendChart from "@/framework/charts/BalanceTrendChart";
import ConstrainedContent from "@/framework/view/ConstrainedContent";
import ContentSurface from "@/framework/view/ContentSurface";
import FundBalanceSummaryCards from "@/funds/FundBalanceSummaryCards";
import FundTrendsChangeChart from "@/funds/trends/FundTrendsChangeChart";
import type { JSX } from "react";
import OverviewPageHeader from "@/overview/OverviewPageHeader";
import type { OverviewSearchParams } from "@/overview/helpers";
import PageLayout from "@/framework/view/PageLayout";
import ResponsiveGrid from "@/framework/view/ResponsiveGrid";
import { buildBalanceTrendChartPoints } from "@/framework/charts/balanceTrendHelpers";
import createApiClient from "@/framework/data/createApiClient";
import unwrapApiResponse from "@/framework/data/unwrapApiResponse";

interface OverviewViewProps {
  readonly searchParams: Promise<OverviewSearchParams>;
}

/** Displays account and fund balance trends for one shared range. */
const OverviewView = async function ({
  searchParams,
}: OverviewViewProps): Promise<JSX.Element> {
  const {
    mode,
    startAccountingPeriodId,
    endAccountingPeriodId,
    startDate,
    endDate,
  } = await searchParams;
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
  const defaultDateRange = getDefaultTrendDateRange();
  const defaultAccountingPeriodRange = getDefaultTrendAccountingPeriodRange(
    accountingPeriods.items,
  );
  const currentMode: TrendRangeMode =
    mode === "accounting-period" && defaultAccountingPeriodRange !== null
      ? "accounting-period"
      : "date";
  const accountMode: AccountTrendsDataMode =
    currentMode === "date" ? "Date" : "AccountingPeriod";
  const fundMode: FundTrendsDataMode = accountMode;
  const range =
    currentMode === "date"
      ? {
          "Range.Start": startDate ?? defaultDateRange.start,
          "Range.End": endDate ?? defaultDateRange.end,
        }
      : {
          "Range.Start":
            startAccountingPeriodId ??
            defaultAccountingPeriodRange?.start ??
            "",
          "Range.End":
            endAccountingPeriodId ?? defaultAccountingPeriodRange?.end ?? "",
        };
  const query = { ...range, Limit: 1, Offset: 0 };
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
  const accountSnapshot = getAccountTrendsSnapshot(
    accountMode,
    accountPeriods,
    accountDates,
  );
  const fundSnapshot = getFundTrendsSnapshot(fundMode, fundPeriods, fundDates);
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
          <OverviewPageHeader accountingPeriods={accountingPeriods.items} />
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
          <AccountTrendsChangeChart
            mode={accountMode}
            accountingPeriods={accountPeriods}
            dates={accountDates}
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
          <FundTrendsChangeChart
            mode={fundMode}
            accountingPeriods={fundPeriods}
            dates={fundDates}
            title="Fund Balance Change"
          />
        </ResponsiveGrid>
      </Stack>
    </PageLayout>
  );
};

export default OverviewView;
