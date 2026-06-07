import type {
  AccountDashboardBalanceEventSortOrder,
  AccountDashboardSortOrder,
  AccountType,
} from "@/accounts/types";
import { Box, Stack } from "@mui/material";
import { getPageOffset, normalizePageValue } from "@/framework/listframe/page";
import {
  normalizeAccountTypes,
  shouldPersistAccountTypes,
} from "@/accounts/dashboard/accountTypeFilter";
import {
  normalizeRequestedAccountNames,
  shouldPersistAccountNames,
} from "@/accounts/dashboard/accountNameFilter";
import AccountDashboardBalanceEventListFrame from "@/accounts/dashboard/AccountDashboardBalanceEventListFrame";
import AccountDashboardChangeChart from "@/accounts/dashboard/AccountDashboardChangeChart";
import AccountDashboardFilter from "@/accounts/dashboard/AccountDashboardFilter";
import AccountDashboardIncomeSpendingCard from "@/accounts/dashboard/AccountDashboardIncomeSpendingCard";
import AccountDashboardListFrame from "@/accounts/dashboard/AccountDashboardListFrame";
import AccountDashboardSummaryCards from "@/accounts/dashboard/AccountDashboardSummaryCards";
import AccountDashboardTrendChart from "@/accounts/dashboard/AccountDashboardTrendChart";
import { AccountingPeriodSortOrder } from "@/accounting-periods/types";
import type { JSX } from "react";
import dayjs from "dayjs";
import getApiClient from "@/framework/data/getApiClient";
import { redirect } from "next/navigation";
import routes from "@/accounts/routes";
import { rowsPerPage } from "@/framework/listframe/Constants";

/**
 * URL mode values used to filter the Accounts dashboard.
 */
type AccountsDashboardFilterMode = "accounting-period" | "date";

/**
 * Search parameters for the account dashboard.
 */
interface AccountDashboardSearchParams {
  sort?: AccountDashboardSortOrder;
  page?: number | string | null;
  balanceEventSort?: AccountDashboardBalanceEventSortOrder;
  balanceEventPage?: number | string | null;
  mode?: AccountsDashboardFilterMode;
  accountType?: AccountType | readonly AccountType[];
  accountName?: string | readonly string[];
  startAccountingPeriodId?: string;
  endAccountingPeriodId?: string;
  startDate?: string;
  endDate?: string;
}

/**
 * Props for the AccountDashboard component.
 */
interface AccountDashboardProps {
  readonly searchParams: Promise<AccountDashboardSearchParams>;
}

/**
 * Component that displays the Accounts view.
 */
const AccountDashboard = async function ({
  searchParams,
}: AccountDashboardProps): Promise<JSX.Element> {
  const {
    sort,
    page,
    balanceEventSort,
    balanceEventPage,
    mode,
    accountType,
    accountName,
    startAccountingPeriodId,
    endAccountingPeriodId,
    startDate,
    endDate,
  } = await searchParams;

  const defaultEndDate = dayjs();
  const defaultStartDate = defaultEndDate.subtract(90, "day");

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
  const { data: accountingPeriods } = await accountingPeriodsPromise;
  const latestAccountingPeriod = accountingPeriods?.items[0] ?? null;
  const isInOnboardingMode = typeof latestAccountingPeriod === "undefined";
  const currentMode: AccountsDashboardFilterMode =
    typeof mode === "undefined" || isInOnboardingMode ? "date" : mode;
  const currentAccountTypes = normalizeAccountTypes(
    Array.isArray(accountType)
      ? accountType
      : typeof accountType === "string"
        ? [accountType]
        : [],
  );
  const currentAccountNames = normalizeRequestedAccountNames(
    Array.isArray(accountName)
      ? accountName
      : typeof accountName === "string"
        ? [accountName]
        : [],
  );
  const currentPage = normalizePageValue(page);
  const currentBalanceEventPage = normalizePageValue(balanceEventPage);

  const persistedFilters = {
    ...(typeof sort === "string" ? { sort } : {}),
    ...(typeof balanceEventSort === "string" ? { balanceEventSort } : {}),
    ...(shouldPersistAccountTypes(currentAccountTypes)
      ? { accountType: currentAccountTypes }
      : {}),
    ...(shouldPersistAccountNames(currentAccountNames)
      ? { accountName: currentAccountNames }
      : {}),
  };

  if (
    (currentMode === "date" &&
      (typeof startDate === "undefined" || typeof endDate === "undefined")) ||
    (currentMode === "accounting-period" && latestAccountingPeriod === null)
  ) {
    redirect(
      routes.dashboard({
        mode: "date",
        ...persistedFilters,
        startDate: defaultStartDate.format("YYYY-MM-DD"),
        endDate: defaultEndDate.format("YYYY-MM-DD"),
      }),
    );
  }

  if (
    currentMode === "accounting-period" &&
    latestAccountingPeriod !== null &&
    (typeof startAccountingPeriodId === "undefined" ||
      typeof endAccountingPeriodId === "undefined")
  ) {
    redirect(
      routes.dashboard({
        mode: "date",
        ...persistedFilters,
        startAccountingPeriodId: latestAccountingPeriod.id,
        endAccountingPeriodId: latestAccountingPeriod.id,
      }),
    );
  }

  const accountDashboardPromise = apiClient.GET("/accounts/dashboard", {
    params: {
      query: {
        ...(typeof sort === "string" ? { Sort: sort } : {}),
        ...(typeof balanceEventSort === "string"
          ? { BalanceEventSort: balanceEventSort }
          : {}),
        Limit: rowsPerPage,
        BalanceEventLimit: rowsPerPage,
        ...(shouldPersistAccountTypes(currentAccountTypes)
          ? { AccountType: [...currentAccountTypes] }
          : {}),
        ...(shouldPersistAccountNames(currentAccountNames)
          ? { AccountName: [...currentAccountNames] }
          : {}),
        Offset: getPageOffset(currentPage),
        BalanceEventOffset: getPageOffset(currentBalanceEventPage),
        ...(currentMode === "date"
          ? {
              StartDate:
                typeof startDate === "string"
                  ? startDate
                  : defaultStartDate.format("YYYY-MM-DD"),
              EndDate:
                typeof endDate === "string"
                  ? endDate
                  : defaultEndDate.format("YYYY-MM-DD"),
            }
          : {}),
        ...(currentMode === "accounting-period" &&
        latestAccountingPeriod !== null
          ? {
              StartAccountingPeriodId:
                typeof startAccountingPeriodId === "string"
                  ? startAccountingPeriodId
                  : latestAccountingPeriod.id,
              EndAccountingPeriodId:
                typeof endAccountingPeriodId === "string"
                  ? endAccountingPeriodId
                  : latestAccountingPeriod.id,
            }
          : {}),
      },
    },
  });
  const { data: dashboard } = await accountDashboardPromise;
  if (typeof dashboard === "undefined") {
    throw new Error("Failed to load account dashboard data");
  }

  return (
    <Stack spacing={3} sx={{ width: "100%" }}>
      <Stack spacing={3} sx={{ maxWidth: 1440, width: "100%" }}>
        <AccountDashboardFilter
          accountingPeriods={accountingPeriods?.items ?? []}
          availableAccountNames={dashboard.availableAccountNames}
          defaultAccountingPeriodId={latestAccountingPeriod?.id ?? null}
          defaultStartDate={defaultStartDate.format("YYYY-MM-DD")}
          defaultEndDate={defaultEndDate.format("YYYY-MM-DD")}
        />
      </Stack>
      <AccountDashboardSummaryCards dashboard={dashboard} />
      <AccountDashboardIncomeSpendingCard dashboard={dashboard} />
      <Box
        sx={{
          display: "grid",
          gap: 3,
          gridTemplateColumns: {
            xs: "1fr",
            lg: "minmax(0, 1fr) minmax(0, 1fr)",
          },
        }}
      >
        <AccountDashboardTrendChart
          mode={dashboard.mode}
          accountingPeriods={dashboard.accountingPeriods ?? null}
          dates={dashboard.dates ?? null}
        />
        <AccountDashboardChangeChart
          mode={dashboard.mode}
          accountingPeriods={dashboard.accountingPeriods ?? null}
          dates={dashboard.dates ?? null}
        />
      </Box>
      <Box
        sx={{
          display: "grid",
          gap: 3,
          gridTemplateColumns:
            "repeat(auto-fit, minmax(min(100%, 800px), 1fr))",
        }}
      >
        <AccountDashboardListFrame
          data={[...dashboard.accounts.items]}
          isInOnboardingMode={isInOnboardingMode}
          totalCount={dashboard.accounts.totalCount}
        />
        <AccountDashboardBalanceEventListFrame
          data={[...dashboard.balanceEvents.items]}
          mode={dashboard.mode}
          totalCount={dashboard.balanceEvents.totalCount}
        />
      </Box>
    </Stack>
  );
};

export type { AccountDashboardSearchParams };
export default AccountDashboard;
