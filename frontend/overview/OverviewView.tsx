import {
  type OverviewSearchParams,
  type OverviewSection,
  buildDateSection,
  buildPeriodSection,
  overviewParamNames,
} from "@/overview/helpers";
import { buildUrl, objectToSearchParams } from "@/framework/routes/helpers";
import {
  matchesTrendRange,
  resolveTrendRange,
} from "@/framework/routes/trendRange";
import type { AccountBalanceSummary } from "@/accounts/types";
import AccountBalanceSummaryCards from "@/accounts/AccountBalanceSummaryCards";
import { AccountingPeriodSort } from "@/accounting-periods/types";
import BalanceTrendChart from "@/framework/charts/BalanceTrendChart";
import BalanceTrendsChangeChart from "@/framework/charts/BalanceTrendsChangeChart";
import ConstrainedContent from "@/framework/view/ConstrainedContent";
import ContentSurface from "@/framework/view/ContentSurface";
import type { FundBalanceSummary } from "@/funds/types";
import FundBalanceSummaryCards from "@/funds/FundBalanceSummaryCards";
import type { JSX } from "react";
import OverviewPageHeader from "@/overview/OverviewPageHeader";
import OverviewSectionHeading from "@/overview/OverviewSectionHeading";
import PageLayout from "@/framework/view/PageLayout";
import ResponsiveGrid from "@/framework/view/ResponsiveGrid";
import { Stack } from "@mui/material";
import accountRoutes from "@/accounts/routes";
import createApiClient from "@/framework/data/createApiClient";
import fundRoutes from "@/funds/routes";
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
  const chartMode = range.mode === "date" ? "Date" : "AccountingPeriod";
  const { accounts, funds } = await (async function (): Promise<{
    readonly accounts: OverviewSection<AccountBalanceSummary>;
    readonly funds: OverviewSection<FundBalanceSummary>;
  }> {
    if (range.mode === "date") {
      const overview = unwrapApiResponse(
        await apiClient.GET("/overview/date-range", {
          params: {
            query: {
              "Range.Start": range.startDate,
              "Range.End": range.endDate,
            },
          },
        }),
        "Failed to load overview",
      );
      return {
        accounts: buildDateSection({
          balances: overview.accountBalances,
          openingBalance: overview.accountOpeningBalance,
          closingBalance: overview.accountClosingBalance,
          startDate: range.startDate,
          endDate: range.endDate,
          granularity: overview.granularity,
        }),
        funds: buildDateSection({
          balances: overview.fundBalances,
          openingBalance: overview.fundOpeningBalance,
          closingBalance: overview.fundClosingBalance,
          startDate: range.startDate,
          endDate: range.endDate,
          granularity: overview.granularity,
        }),
      };
    }
    const overview = unwrapApiResponse(
      await apiClient.GET("/overview/accounting-period-range", {
        params: {
          query: {
            "Range.Start": range.startAccountingPeriodId,
            "Range.End": range.endAccountingPeriodId,
          },
        },
      }),
      "Failed to load overview",
    );
    return {
      accounts: buildPeriodSection({
        balances: overview.accountBalances,
        openingBalance: overview.accountOpeningBalance,
        closingBalance: overview.accountClosingBalance,
      }),
      funds: buildPeriodSection({
        balances: overview.fundBalances,
        openingBalance: overview.fundOpeningBalance,
        closingBalance: overview.fundClosingBalance,
      }),
    };
  })();
  const xAxisLabel = range.mode === "date" ? "Date" : "Accounting Period";

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
        <OverviewSectionHeading
          title="Accounts"
          trendsHref={accountRoutes.trends(range)}
        />
        <AccountBalanceSummaryCards
          startingLabel={accounts.startLabel}
          endingLabel={accounts.endLabel}
          startingBalance={accounts.openingBalance}
          endingBalance={accounts.closingBalance}
        />
        <ResponsiveGrid columns={{ xs: 1, lg: 2 }}>
          <BalanceTrendChart
            chartPoints={accounts.trendPoints}
            title="Account Balance Trend"
            xAxisLabel={xAxisLabel}
          />
          <BalanceTrendsChangeChart
            mode={chartMode}
            accountingPeriods={accounts.changePeriods}
            dates={accounts.changeDates}
            openingBalance={accounts.openingBalance.totalBalance}
            granularity={accounts.granularity}
            title="Account Balance Change"
          />
        </ResponsiveGrid>
        <OverviewSectionHeading
          title="Funds"
          trendsHref={fundRoutes.trends(range)}
        />
        <FundBalanceSummaryCards
          startingLabel={funds.startLabel}
          endingLabel={funds.endLabel}
          startingBalance={funds.openingBalance}
          endingBalance={funds.closingBalance}
        />
        <ResponsiveGrid columns={{ xs: 1, lg: 2 }}>
          <BalanceTrendChart
            chartPoints={funds.trendPoints}
            title="Fund Balance Trend"
            xAxisLabel={xAxisLabel}
          />
          <BalanceTrendsChangeChart
            mode={chartMode}
            accountingPeriods={funds.changePeriods}
            dates={funds.changeDates}
            openingBalance={funds.openingBalance.totalBalance}
            granularity={funds.granularity}
            title="Fund Balance Change"
          />
        </ResponsiveGrid>
      </Stack>
    </PageLayout>
  );
};

export default OverviewView;
