import {
  AccountSortOrder,
  type AccountTypeBalance,
  formatAccountType,
} from "@/accounts/types";
import {
  Box,
  Button,
  Divider,
  LinearProgress,
  Paper,
  Stack,
  Typography,
} from "@mui/material";
import AccountListFrame from "@/accounts/AccountListFrame";
import AccountsDashboardControls from "@/accounts/AccountsDashboardControls";
import Breadcrumbs from "@/framework/Breadcrumbs";
import type { JSX } from "react";
import SummaryCard from "@/framework/view/SummaryCard";
import breadcrumbs from "@/accounts/breadcrumbs";
import formatCurrency from "@/framework/formatCurrency";
import getApiClient from "@/framework/data/getApiClient";
import nameof from "@/framework/data/nameof";
import routes from "@/accounts/routes";
import { rowsPerPage } from "@/framework/listframe/Constants";

/**
 * Props for the AccountBalanceCompositionPanel component.
 */
interface AccountBalanceCompositionPanelProps {
  readonly balances: AccountTypeBalance[];
}

/**
 * Displays the account balance mix grouped by account type.
 */
const AccountBalanceCompositionPanel = function ({
  balances,
}: AccountBalanceCompositionPanelProps): JSX.Element {
  const maxValue = Math.max(
    ...balances.map((item) => Math.abs(item.totalBalance)),
    0,
  );

  return (
    <Paper sx={{ border: "1px solid", borderColor: "divider", p: 3 }}>
      <Stack spacing={3}>
        <Stack spacing={0.5}>
          <Typography variant="h5">Balance composition</Typography>
          <Typography variant="body2" color="text.secondary">
            Use the distribution by account type to spot where balances are
            concentrated before drilling into the registry.
          </Typography>
        </Stack>
        {balances.length === 0 ? (
          <Typography variant="body2" color="text.secondary">
            No account balances available.
          </Typography>
        ) : (
          <Stack spacing={2} divider={<Divider flexItem />}>
            {balances.map((balanceByAccountType) => (
              <Stack key={balanceByAccountType.accountType} spacing={1.25}>
                <Stack direction="row" justifyContent="space-between" gap={2}>
                  <Typography variant="body1">
                    {formatAccountType(balanceByAccountType.accountType)}
                  </Typography>
                  <Typography variant="body1" fontWeight={600}>
                    {formatCurrency(balanceByAccountType.totalBalance)}
                  </Typography>
                </Stack>
                <LinearProgress
                  variant="determinate"
                  value={
                    maxValue === 0
                      ? 0
                      : (Math.abs(balanceByAccountType.totalBalance) /
                          maxValue) *
                        100
                  }
                  sx={{ height: 9, borderRadius: 999 }}
                />
              </Stack>
            ))}
          </Stack>
        )}
      </Stack>
    </Paper>
  );
};

/**
 * Formats the current account sort into human-readable text.
 */
const formatAccountSort = function (
  sort: AccountSortOrder | undefined,
): string {
  if (typeof sort !== "string") {
    return "Default order";
  }

  switch (sort) {
    case AccountSortOrder.Name:
      return "Name: A to Z";
    case AccountSortOrder.NameDescending:
      return "Name: Z to A";
    case AccountSortOrder.Type:
      return "Type: A to Z";
    case AccountSortOrder.TypeDescending:
      return "Type: Z to A";
    case AccountSortOrder.PostedBalance:
      return "Balance: low to high";
    case AccountSortOrder.PostedBalanceDescending:
      return "Balance: high to low";
    default:
      return "Default order";
  }
};

/**
 * Search parameters for the AccountsView component.
 */
interface AccountsViewSearchParams {
  search?: string;
  sort?: AccountSortOrder;
  page?: number;
}

/**
 * Props for the AccountsView component.
 */
interface AccountsViewProps {
  readonly searchParams: Promise<AccountsViewSearchParams>;
}

/**
 * Component that displays the Accounts view.
 */
const AccountsView = async function ({
  searchParams,
}: AccountsViewProps): Promise<JSX.Element> {
  const { search, sort, page } = await searchParams;

  const apiClient = getApiClient();
  const accountsPromise = apiClient.GET("/accounts", {
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
  const summaryPromise = apiClient.GET("/accounts/summary");

  const [{ data: accounts }, { data: accountingPeriods }, { data: summary }] =
    await Promise.all([
      accountsPromise,
      accountingPeriodsPromise,
      summaryPromise,
    ]);
  if (
    typeof accounts === "undefined" ||
    typeof accountingPeriods === "undefined" ||
    typeof summary === "undefined"
  ) {
    throw new Error(`Failed to fetch accounts`);
  }

  const isInOnboardingMode = accountingPeriods.totalCount === 0;
  const currentSearch = search?.trim() ?? "";
  const hasActiveSearch = currentSearch !== "";
  const visibleCount = accounts.items.length;
  const trackedUntrackedTotal =
    Math.abs(summary.totalTrackedBalance) +
    Math.abs(summary.totalUntrackedBalance);
  const trackedShare =
    trackedUntrackedTotal === 0
      ? 0
      : (Math.abs(summary.totalTrackedBalance) / trackedUntrackedTotal) * 100;
  const untrackedShare =
    trackedUntrackedTotal === 0
      ? 0
      : (Math.abs(summary.totalUntrackedBalance) / trackedUntrackedTotal) * 100;

  return (
    <Stack spacing={3} sx={{ maxWidth: 1440 }}>
      <Breadcrumbs breadcrumbs={breadcrumbs.index()} />
      <Paper
        sx={{
          backgroundColor: "background.paper",
          backgroundImage:
            "linear-gradient(135deg, rgba(76, 175, 80, 0.18) 0%, rgba(255, 255, 255, 0) 58%)",
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
                Accounts workspace
              </Typography>
              <Typography variant="h3">Accounts dashboard</Typography>
              <Typography color="text.secondary" maxWidth={760}>
                {isInOnboardingMode
                  ? "Finish onboarding, establish your first account structure, and keep tracked versus untracked balances in view from one workspace."
                  : "Monitor the current account picture, refine the registry view, and move from balance-level insight into individual accounts without leaving the page."}
              </Typography>
              <Typography variant="body2" color="text.secondary">
                {hasActiveSearch
                  ? `Showing ${visibleCount} of ${accounts.totalCount} accounts matching "${currentSearch}".`
                  : `Showing ${visibleCount} accounts on this page across ${accounts.totalCount} total accounts.`}
              </Typography>
            </Stack>
            <Stack direction="row" spacing={1.5} flexWrap="wrap" useFlexGap>
              <Button
                variant="contained"
                href={isInOnboardingMode ? routes.onboard : routes.create({})}
              >
                {isInOnboardingMode ? "Start onboarding" : "Create account"}
              </Button>
              <Button
                variant="outlined"
                href={routes.index({
                  sort: AccountSortOrder.PostedBalanceDescending,
                })}
              >
                Largest balances first
              </Button>
              <Button
                variant="outlined"
                href={routes.index({ sort: AccountSortOrder.Type })}
              >
                Group by type
              </Button>
            </Stack>
          </Stack>
          <AccountsDashboardControls
            searchParamName={nameof<AccountsViewSearchParams>("search")}
            sortParamName={nameof<AccountsViewSearchParams>("sort")}
            pageParamName={nameof<AccountsViewSearchParams>("page")}
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
          title="Total Balance"
          value={formatCurrency(summary.totalBalance)}
          description="Sum of all account balances"
        />
        <SummaryCard
          title="Total Tracked Balance"
          value={formatCurrency(summary.totalTrackedBalance)}
          description="Sum of tracked account balances"
        />
        <SummaryCard
          title="Total Untracked Balance"
          value={formatCurrency(summary.totalUntrackedBalance)}
          description="Sum of untracked account balances"
        />
        <SummaryCard
          title="Accounts In Scope"
          value={accounts.totalCount}
          description={
            hasActiveSearch
              ? `Filtered by "${currentSearch}" with ${visibleCount} account${visibleCount === 1 ? "" : "s"} visible on this page.`
              : `${visibleCount} account${visibleCount === 1 ? "" : "s"} visible on this page.`
          }
        />
      </Box>
      <AccountBalanceCompositionPanel balances={summary.balanceByAccountType} />
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
              <Typography variant="h5">Account registry</Typography>
              <Typography variant="body2" color="text.secondary">
                The full account list stays available for detailed review, while
                sorting in the column headers remains available when you need
                finer control.
              </Typography>
            </Stack>
          </Paper>
          <AccountListFrame
            data={accounts.items}
            isInOnboardingMode={isInOnboardingMode}
            totalCount={accounts.totalCount}
            showCreateAction={false}
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
                    {hasActiveSearch ? currentSearch : "All accounts"}
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
                    {formatAccountSort(sort)}
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
                    {visibleCount} of {accounts.totalCount}
                  </Typography>
                </Box>
              </Stack>
            </Stack>
          </Paper>
          <Paper sx={{ border: "1px solid", borderColor: "divider", p: 3 }}>
            <Stack spacing={2}>
              <Typography variant="h6">Balance mix</Typography>
              <Stack spacing={1.5}>
                <Stack spacing={0.75}>
                  <Stack direction="row" justifyContent="space-between" gap={2}>
                    <Typography variant="body2">Tracked balances</Typography>
                    <Typography variant="body2" fontWeight={600}>
                      {formatCurrency(summary.totalTrackedBalance)}
                    </Typography>
                  </Stack>
                  <LinearProgress
                    variant="determinate"
                    value={trackedShare}
                    sx={{ height: 8, borderRadius: 999 }}
                  />
                </Stack>
                <Stack spacing={0.75}>
                  <Stack direction="row" justifyContent="space-between" gap={2}>
                    <Typography variant="body2">Untracked balances</Typography>
                    <Typography variant="body2" fontWeight={600}>
                      {formatCurrency(summary.totalUntrackedBalance)}
                    </Typography>
                  </Stack>
                  <LinearProgress
                    variant="determinate"
                    value={untrackedShare}
                    sx={{ height: 8, borderRadius: 999 }}
                    color="secondary"
                  />
                </Stack>
              </Stack>
            </Stack>
          </Paper>
          <Paper sx={{ border: "1px solid", borderColor: "divider", p: 3 }}>
            <Stack spacing={2}>
              <Typography variant="h6">Next actions</Typography>
              <Typography variant="body2" color="text.secondary">
                {isInOnboardingMode
                  ? "Use the onboarding flow to create your first account and start classifying balances."
                  : "Use the dashboard controls for broad review, then jump into individual accounts when you are ready to inspect transactions."}
              </Typography>
              <Stack spacing={1.25}>
                <Button
                  variant="contained"
                  href={isInOnboardingMode ? routes.onboard : routes.create({})}
                >
                  {isInOnboardingMode ? "Start onboarding" : "Create account"}
                </Button>
                <Button
                  variant="outlined"
                  href={routes.index({
                    sort: AccountSortOrder.PostedBalanceDescending,
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

export type { AccountsViewSearchParams };
export default AccountsView;
