import { Box, IconButton, Stack, Typography } from "@mui/material";
import OverviewAddTransactionButton, {
  OverviewAddTransactionButtonSpacer,
} from "@/overview/OverviewAddTransactionButton";
import {
  type OverviewSearchParams,
  getOverviewUrl,
  parseOverviewHistoryRange,
} from "@/overview/helpers";
import {
  formatCurrency,
  getCurrencyDifference,
} from "@/framework/currencyHelpers";
import BalanceTrendChart from "@/framework/charts/BalanceTrendChart";
import ChevronRight from "@mui/icons-material/ChevronRight";
import type { JSX } from "react";
import Link from "next/link";
import OverviewAttentionList from "@/overview/OverviewAttentionList";
import { OverviewHistoryRangeModel } from "@/framework/data/api";
import OverviewHistoryRangeSelector from "@/overview/OverviewHistoryRangeSelector";
import OverviewPeriodCard from "@/overview/OverviewPeriodCard";
import PageLayout from "@/framework/view/PageLayout";
import ResponsiveGrid from "@/framework/view/ResponsiveGrid";
import SignedChange from "@/framework/view/SignedChange";
import StatCard from "@/framework/view/StatCard";
import accountRoutes from "@/accounts/routes";
import { buildDateChartPoints } from "@/framework/charts/balanceTrendHelpers";
import createApiClient from "@/framework/data/createApiClient";
import dayjs from "dayjs";
import fundRoutes from "@/funds/routes";
import { getAccountTypeStatDetails } from "@/accounts/balanceSummaryHelpers";
import unwrapApiResponse from "@/framework/data/unwrapApiResponse";

/**
 * Props for the OverviewView component.
 */
interface OverviewViewProps {
  readonly searchParams: Promise<OverviewSearchParams>;
}

/**
 * Describes each history length in the net worth change caption.
 */
const historyDescriptions = {
  [OverviewHistoryRangeModel.ThreeMonths]: "the last 3 months",
  [OverviewHistoryRangeModel.OneYear]: "the last year",
  [OverviewHistoryRangeModel.FiveYears]: "the last 5 years",
} as const satisfies Record<OverviewHistoryRangeModel, string>;

/**
 * Stretches a section's surface to the height of its row so side-by-side sections line up.
 */
const fillHeightSx = {
  display: "flex",
  flexDirection: "column",
  "& > .MuiPaper-root": { flex: 1 },
} as const;

/**
 * Displays where balances stand today, what needs attention, the current accounting period, and balance history.
 */
const OverviewView = async function ({
  searchParams,
}: OverviewViewProps): Promise<JSX.Element> {
  const history = parseOverviewHistoryRange((await searchParams).history);
  const asOf = dayjs().format("YYYY-MM-DD");
  const apiClient = await createApiClient();
  const overview = unwrapApiResponse(
    await apiClient.GET("/overview", {
      params: { query: { AsOf: asOf, History: history } },
    }),
    "Failed to load overview",
  );
  const currentPeriod = overview.currentPeriod ?? null;
  const opening = overview.accountOpeningBalance;
  const closing = overview.accountClosingBalance;
  const describeChange = function (
    current: number,
    previous: number,
  ): JSX.Element {
    return (
      <>
        <SignedChange amount={getCurrencyDifference(current, previous)} /> over{" "}
        {historyDescriptions[history]}
      </>
    );
  };
  const overviewUrl = getOverviewUrl(history);
  const trendsRange = {
    mode: "date",
    startDate: overview.historyStart,
    endDate: asOf,
    returnUrl: overviewUrl,
  } as const;

  return (
    <PageLayout>
      <Stack
        direction="row"
        useFlexGap
        spacing={2}
        // Keeps the range selector usable while a new range loads
        data-page-filters
        sx={{
          flexWrap: "wrap",
          alignItems: "center",
          justifyContent: "space-between",
        }}
      >
        <Typography variant="h4" noWrap sx={{ maxWidth: "100%" }}>
          Overview
        </Typography>
        <Stack
          direction="row"
          useFlexGap
          spacing={1.5}
          sx={{ flexWrap: "wrap", alignItems: "center" }}
        >
          <OverviewHistoryRangeSelector value={history} />
          <OverviewAddTransactionButton returnUrl={overviewUrl} />
        </Stack>
      </Stack>
      <ResponsiveGrid columns={{ xs: 1, sm: 2, lg: 4 }}>
        <StatCard
          label="Net worth"
          value={formatCurrency(closing.totalBalance)}
          caption={describeChange(closing.totalBalance, opening.totalBalance)}
        />
        <StatCard
          label="Tracked money"
          value={formatCurrency(closing.totalTrackedBalance)}
          caption={describeChange(
            closing.totalTrackedBalance,
            opening.totalTrackedBalance,
          )}
          details={getAccountTypeStatDetails(opening, closing, true)}
          detailsLabel="by account type"
        />
        <StatCard
          label="Untracked money"
          value={formatCurrency(closing.totalUntrackedBalance)}
          caption={describeChange(
            closing.totalUntrackedBalance,
            opening.totalUntrackedBalance,
          )}
          details={getAccountTypeStatDetails(opening, closing, false)}
          detailsLabel="by account type"
        />
        <StatCard
          label="Unassigned funds"
          value={formatCurrency(
            overview.fundClosingBalance.totalUnassignedBalance,
          )}
          caption="Available to assign to funds"
        />
      </ResponsiveGrid>
      <Stack direction={{ xs: "column", lg: "row" }} spacing={2}>
        <Box sx={{ flex: 2, minWidth: 0, ...fillHeightSx }}>
          <OverviewAttentionList
            groups={overview.attention}
            currentPeriodId={currentPeriod?.accountingPeriod.id ?? null}
            returnUrl={overviewUrl}
          />
        </Box>
        <Box sx={{ flex: 1, minWidth: 0, ...fillHeightSx }}>
          <OverviewPeriodCard period={currentPeriod} returnUrl={overviewUrl} />
        </Box>
      </Stack>
      <ResponsiveGrid columns={{ xs: 1, lg: 2 }}>
        <BalanceTrendChart
          chartPoints={buildDateChartPoints(
            overview.accountBalances,
            overview.granularity,
          )}
          title="Account Balance Trend"
          xAxisLabel="Date"
          variant="surface"
          headerContent={
            <Link
              href={accountRoutes.trends(trendsRange)}
              aria-label="View account trends"
            >
              <IconButton component="span" size="small">
                <ChevronRight />
              </IconButton>
            </Link>
          }
        />
        <BalanceTrendChart
          chartPoints={buildDateChartPoints(
            overview.fundBalances,
            overview.granularity,
          )}
          title="Fund Balance Trend"
          xAxisLabel="Date"
          variant="surface"
          headerContent={
            <Link
              href={fundRoutes.trends(trendsRange)}
              aria-label="View fund trends"
            >
              <IconButton component="span" size="small">
                <ChevronRight />
              </IconButton>
            </Link>
          }
        />
      </ResponsiveGrid>
      <OverviewAddTransactionButtonSpacer />
    </PageLayout>
  );
};

export default OverviewView;
