import {
  AccountBalanceTrendPanel,
  type AccountBalanceTrendPoint,
  AccountLargestMoversPanel,
  AccountTypeComparisonPanel,
} from "@/accounts/overview-dashboard/AccountsDashboardPanels";
import type {
  AccountDashboardSortOrder,
  AccountType,
  AccountTypeBalance,
} from "@/accounts/types";
import {
  type AccountingPeriod,
  AccountingPeriodSortOrder,
} from "@/accounting-periods/types";
import { Box, Button, Paper, Stack, Typography } from "@mui/material";
import AccountsDashboardControls from "@/accounts/overview-dashboard/AccountsDashboardControls";
import AccountsDashboardListFrame from "@/accounts/overview-dashboard/AccountsDashboardListFrame";
import Breadcrumbs from "@/framework/Breadcrumbs";
import type { JSX } from "react";
import SummaryCard from "@/framework/view/SummaryCard";
import breadcrumbs from "@/accounts/breadcrumbs";
import formatCurrency from "@/framework/formatCurrency";
import getApiClient from "@/framework/data/getApiClient";
import { redirect } from "next/navigation";
import routes from "@/accounts/routes";
import { rowsPerPage } from "@/framework/listframe/Constants";

/**
 * URL mode values used to filter the Accounts dashboard.
 */
type AccountsDashboardFilterMode = "accounting-period" | "date";

/**
 * Search parameters for the account overview dashboard.
 */
interface AccountOverviewDashboardSearchParams {
  search?: string;
  sort?: AccountDashboardSortOrder;
  page?: number | string;
  mode?: AccountsDashboardFilterMode;
  accountType?: AccountType;
  startAccountingPeriodId?: string;
  endAccountingPeriodId?: string;
  startDate?: string;
  endDate?: string;
}

/**
 * Props for the AccountOverviewDashboard component.
 */
interface AccountOverviewDashboardProps {
  readonly searchParams: Promise<AccountOverviewDashboardSearchParams>;
}

const accountDashboardMode = {
  AccountingPeriod: "AccountingPeriod",
  Date: "Date",
} as const;

type AccountDashboardModeValue =
  (typeof accountDashboardMode)[keyof typeof accountDashboardMode];

interface AccountDashboardAccountResult {
  readonly id: string;
  readonly name: string;
  readonly type: AccountType;
  readonly startingBalance: number;
  readonly endingBalance: number;
}

interface AccountDashboardPeriodSummaryResult {
  readonly accountingPeriodId: string;
  readonly accountingPeriodName: string;
  readonly year: number;
  readonly month: number;
  readonly totalOpeningBalance: number;
  readonly totalClosingBalance: number;
  readonly trackedOpeningBalance: number;
  readonly trackedClosingBalance: number;
  readonly untrackedOpeningBalance: number;
  readonly untrackedClosingBalance: number;
  readonly openingBalanceByAccountType: readonly AccountTypeBalance[];
  readonly closingBalanceByAccountType: readonly AccountTypeBalance[];
}

interface AccountDashboardDateSummaryResult {
  readonly date: string;
  readonly totalBalance: number;
  readonly trackedBalance: number;
  readonly untrackedBalance: number;
  readonly balanceByAccountType: readonly AccountTypeBalance[];
}

interface AccountDashboardResult {
  readonly mode: AccountDashboardModeValue;
  readonly accounts: {
    readonly items: readonly AccountDashboardAccountResult[];
    readonly totalCount: number;
  };
  readonly accountingPeriods:
    | readonly AccountDashboardPeriodSummaryResult[]
    | null;
  readonly dates: readonly AccountDashboardDateSummaryResult[] | null;
}

/**
 * Summary metrics derived from the selected dashboard range.
 */
interface DashboardSnapshot {
  readonly startLabel: string;
  readonly endLabel: string;
  readonly totalStartingBalance: number;
  readonly totalEndingBalance: number;
  readonly trackedEndingBalance: number;
  readonly untrackedEndingBalance: number;
  readonly startingBalancesByType: readonly AccountTypeBalance[];
  readonly endingBalancesByType: readonly AccountTypeBalance[];
}

const dateFormatter = new Intl.DateTimeFormat("en-US", {
  month: "short",
  day: "numeric",
  year: "numeric",
});

const defaultFilterMode: AccountsDashboardFilterMode = "accounting-period";

const compareAccountingPeriodsDescending = function (
  left: AccountingPeriod,
  right: AccountingPeriod,
): number {
  if (left.year !== right.year) {
    return right.year - left.year;
  }
  return right.month - left.month;
};

const compareAccountingPeriodsAscending = function (
  left: AccountingPeriod,
  right: AccountingPeriod,
): number {
  if (left.year !== right.year) {
    return left.year - right.year;
  }
  return left.month - right.month;
};

const parsePageNumber = function (
  page: AccountOverviewDashboardSearchParams["page"],
): number {
  const pageNumber =
    typeof page === "number" ? page : Number.parseInt(page ?? "1", 10);
  return Number.isFinite(pageNumber) && pageNumber > 0 ? pageNumber : 1;
};

const toInputDate = function (
  year: number,
  month: number,
  day: number,
): string {
  return `${year.toString().padStart(4, "0")}-${month
    .toString()
    .padStart(2, "0")}-${day.toString().padStart(2, "0")}`;
};

const getPeriodDateRange = function (accountingPeriod: AccountingPeriod): {
  startDate: string;
  endDate: string;
} {
  const startDate = toInputDate(
    accountingPeriod.year,
    accountingPeriod.month,
    1,
  );
  const endDate = toInputDate(
    accountingPeriod.year,
    accountingPeriod.month,
    new Date(accountingPeriod.year, accountingPeriod.month, 0).getDate(),
  );

  return {
    startDate,
    endDate,
  };
};

const formatDateLabel = function (value: string): string {
  return dateFormatter.format(new Date(`${value}T00:00:00`));
};

const isObject = function (value: unknown): value is Record<string, unknown> {
  return typeof value === "object" && value !== null;
};

const getErrorMessage = function (value: unknown): string | null {
  if (!isObject(value)) {
    return null;
  }
  if (typeof value["detail"] === "string") {
    return value["detail"];
  }
  if (typeof value["title"] === "string") {
    return value["title"];
  }
  return null;
};

const isAccountTypeBalance = function (
  value: unknown,
): value is AccountTypeBalance {
  return (
    isObject(value) &&
    typeof value["accountType"] === "string" &&
    typeof value["totalBalance"] === "number"
  );
};

const isAccountDashboardAccount = function (
  value: unknown,
): value is AccountDashboardAccountResult {
  return (
    isObject(value) &&
    typeof value["id"] === "string" &&
    typeof value["name"] === "string" &&
    typeof value["type"] === "string" &&
    typeof value["startingBalance"] === "number" &&
    typeof value["endingBalance"] === "number"
  );
};

const isAccountDashboardPeriodSummary = function (
  value: unknown,
): value is AccountDashboardPeriodSummaryResult {
  return (
    isObject(value) &&
    typeof value["accountingPeriodId"] === "string" &&
    typeof value["accountingPeriodName"] === "string" &&
    typeof value["year"] === "number" &&
    typeof value["month"] === "number" &&
    typeof value["totalOpeningBalance"] === "number" &&
    typeof value["totalClosingBalance"] === "number" &&
    typeof value["trackedOpeningBalance"] === "number" &&
    typeof value["trackedClosingBalance"] === "number" &&
    typeof value["untrackedOpeningBalance"] === "number" &&
    typeof value["untrackedClosingBalance"] === "number" &&
    Array.isArray(value["openingBalanceByAccountType"]) &&
    value["openingBalanceByAccountType"].every(isAccountTypeBalance) &&
    Array.isArray(value["closingBalanceByAccountType"]) &&
    value["closingBalanceByAccountType"].every(isAccountTypeBalance)
  );
};

const isAccountDashboardDateSummary = function (
  value: unknown,
): value is AccountDashboardDateSummaryResult {
  return (
    isObject(value) &&
    typeof value["date"] === "string" &&
    typeof value["totalBalance"] === "number" &&
    typeof value["trackedBalance"] === "number" &&
    typeof value["untrackedBalance"] === "number" &&
    Array.isArray(value["balanceByAccountType"]) &&
    value["balanceByAccountType"].every(isAccountTypeBalance)
  );
};

const isAccountDashboard = function (
  value: unknown,
): value is AccountDashboardResult {
  if (!isObject(value)) {
    return false;
  }

  const { mode, accounts, accountingPeriods, dates } = value;
  if (
    mode !== accountDashboardMode.AccountingPeriod &&
    mode !== accountDashboardMode.Date
  ) {
    return false;
  }
  if (!isObject(accounts)) {
    return false;
  }
  if (
    !Array.isArray(accounts["items"]) ||
    !accounts["items"].every(isAccountDashboardAccount) ||
    typeof accounts["totalCount"] !== "number"
  ) {
    return false;
  }
  if (
    accountingPeriods !== null &&
    typeof accountingPeriods !== "undefined" &&
    (!Array.isArray(accountingPeriods) ||
      !accountingPeriods.every(isAccountDashboardPeriodSummary))
  ) {
    return false;
  }
  if (
    dates !== null &&
    typeof dates !== "undefined" &&
    (!Array.isArray(dates) || !dates.every(isAccountDashboardDateSummary))
  ) {
    return false;
  }

  return true;
};

const getDashboardSnapshot = function (
  dashboard: AccountDashboardResult,
): DashboardSnapshot {
  if (
    dashboard.mode === accountDashboardMode.AccountingPeriod &&
    dashboard.accountingPeriods !== null &&
    dashboard.accountingPeriods.length > 0
  ) {
    const firstPeriod = dashboard.accountingPeriods.at(0);
    const lastPeriod = dashboard.accountingPeriods.at(-1);
    if (
      typeof firstPeriod === "undefined" ||
      typeof lastPeriod === "undefined"
    ) {
      return {
        startLabel: "Start",
        endLabel: "End",
        totalStartingBalance: 0,
        totalEndingBalance: 0,
        trackedEndingBalance: 0,
        untrackedEndingBalance: 0,
        startingBalancesByType: [],
        endingBalancesByType: [],
      };
    }
    return {
      startLabel: firstPeriod.accountingPeriodName,
      endLabel: lastPeriod.accountingPeriodName,
      totalStartingBalance: firstPeriod.totalOpeningBalance,
      totalEndingBalance: lastPeriod.totalClosingBalance,
      trackedEndingBalance: lastPeriod.trackedClosingBalance,
      untrackedEndingBalance: lastPeriod.untrackedClosingBalance,
      startingBalancesByType: firstPeriod.openingBalanceByAccountType,
      endingBalancesByType: lastPeriod.closingBalanceByAccountType,
    };
  }

  const dates = dashboard.dates ?? [];
  const firstDate = dates.at(0);
  const lastDate = dates.at(-1);

  return {
    startLabel: firstDate ? formatDateLabel(firstDate.date) : "Start",
    endLabel: lastDate ? formatDateLabel(lastDate.date) : "End",
    totalStartingBalance: firstDate?.totalBalance ?? 0,
    totalEndingBalance: lastDate?.totalBalance ?? 0,
    trackedEndingBalance: lastDate?.trackedBalance ?? 0,
    untrackedEndingBalance: lastDate?.untrackedBalance ?? 0,
    startingBalancesByType: firstDate?.balanceByAccountType ?? [],
    endingBalancesByType: lastDate?.balanceByAccountType ?? [],
  };
};

const getTrendPoints = function (
  dashboard: AccountDashboardResult,
): AccountBalanceTrendPoint[] {
  if (
    dashboard.mode === accountDashboardMode.AccountingPeriod &&
    dashboard.accountingPeriods !== null
  ) {
    return dashboard.accountingPeriods.map((accountingPeriod) => ({
      label: accountingPeriod.accountingPeriodName,
      totalBalance: accountingPeriod.totalClosingBalance,
      trackedBalance: accountingPeriod.trackedClosingBalance,
      untrackedBalance: accountingPeriod.untrackedClosingBalance,
    }));
  }

  return (dashboard.dates ?? []).map((dateSummary) => ({
    label: formatDateLabel(dateSummary.date),
    totalBalance: dateSummary.totalBalance,
    trackedBalance: dateSummary.trackedBalance,
    untrackedBalance: dateSummary.untrackedBalance,
  }));
};

const getDashboardRangeLabel = function (
  dashboard: AccountDashboardResult,
): string {
  if (
    dashboard.mode === accountDashboardMode.AccountingPeriod &&
    dashboard.accountingPeriods !== null &&
    dashboard.accountingPeriods.length > 0
  ) {
    const firstPeriod = dashboard.accountingPeriods.at(0);
    const lastPeriod = dashboard.accountingPeriods.at(-1);
    if (
      typeof firstPeriod === "undefined" ||
      typeof lastPeriod === "undefined"
    ) {
      return "No range selected";
    }
    return firstPeriod.accountingPeriodId === lastPeriod.accountingPeriodId
      ? firstPeriod.accountingPeriodName
      : `${firstPeriod.accountingPeriodName} to ${lastPeriod.accountingPeriodName}`;
  }

  const dates = dashboard.dates ?? [];
  const firstDate = dates.at(0);
  const lastDate = dates.at(-1);

  if (typeof firstDate === "undefined" || typeof lastDate === "undefined") {
    return "No range selected";
  }

  return firstDate.date === lastDate.date
    ? formatDateLabel(firstDate.date)
    : `${formatDateLabel(firstDate.date)} to ${formatDateLabel(lastDate.date)}`;
};

const fetchAccountsDashboard = async function (
  searchParams: URLSearchParams,
): Promise<AccountDashboardResult> {
  const apiUrl = process.env["API_URL"];
  if (typeof apiUrl !== "string" || apiUrl === "") {
    throw new Error("API_URL is not configured for the frontend.");
  }

  const response = await fetch(
    `${apiUrl}/accounts/dashboard?${searchParams.toString()}`,
    {
      cache: "no-store",
    },
  );
  if (!response.ok) {
    const errorPayload: unknown = await response.json().catch(() => null);
    throw new Error(
      getErrorMessage(errorPayload) ?? "Failed to fetch Accounts dashboard.",
    );
  }

  const dashboardPayload: unknown = await response.json();
  if (!isAccountDashboard(dashboardPayload)) {
    throw new Error(
      "Accounts dashboard response did not match the expected shape.",
    );
  }

  return dashboardPayload;
};

/**
 * Component that displays the Accounts view.
 */
const AccountOverviewDashboard = async function ({
  searchParams,
}: AccountOverviewDashboardProps): Promise<JSX.Element> {
  const {
    search,
    sort,
    page,
    mode,
    accountType,
    startAccountingPeriodId,
    endAccountingPeriodId,
    startDate,
    endDate,
  } = await searchParams;

  const apiClient = getApiClient();
  const accountingPeriodsPromise = apiClient.GET("/accounting-periods", {
    params: {
      query: {
        Search: "",
        Sort: AccountingPeriodSortOrder.DateDescending,
        Limit: 500,
        Offset: 0,
      },
    },
  });

  const [{ data: accountingPeriods }] = await Promise.all([
    accountingPeriodsPromise,
  ]);
  if (typeof accountingPeriods === "undefined") {
    throw new Error("Failed to fetch accounting periods");
  }

  const sortedAccountingPeriodsDescending = [...accountingPeriods.items].sort(
    compareAccountingPeriodsDescending,
  );
  const sortedAccountingPeriodsAscending = [...accountingPeriods.items].sort(
    compareAccountingPeriodsAscending,
  );
  const currentAccountingPeriod =
    sortedAccountingPeriodsDescending.find(
      (accountingPeriod) => accountingPeriod.isOpen,
    ) ??
    sortedAccountingPeriodsDescending[0] ??
    null;
  const isInOnboardingMode = currentAccountingPeriod === null;
  const currentSearch = search?.trim() ?? "";
  const hasActiveSearch = currentSearch !== "";

  if (isInOnboardingMode) {
    return (
      <Stack spacing={3} sx={{ maxWidth: 1280 }}>
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
          <Stack spacing={2.5}>
            <Typography variant="overline" color="text.secondary">
              Accounts workspace
            </Typography>
            <Typography variant="h3">Accounts dashboard</Typography>
            <Typography color="text.secondary" maxWidth={760}>
              Start onboarding to create your first accounting period and
              account structure. The dashboard becomes available as soon as
              there is range data to review.
            </Typography>
            <Stack
              direction={{ xs: "column", sm: "row" }}
              spacing={1.5}
              useFlexGap
            >
              <Button variant="contained" href={routes.onboard}>
                Start onboarding
              </Button>
              <Button variant="outlined" href={routes.create({})}>
                Create account
              </Button>
            </Stack>
          </Stack>
        </Paper>
      </Stack>
    );
  }

  const defaultDateRange = getPeriodDateRange(currentAccountingPeriod);
  const currentMode: AccountsDashboardFilterMode =
    mode === "date" ? "date" : defaultFilterMode;
  const currentPage = parsePageNumber(page);
  const persistedFilters = {
    ...(currentSearch === "" ? {} : { search: currentSearch }),
    ...(typeof sort === "string" ? { sort } : {}),
    ...(typeof accountType === "string" ? { accountType } : {}),
  };

  if (
    currentMode === "accounting-period" &&
    (typeof startAccountingPeriodId === "undefined" ||
      typeof endAccountingPeriodId === "undefined")
  ) {
    redirect(
      routes.index({
        mode: defaultFilterMode,
        ...persistedFilters,
        startAccountingPeriodId: currentAccountingPeriod.id,
        endAccountingPeriodId: currentAccountingPeriod.id,
      }),
    );
  }

  if (
    currentMode === "date" &&
    (typeof startDate === "undefined" || typeof endDate === "undefined")
  ) {
    redirect(
      routes.index({
        mode: "date",
        ...persistedFilters,
        startDate: defaultDateRange.startDate,
        endDate: defaultDateRange.endDate,
      }),
    );
  }

  const dashboardRequestParams = new URLSearchParams();
  if (currentSearch !== "") {
    dashboardRequestParams.set("Search", currentSearch);
  }
  if (typeof sort === "string") {
    dashboardRequestParams.set("Sort", sort);
  }
  if (typeof accountType === "string") {
    dashboardRequestParams.set("AccountType", accountType);
  }
  dashboardRequestParams.set("Limit", rowsPerPage.toString());
  dashboardRequestParams.set(
    "Offset",
    ((currentPage - 1) * rowsPerPage).toString(),
  );
  if (currentMode === "date") {
    dashboardRequestParams.set(
      "StartDate",
      startDate ?? defaultDateRange.startDate,
    );
    dashboardRequestParams.set("EndDate", endDate ?? defaultDateRange.endDate);
  } else {
    dashboardRequestParams.set(
      "StartAccountingPeriodId",
      startAccountingPeriodId ?? currentAccountingPeriod.id,
    );
    dashboardRequestParams.set(
      "EndAccountingPeriodId",
      endAccountingPeriodId ?? currentAccountingPeriod.id,
    );
  }

  const dashboard = await fetchAccountsDashboard(dashboardRequestParams);
  const snapshot = getDashboardSnapshot(dashboard);
  const trendPoints = getTrendPoints(dashboard);
  const visibleCount = dashboard.accounts.items.length;
  const trackedUntrackedTotal =
    Math.abs(snapshot.trackedEndingBalance) +
    Math.abs(snapshot.untrackedEndingBalance);
  const trackedShare =
    trackedUntrackedTotal === 0
      ? 0
      : (Math.abs(snapshot.trackedEndingBalance) / trackedUntrackedTotal) * 100;
  const rangeChange =
    snapshot.totalEndingBalance - snapshot.totalStartingBalance;
  const rangeLabel = getDashboardRangeLabel(dashboard);
  const dateModeHref = routes.index({
    mode: "date",
    startDate: defaultDateRange.startDate,
    endDate: defaultDateRange.endDate,
  });
  const defaultDashboardHref = routes.index({
    mode: defaultFilterMode,
    startAccountingPeriodId: currentAccountingPeriod.id,
    endAccountingPeriodId: currentAccountingPeriod.id,
  });

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
                Review account balances across accounting periods or date ranges
                from a single dashboard query, then move straight into the
                accounts that need attention.
              </Typography>
              <Typography variant="body2" color="text.secondary">
                {hasActiveSearch
                  ? `Showing ${visibleCount} of ${dashboard.accounts.totalCount} matching accounts for ${rangeLabel}.`
                  : `Showing ${visibleCount} accounts on this page across ${dashboard.accounts.totalCount} total accounts for ${rangeLabel}.`}
              </Typography>
            </Stack>
            <Stack direction="row" spacing={1.5} flexWrap="wrap" useFlexGap>
              <Button variant="contained" href={routes.create({})}>
                Create account
              </Button>
              <Button variant="outlined" href={dateModeHref}>
                Current period by date
              </Button>
              <Button variant="outlined" href={defaultDashboardHref}>
                Reset to current period
              </Button>
            </Stack>
          </Stack>
          <AccountsDashboardControls
            accountingPeriods={sortedAccountingPeriodsAscending}
            defaultAccountingPeriodId={currentAccountingPeriod.id}
            defaultStartDate={defaultDateRange.startDate}
            defaultEndDate={defaultDateRange.endDate}
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
          title={snapshot.startLabel}
          value={formatCurrency(snapshot.totalStartingBalance)}
          description="Starting balance for the selected range"
        />
        <SummaryCard
          title={snapshot.endLabel}
          value={formatCurrency(snapshot.totalEndingBalance)}
          description="Ending balance for the selected range"
        />
        <SummaryCard
          title="Net change"
          value={formatCurrency(rangeChange)}
          description="Total change from the start of the range to the end"
        />
        <SummaryCard
          title="Accounts In Scope"
          value={dashboard.accounts.totalCount}
          description={
            hasActiveSearch
              ? `Filtered by "${currentSearch}" with ${visibleCount} account${visibleCount === 1 ? "" : "s"} visible on this page.`
              : `${visibleCount} account${visibleCount === 1 ? "" : "s"} visible on this page.`
          }
        />
      </Box>
      <Box
        sx={{
          display: "grid",
          gap: 3,
          gridTemplateColumns: {
            xs: "1fr",
            xl: "minmax(0, 1.15fr) minmax(0, 0.85fr)",
          },
        }}
      >
        <Stack spacing={2}>
          <AccountBalanceTrendPanel
            points={trendPoints}
            modeLabel={
              dashboard.mode === accountDashboardMode.AccountingPeriod
                ? "Accounting periods"
                : "Dates"
            }
          />
          <AccountTypeComparisonPanel
            startLabel={snapshot.startLabel}
            endLabel={snapshot.endLabel}
            startingBalances={snapshot.startingBalancesByType}
            endingBalances={snapshot.endingBalancesByType}
          />
          <Paper sx={{ border: "1px solid", borderColor: "divider", p: 3 }}>
            <Stack spacing={0.75}>
              <Typography variant="h5">Accounts in range</Typography>
              <Typography variant="body2" color="text.secondary">
                The dashboard page stays paged for scanning, while the table
                reflects the same range, search, and type filters used by the
                summary visuals.
              </Typography>
            </Stack>
          </Paper>
          <AccountsDashboardListFrame
            data={[...dashboard.accounts.items]}
            isInOnboardingMode={isInOnboardingMode}
            totalCount={dashboard.accounts.totalCount}
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
          <AccountLargestMoversPanel accounts={dashboard.accounts.items} />
          <Paper sx={{ border: "1px solid", borderColor: "divider", p: 3 }}>
            <Stack spacing={2}>
              <Typography variant="h6">Current range</Typography>
              <Stack spacing={1.25}>
                <Box
                  sx={{
                    display: "flex",
                    justifyContent: "space-between",
                    gap: 2,
                  }}
                >
                  <Typography variant="body2" color="text.secondary">
                    Mode
                  </Typography>
                  <Typography
                    variant="body2"
                    fontWeight={600}
                    textAlign="right"
                  >
                    {dashboard.mode === accountDashboardMode.AccountingPeriod
                      ? "Accounting periods"
                      : "Dates"}
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
                    Range
                  </Typography>
                  <Typography
                    variant="body2"
                    fontWeight={600}
                    textAlign="right"
                  >
                    {rangeLabel}
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
                    {visibleCount} of {dashboard.accounts.totalCount}
                  </Typography>
                </Box>
              </Stack>
            </Stack>
          </Paper>
          <Paper sx={{ border: "1px solid", borderColor: "divider", p: 3 }}>
            <Stack spacing={2}>
              <Typography variant="h6">Ending balance mix</Typography>
              <Stack spacing={1.5}>
                <Stack spacing={0.75}>
                  <Typography variant="body2" color="text.secondary">
                    Tracked balances
                  </Typography>
                  <Typography variant="h6">
                    {formatCurrency(snapshot.trackedEndingBalance)}
                  </Typography>
                </Stack>
                <Stack spacing={0.75}>
                  <Typography variant="body2" color="text.secondary">
                    Tracked share
                  </Typography>
                  <Typography variant="h6">
                    {trackedShare.toFixed(0)}%
                  </Typography>
                </Stack>
                <Stack spacing={0.75}>
                  <Typography variant="body2" color="text.secondary">
                    Untracked balances
                  </Typography>
                  <Typography variant="h6">
                    {formatCurrency(snapshot.untrackedEndingBalance)}
                  </Typography>
                </Stack>
              </Stack>
            </Stack>
          </Paper>
          <Paper sx={{ border: "1px solid", borderColor: "divider", p: 3 }}>
            <Stack spacing={2}>
              <Typography variant="h6">Next actions</Typography>
              <Typography variant="body2" color="text.secondary">
                Use the range filters to narrow the dashboard, then open an
                account when you need row-level transaction detail.
              </Typography>
              <Stack spacing={1.25}>
                <Button variant="contained" href={routes.create({})}>
                  Create account
                </Button>
                <Button variant="outlined" href={dateModeHref}>
                  Inspect current month by date
                </Button>
                <Button variant="outlined" href={defaultDashboardHref}>
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

export type { AccountOverviewDashboardSearchParams };
export default AccountOverviewDashboard;
