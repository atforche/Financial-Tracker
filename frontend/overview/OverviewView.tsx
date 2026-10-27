import { Box, Button, Stack, Typography } from "@mui/material";
import {
  type OverviewSearchParams,
  getOverviewHistoryStart,
  parseOverviewHistoryRange,
} from "@/overview/helpers";
import OverviewStat, { type OverviewStatDetail } from "@/overview/OverviewStat";
import { formatAccountType, isTrackedAccountType } from "@/accounts/helpers";
import ArrowForward from "@mui/icons-material/ArrowForward";
import BalanceTrendChart from "@/framework/charts/BalanceTrendChart";
import type { JSX } from "react";
import Link from "next/link";
import OverviewAddTransactionButton from "@/overview/OverviewAddTransactionButton";
import OverviewAttentionList from "@/overview/OverviewAttentionList";
import OverviewChange from "@/overview/OverviewChange";
import { OverviewHistoryRangeModel } from "@/framework/data/api";
import OverviewHistoryRangeSelector from "@/overview/OverviewHistoryRangeSelector";
import OverviewPeriodCard from "@/overview/OverviewPeriodCard";
import PageLayout from "@/framework/view/PageLayout";
import ResponsiveGrid from "@/framework/view/ResponsiveGrid";
import accountRoutes from "@/accounts/routes";
import { buildDateChartPoints } from "@/framework/charts/balanceTrendHelpers";
import createApiClient from "@/framework/data/createApiClient";
import dayjs from "dayjs";
import { formatCurrency } from "@/framework/currencyHelpers";
import { formatLongDate } from "@/framework/dateHelpers";
import fundRoutes from "@/funds/routes";
import { getAccountTypeBreakdownDetails } from "@/accounts/balanceSummaryHelpers";
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
        <OverviewChange amount={current - previous} /> over{" "}
        {historyDescriptions[history]}
      </>
    );
  };
  const accountTypeDetails = getAccountTypeBreakdownDetails(opening, closing);
  const getTypeDetails = function (tracked: boolean): OverviewStatDetail[] {
    return accountTypeDetails
      .filter(
        ({ accountType }) => isTrackedAccountType(accountType) === tracked,
      )
      .map((detail) => ({
        key: detail.accountType,
        label: formatAccountType(detail.accountType),
        value: formatCurrency(detail.endingBalance),
        change: detail.netChange,
      }));
  };
  const trendsRange = {
    mode: "date",
    startDate: getOverviewHistoryStart(asOf, history),
    endDate: asOf,
  } as const;

  return (
    <PageLayout>
      <Stack
        direction="row"
        useFlexGap
        spacing={2}
        sx={{
          flexWrap: "wrap",
          alignItems: "center",
          justifyContent: "space-between",
        }}
      >
        <Stack spacing={0.5}>
          <Typography variant="h4">Overview</Typography>
          <Typography color="text.secondary">
            {formatLongDate(new Date(`${asOf}T00:00:00`))} ·{" "}
            {currentPeriod === null
              ? "No open accounting period"
              : `${currentPeriod.accountingPeriod.name} is open`}
          </Typography>
        </Stack>
        <OverviewAddTransactionButton />
      </Stack>
      <ResponsiveGrid columns={{ xs: 1, sm: 2, lg: 4 }}>
        <OverviewStat
          label="Net worth"
          value={formatCurrency(closing.totalBalance)}
          caption={describeChange(closing.totalBalance, opening.totalBalance)}
        />
        <OverviewStat
          label="Tracked money"
          value={formatCurrency(closing.totalTrackedBalance)}
          caption={describeChange(
            closing.totalTrackedBalance,
            opening.totalTrackedBalance,
          )}
          details={getTypeDetails(true)}
          detailsLabel="by account type"
        />
        <OverviewStat
          label="Untracked money"
          value={formatCurrency(closing.totalUntrackedBalance)}
          caption={describeChange(
            closing.totalUntrackedBalance,
            opening.totalUntrackedBalance,
          )}
          details={getTypeDetails(false)}
          detailsLabel="by account type"
        />
        <OverviewStat
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
          />
        </Box>
        <Box sx={{ flex: 1, minWidth: 0, ...fillHeightSx }}>
          <OverviewPeriodCard period={currentPeriod} />
        </Box>
      </Stack>
      <Stack spacing={2}>
        <Stack
          direction="row"
          useFlexGap
          spacing={1.5}
          sx={{
            flexWrap: "wrap",
            alignItems: "center",
            justifyContent: "space-between",
          }}
        >
          <Typography variant="h5">Balance history</Typography>
          <OverviewHistoryRangeSelector value={history} />
        </Stack>
        <ResponsiveGrid columns={{ xs: 1, lg: 2 }}>
          <BalanceTrendChart
            chartPoints={buildDateChartPoints(
              overview.accountBalances,
              overview.granularity,
            )}
            title="Account Balance Trend"
            xAxisLabel="Date"
          />
          <BalanceTrendChart
            chartPoints={buildDateChartPoints(
              overview.fundBalances,
              overview.granularity,
            )}
            title="Fund Balance Trend"
            xAxisLabel="Date"
          />
        </ResponsiveGrid>
        <Stack direction="row" useFlexGap spacing={1} sx={{ flexWrap: "wrap" }}>
          <Link
            href={accountRoutes.trends(trendsRange)}
            style={{ textDecoration: "none" }}
          >
            <Button component="span" endIcon={<ArrowForward />}>
              View account trends
            </Button>
          </Link>
          <Link
            href={fundRoutes.trends(trendsRange)}
            style={{ textDecoration: "none" }}
          >
            <Button component="span" endIcon={<ArrowForward />}>
              View fund trends
            </Button>
          </Link>
        </Stack>
      </Stack>
    </PageLayout>
  );
};

export default OverviewView;
