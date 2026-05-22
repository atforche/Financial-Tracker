import {
  Box,
  Button,
  LinearProgress,
  Paper,
  Stack,
  Typography,
} from "@mui/material";
import { FundSortOrder, type FundSummary } from "@/funds/types";
import Breadcrumbs from "@/framework/Breadcrumbs";
import FundListFrame from "@/funds/FundListFrame";
import FundsDashboardControls from "@/funds/FundsDashboardControls";
import type { JSX } from "react";
import SummaryCard from "@/framework/view/SummaryCard";
import breadcrumbs from "@/funds/breadcrumbs";
import formatCurrency from "@/framework/formatCurrency";
import getApiClient from "@/framework/data/getApiClient";
import nameof from "@/framework/data/nameof";
import routes from "@/funds/routes";
import { rowsPerPage } from "@/framework/listframe/Constants";

/**
 * Formats the current fund sort into human-readable text.
 */
const formatFundSort = function (sort: FundSortOrder | undefined): string {
  if (typeof sort !== "string") {
    return "Default order";
  }

  switch (sort) {
    case FundSortOrder.Name:
      return "Name: A to Z";
    case FundSortOrder.NameDescending:
      return "Name: Z to A";
    case FundSortOrder.Description:
      return "Description: A to Z";
    case FundSortOrder.DescriptionDescending:
      return "Description: Z to A";
    case FundSortOrder.Balance:
      return "Balance: low to high";
    case FundSortOrder.BalanceDescending:
      return "Balance: high to low";
    default:
      return "Default order";
  }
};

/**
 * Search parameters for the FundsView component.
 */
interface FundsViewSearchParams {
  search?: string;
  sort?: FundSortOrder;
  page?: number;
}

/**
 * Props for the FundsView component.
 */
interface FundsViewProps {
  readonly searchParams: Promise<FundsViewSearchParams>;
}

/**
 * Component that displays the Funds view.
 */
const FundsView = async function ({
  searchParams,
}: FundsViewProps): Promise<JSX.Element> {
  const { search, sort, page } = await searchParams;

  const apiClient = getApiClient();
  const fundsPromise = apiClient.GET("/funds", {
    params: {
      query: {
        Search: search ?? "",
        Sort: sort ?? null,
        Limit: rowsPerPage,
        Offset: ((page ?? 1) - 1) * rowsPerPage,
      },
    },
  });
  const accountingPeriodsPromise = apiClient.GET("/accounting-periods", {
    params: {
      query: {
        Search: "",
        Sort: null,
        Limit: 1,
        Offset: 0,
      },
    },
  });
  const summaryPromise = apiClient.GET("/funds/summary");

  const [{ data: funds }, { data: accountingPeriods }, { data: summary }] =
    await Promise.all([fundsPromise, accountingPeriodsPromise, summaryPromise]);

  if (
    typeof funds === "undefined" ||
    typeof accountingPeriods === "undefined" ||
    typeof summary === "undefined"
  ) {
    throw new Error(`Failed to fetch funds`);
  }

  const fundSummary: FundSummary = summary;
  const isInOnboardingMode = accountingPeriods.totalCount === 0;
  const currentSearch = search?.trim() ?? "";
  const hasActiveSearch = currentSearch !== "";
  const visibleCount = funds.items.length;
  const allocationTotal =
    Math.abs(fundSummary.totalAssignedBalance) +
    Math.abs(fundSummary.totalUnassignedBalance);
  const assignedShare =
    allocationTotal === 0
      ? 0
      : (Math.abs(fundSummary.totalAssignedBalance) / allocationTotal) * 100;
  const unassignedShare =
    allocationTotal === 0
      ? 0
      : (Math.abs(fundSummary.totalUnassignedBalance) / allocationTotal) * 100;

  return (
    <Stack spacing={3} sx={{ maxWidth: 1440 }}>
      <Breadcrumbs breadcrumbs={breadcrumbs.index()} />
      <Paper
        sx={{
          backgroundColor: "background.paper",
          backgroundImage:
            "linear-gradient(135deg, rgba(0, 150, 136, 0.18) 0%, rgba(255, 193, 7, 0.1) 45%, rgba(255, 255, 255, 0) 72%)",
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
              xl: "minmax(0, 1.2fr) minmax(360px, 0.8fr)",
            },
          }}
        >
          <Stack spacing={3}>
            <Stack spacing={1}>
              <Typography variant="overline" color="text.secondary">
                Funds workspace
              </Typography>
              <Typography variant="h3">Funds dashboard</Typography>
              <Typography color="text.secondary" maxWidth={760}>
                {isInOnboardingMode
                  ? "Finish onboarding, establish your first fund structure, and keep assigned versus unassigned balances visible from one workspace."
                  : "Monitor how tracked money is allocated, refine the registry view, and move from high-level balance review into individual funds without leaving the page."}
              </Typography>
              <Typography variant="body2" color="text.secondary">
                {hasActiveSearch
                  ? `Showing ${visibleCount} of ${funds.totalCount} funds matching "${currentSearch}".`
                  : `Showing ${visibleCount} funds on this page across ${funds.totalCount} total funds.`}
              </Typography>
            </Stack>
            <Stack direction="row" spacing={1.5} flexWrap="wrap" useFlexGap>
              <Button
                variant="contained"
                href={isInOnboardingMode ? routes.onboard : routes.create({})}
              >
                {isInOnboardingMode ? "Start onboarding" : "Create fund"}
              </Button>
              <Button
                variant="outlined"
                href={routes.index({
                  sort: FundSortOrder.BalanceDescending,
                })}
              >
                Largest balances first
              </Button>
              <Button
                variant="outlined"
                href={routes.index({ sort: FundSortOrder.Name })}
              >
                A to Z
              </Button>
            </Stack>
          </Stack>
          <FundsDashboardControls
            searchParamName={nameof<FundsViewSearchParams>("search")}
            sortParamName={nameof<FundsViewSearchParams>("sort")}
            pageParamName={nameof<FundsViewSearchParams>("page")}
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
          title="Total Tracked Balance"
          value={formatCurrency(fundSummary.totalTrackedBalance)}
          description="Sum of all fund balances"
        />
        <SummaryCard
          title="Total Assigned Balance"
          value={formatCurrency(fundSummary.totalAssignedBalance)}
          description="All fund balances except Unassigned"
        />
        <SummaryCard
          title="Total Unassigned Balance"
          value={formatCurrency(fundSummary.totalUnassignedBalance)}
          description="Current balance of the Unassigned fund"
        />
        <SummaryCard
          title="Funds In Scope"
          value={funds.totalCount}
          description={
            hasActiveSearch
              ? `Filtered by "${currentSearch}" with ${visibleCount} fund${visibleCount === 1 ? "" : "s"} visible on this page.`
              : `${visibleCount} fund${visibleCount === 1 ? "" : "s"} visible on this page.`
          }
        />
      </Box>
      <Box
        sx={{
          display: "grid",
          gap: 3,
          gridTemplateColumns: {
            xs: "1fr",
            xl: "minmax(0, 1.3fr) minmax(320px, 0.7fr)",
          },
        }}
      >
        <Stack spacing={2}>
          <Paper sx={{ border: "1px solid", borderColor: "divider", p: 3 }}>
            <Stack spacing={0.75}>
              <Typography variant="h5">Fund registry</Typography>
              <Typography variant="body2" color="text.secondary">
                The full fund list stays available for detailed review, while
                sorting in the column headers remains available when you need
                finer control.
              </Typography>
            </Stack>
          </Paper>
          <FundListFrame
            data={funds.items}
            isInOnboardingMode={isInOnboardingMode}
            totalCount={funds.totalCount}
          />
        </Stack>
        <Stack
          spacing={2}
          sx={{
            alignSelf: "start",
            position: { xl: "sticky" },
            top: { xl: 24 },
          }}
        >
          <Paper sx={{ border: "1px solid", borderColor: "divider", p: 3 }}>
            <Stack spacing={2}>
              <Typography variant="h6">Current view</Typography>
              <Stack spacing={1.25}>
                <Box
                  sx={{
                    display: "flex",
                    justifyContent: "space-between",
                    gap: 2,
                  }}
                >
                  <Typography variant="body2" color="text.secondary">
                    Search
                  </Typography>
                  <Typography
                    variant="body2"
                    fontWeight={600}
                    textAlign="right"
                  >
                    {hasActiveSearch ? currentSearch : "All funds"}
                  </Typography>
                </Box>
                <Box
                  sx={{
                    display: "flex",
                    justifyContent: "space-between",
                    gap: 2,
                  }}
                >
                  <Typography variant="body2" color="text.secondary">
                    Sort
                  </Typography>
                  <Typography
                    variant="body2"
                    fontWeight={600}
                    textAlign="right"
                  >
                    {formatFundSort(sort)}
                  </Typography>
                </Box>
                <Box
                  sx={{
                    display: "flex",
                    justifyContent: "space-between",
                    gap: 2,
                  }}
                >
                  <Typography variant="body2" color="text.secondary">
                    Visible rows
                  </Typography>
                  <Typography
                    variant="body2"
                    fontWeight={600}
                    textAlign="right"
                  >
                    {visibleCount} of {funds.totalCount}
                  </Typography>
                </Box>
              </Stack>
            </Stack>
          </Paper>
          <Paper sx={{ border: "1px solid", borderColor: "divider", p: 3 }}>
            <Stack spacing={2}>
              <Typography variant="h6">Allocation mix</Typography>
              <Stack spacing={1.5}>
                <Stack spacing={0.75}>
                  <Stack direction="row" justifyContent="space-between" gap={2}>
                    <Typography variant="body2">Assigned balances</Typography>
                    <Typography variant="body2" fontWeight={600}>
                      {formatCurrency(fundSummary.totalAssignedBalance)}
                    </Typography>
                  </Stack>
                  <LinearProgress
                    variant="determinate"
                    value={assignedShare}
                    sx={{ height: 8, borderRadius: 999 }}
                  />
                </Stack>
                <Stack spacing={0.75}>
                  <Stack direction="row" justifyContent="space-between" gap={2}>
                    <Typography variant="body2">Unassigned balances</Typography>
                    <Typography variant="body2" fontWeight={600}>
                      {formatCurrency(fundSummary.totalUnassignedBalance)}
                    </Typography>
                  </Stack>
                  <LinearProgress
                    variant="determinate"
                    value={unassignedShare}
                    sx={{ height: 8, borderRadius: 999 }}
                    color="secondary"
                  />
                </Stack>
                <Box
                  sx={{
                    display: "flex",
                    justifyContent: "space-between",
                    gap: 2,
                  }}
                >
                  <Typography variant="body2" color="text.secondary">
                    Tracked total
                  </Typography>
                  <Typography variant="body2" fontWeight={600}>
                    {formatCurrency(fundSummary.totalTrackedBalance)}
                  </Typography>
                </Box>
              </Stack>
            </Stack>
          </Paper>
          <Paper sx={{ border: "1px solid", borderColor: "divider", p: 3 }}>
            <Stack spacing={2}>
              <Typography variant="h6">Next actions</Typography>
              <Typography variant="body2" color="text.secondary">
                {isInOnboardingMode
                  ? "Use the onboarding flow to create your first fund and start assigning money with intent."
                  : "Use the dashboard controls for broad review, then jump into individual funds when you are ready to inspect balances and transaction history."}
              </Typography>
              <Stack spacing={1.25}>
                <Button
                  variant="contained"
                  href={isInOnboardingMode ? routes.onboard : routes.create({})}
                >
                  {isInOnboardingMode ? "Start onboarding" : "Create fund"}
                </Button>
                <Button
                  variant="outlined"
                  href={routes.index({
                    sort: FundSortOrder.BalanceDescending,
                  })}
                >
                  Review highest balances
                </Button>
                <Button variant="outlined" href={routes.index({})}>
                  Reset dashboard view
                </Button>
              </Stack>
            </Stack>
          </Paper>
        </Stack>
      </Box>
    </Stack>
  );
};

export type { FundsViewSearchParams };
export default FundsView;
