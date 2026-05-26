import { Box, Button, Paper, Stack, Typography } from "@mui/material";
import {
  defaultAccountingPeriodTrendRange,
  parseAccountingPeriodTrendRange,
} from "@/accounting-periods/dashboard";
import AccountingPeriodBalanceTrendCard from "@/accounting-periods/AccountingPeriodBalanceTrendCard";
import AccountingPeriodListFrame from "@/accounting-periods/AccountingPeriodListFrame";
import { AccountingPeriodSortOrder } from "@/accounting-periods/types";
import AccountingPeriodsDashboardControls from "@/accounting-periods/AccountingPeriodsDashboardControls";
import Breadcrumbs from "@/framework/Breadcrumbs";
import type { JSX } from "react";
import SummaryCard from "@/framework/view/SummaryCard";
import breadcrumbs from "@/accounting-periods/breadcrumbs";
import formatCurrency from "@/framework/formatCurrency";
import getApiClient from "@/framework/data/getApiClient";
import nameof from "@/framework/data/nameof";
import routes from "@/accounting-periods/routes";
import { rowsPerPage } from "@/framework/listframe/Constants";

/**
 * Search parameters for the AccountingPeriodsView component.
 */
interface AccountingPeriodsViewSearchParams {
  search?: string;
  sort?: AccountingPeriodSortOrder;
  page?: number;
  range?: number;
}

/**
 * Props for the AccountingPeriodsView component.
 */
interface AccountingPeriodsViewProps {
  readonly searchParams: Promise<AccountingPeriodsViewSearchParams>;
}

/**
 * Component that displays the Accounting Periods view.
 */
const AccountingPeriodsView = async function ({
  searchParams,
}: AccountingPeriodsViewProps): Promise<JSX.Element> {
  const { search, sort, page, range } = await searchParams;
  const selectedRange = parseAccountingPeriodTrendRange(range);

  const client = getApiClient();
  const accountingPeriodsPromise = client.GET("/accounting-periods", {
    params: {
      query: {
        Search: search ?? "",
        Sort: sort ?? null,
        Limit: rowsPerPage,
        Offset: ((page ?? 1) - 1) * rowsPerPage,
      },
    },
  });
  const latestAccountingPeriodPromise = client.GET("/accounting-periods", {
    params: {
      query: {
        Search: "",
        Sort: AccountingPeriodSortOrder.DateDescending,
        Limit: 1,
        Offset: 0,
      },
    },
  });
  const trendAccountingPeriodsPromise = client.GET("/accounting-periods", {
    params: {
      query: {
        Search: "",
        Sort: AccountingPeriodSortOrder.DateDescending,
        Limit: selectedRange,
        Offset: 0,
      },
    },
  });

  const [
    { data: accountingPeriods },
    { data: latestAccountingPeriods },
    { data: trendAccountingPeriods },
  ] = await Promise.all([
    accountingPeriodsPromise,
    latestAccountingPeriodPromise,
    trendAccountingPeriodsPromise,
  ]);

  if (
    typeof accountingPeriods === "undefined" ||
    typeof latestAccountingPeriods === "undefined" ||
    typeof trendAccountingPeriods === "undefined"
  ) {
    throw new Error("Failed to fetch accounting periods");
  }

  const latestAccountingPeriod = latestAccountingPeriods.items[0] ?? null;
  const trendPeriods = [...trendAccountingPeriods.items].reverse();
  const currentSearch = search?.trim() ?? "";
  const hasActiveSearch = currentSearch !== "";
  const visibleCount = accountingPeriods.items.length;
  const balanceChange = latestAccountingPeriod
    ? latestAccountingPeriod.closingBalance -
      latestAccountingPeriod.openingBalance
    : null;

  return (
    <Stack spacing={3} sx={{ maxWidth: 1440 }}>
      <Breadcrumbs breadcrumbs={breadcrumbs.index()} />
      <Paper
        sx={{
          backgroundColor: "background.paper",
          backgroundImage:
            "linear-gradient(135deg, rgba(0, 150, 136, 0.16) 0%, rgba(255, 193, 7, 0.12) 48%, rgba(255, 255, 255, 0) 74%)",
          border: "1px solid",
          borderColor: "divider",
          overflow: "hidden",
          p: { xs: 3, md: 4 },
        }}
      >
        <Box
          sx={{
            display: "grid",
            gap: 3,
            gridTemplateColumns: {
              xs: "1fr",
              xl: "minmax(0, 1.15fr) minmax(360px, 0.85fr)",
            },
          }}
        >
          <Stack spacing={3}>
            <Stack spacing={1}>
              <Typography variant="overline" color="text.secondary">
                Accounting periods workspace
              </Typography>
              <Typography variant="h3">Accounting periods dashboard</Typography>
              <Typography color="text.secondary" maxWidth={760}>
                Review the latest period balance position, scan the trend across
                recent months, and keep the full registry available for detailed
                drill-in below.
              </Typography>
              <Typography variant="body2" color="text.secondary">
                {hasActiveSearch
                  ? `Showing ${visibleCount} of ${accountingPeriods.totalCount} accounting periods matching "${currentSearch}".`
                  : `Showing ${visibleCount} accounting periods on this page across ${accountingPeriods.totalCount} total periods.`}
              </Typography>
            </Stack>
            <Stack direction="row" spacing={1.5} flexWrap="wrap" useFlexGap>
              <Button variant="contained" href={routes.create}>
                Create accounting period
              </Button>
              {latestAccountingPeriod !== null && (
                <Button
                  variant="outlined"
                  href={routes.detail({ id: latestAccountingPeriod.id }, {})}
                >
                  Open latest period
                </Button>
              )}
              <Button
                variant="outlined"
                href={routes.index({
                  sort: AccountingPeriodSortOrder.ClosingBalanceDescending,
                })}
              >
                Highest closing balances
              </Button>
            </Stack>
          </Stack>
          <AccountingPeriodsDashboardControls
            searchParamName={nameof<AccountingPeriodsViewSearchParams>(
              "search",
            )}
            sortParamName={nameof<AccountingPeriodsViewSearchParams>("sort")}
            pageParamName={nameof<AccountingPeriodsViewSearchParams>("page")}
            rangeParamName={nameof<AccountingPeriodsViewSearchParams>("range")}
            defaultRange={defaultAccountingPeriodTrendRange}
          />
        </Box>
      </Paper>
      <Box
        sx={{
          display: "grid",
          gap: 2,
          gridTemplateColumns: {
            xs: "1fr",
            md: "repeat(2, minmax(0, 1fr))",
            xl: "repeat(4, minmax(0, 1fr))",
          },
        }}
      >
        <SummaryCard
          title="Latest Period"
          value={latestAccountingPeriod?.name ?? "No periods yet"}
          description={
            latestAccountingPeriod === null
              ? "Create your first accounting period to start tracking balances by month."
              : latestAccountingPeriod.isOpen
                ? "Current latest period is still open."
                : "Current latest period has been closed."
          }
        />
        <SummaryCard
          title="Opening Balance"
          value={
            latestAccountingPeriod === null
              ? "-"
              : formatCurrency(latestAccountingPeriod.openingBalance)
          }
          description="Starting balance for the latest accounting period"
        />
        <SummaryCard
          title="Closing Balance"
          value={
            latestAccountingPeriod === null
              ? "-"
              : formatCurrency(latestAccountingPeriod.closingBalance)
          }
          description="Ending balance for the latest accounting period"
        />
        <SummaryCard
          title="Net Change"
          value={
            balanceChange === null
              ? "-"
              : `${balanceChange >= 0 ? "+" : "-"}${formatCurrency(Math.abs(balanceChange))}`
          }
          description="Change from opening to closing balance in the latest period"
        />
      </Box>
      <Box
        sx={{
          display: "grid",
          gap: 3,
          gridTemplateColumns: {
            xs: "1fr",
            xl: "minmax(0, 1.25fr) minmax(280px, 0.75fr)",
          },
        }}
      >
        <AccountingPeriodBalanceTrendCard
          accountingPeriods={trendPeriods}
          selectedRange={selectedRange}
        />
        <Paper
          sx={{
            border: "1px solid",
            borderColor: "divider",
            p: 3,
          }}
        >
          <Stack spacing={2}>
            <Stack spacing={0.5}>
              <Typography variant="h5">Dashboard notes</Typography>
              <Typography variant="body2" color="text.secondary">
                Use the chart window to compare recent month-end balances, then
                switch to the registry for sorting, paging, and direct access to
                any specific accounting period.
              </Typography>
            </Stack>
            <Box>
              <Typography variant="overline" color="text.secondary">
                Trend window
              </Typography>
              <Typography variant="body1">
                Last {selectedRange} accounting periods
              </Typography>
              <Typography variant="body2" color="text.secondary">
                {trendPeriods.length < selectedRange
                  ? `Only ${trendPeriods.length} periods are currently available.`
                  : "The chart is fully populated for the selected range."}
              </Typography>
            </Box>
            <Box>
              <Typography variant="overline" color="text.secondary">
                Latest balance state
              </Typography>
              <Typography variant="body1">
                {latestAccountingPeriod === null
                  ? "No data available"
                  : latestAccountingPeriod.isOpen
                    ? "Latest period remains open"
                    : "Latest period is closed"}
              </Typography>
              <Typography variant="body2" color="text.secondary">
                {latestAccountingPeriod === null
                  ? "Once a period exists, this panel will summarize the latest month-end status."
                  : `${latestAccountingPeriod.name} is the most recent accounting period in the registry.`}
              </Typography>
            </Box>
          </Stack>
        </Paper>
      </Box>
      <Stack spacing={2}>
        <Paper sx={{ border: "1px solid", borderColor: "divider", p: 3 }}>
          <Stack spacing={0.75}>
            <Typography variant="h5">Accounting period registry</Typography>
            <Typography variant="body2" color="text.secondary">
              The detailed list remains available below so you can sort in the
              table headers, paginate through history, and move into
              period-level details after reviewing the dashboard summary.
            </Typography>
          </Stack>
        </Paper>
        <AccountingPeriodListFrame
          data={accountingPeriods.items}
          totalCount={accountingPeriods.totalCount}
        />
      </Stack>
    </Stack>
  );
};

export type { AccountingPeriodsViewSearchParams };
export default AccountingPeriodsView;
