import type {
  AccountTrendsBalanceEventSortOrder,
  AccountTrendsSortOrder,
  AccountType,
} from "@/accounts/types";
import { Box, Stack } from "@mui/material";
import { getPageOffset, normalizePageValue } from "@/framework/listframe/page";
import {
  normalizeAccountTypes,
  shouldPersistAccountTypes,
} from "@/accounts/trends/accountTypeFilter";
import {
  normalizeRequestedAccountNames,
  shouldPersistAccountNames,
} from "@/accounts/trends/accountNameFilter";
import AccountTrendChart from "@/accounts/trends/AccountTrendChart";
import AccountTrendsBalanceEventListFrame from "@/accounts/trends/AccountTrendsBalanceEventListFrame";
import AccountTrendsChangeChart from "@/accounts/trends/AccountTrendsChangeChart";
import AccountTrendsFilter from "@/accounts/trends/AccountTrendsFilter";
import AccountTrendsIncomeSpendingCard from "@/accounts/trends/AccountTrendsIncomeSpendingCard";
import AccountTrendsListFrame from "@/accounts/trends/AccountTrendsListFrame";
import AccountTrendsSummaryCards from "@/accounts/trends/AccountTrendsSummaryCards";
import { AccountingPeriodSortOrder } from "@/accounting-periods/types";
import type { JSX } from "react";
import dayjs from "dayjs";
import getApiClient from "@/framework/data/getApiClient";
import { redirect } from "next/navigation";
import routes from "@/accounts/routes";
import { rowsPerPage } from "@/framework/listframe/Constants";

/**
 * URL mode values used to filter the Accounts trends.
 */
type AccountsTrendsFilterMode = "accounting-period" | "date";

/**
 * Search parameters for the account trends.
 */
interface AccountTrendsSearchParams {
  sort?: AccountTrendsSortOrder;
  page?: number | string | null;
  balanceEventSort?: AccountTrendsBalanceEventSortOrder;
  balanceEventPage?: number | string | null;
  mode?: AccountsTrendsFilterMode;
  accountType?: AccountType | readonly AccountType[];
  accountName?: string | readonly string[];
  startAccountingPeriodId?: string;
  endAccountingPeriodId?: string;
  startDate?: string;
  endDate?: string;
}

/**
 * Props for the AccountTrends component.
 */
interface AccountTrendsProps {
  readonly searchParams: Promise<AccountTrendsSearchParams>;
}

/**
 * Component that displays the Accounts view.
 */
const AccountTrends = async function ({
  searchParams,
}: AccountTrendsProps): Promise<JSX.Element> {
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
  const currentMode: AccountsTrendsFilterMode =
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
      routes.trends({
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
      routes.trends({
        mode: "date",
        ...persistedFilters,
        startAccountingPeriodId: latestAccountingPeriod.id,
        endAccountingPeriodId: latestAccountingPeriod.id,
      }),
    );
  }

  const accountTrendsPromise = apiClient.GET("/accounts/trends", {
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
  const { data: trends } = await accountTrendsPromise;
  if (typeof trends === "undefined") {
    throw new Error("Failed to load account trends data");
  }

  return (
    <Stack spacing={3} sx={{ width: "100%" }}>
      <Stack spacing={3} sx={{ maxWidth: 1440, width: "100%" }}>
        <AccountTrendsFilter
          accountingPeriods={accountingPeriods?.items ?? []}
          availableAccountNames={trends.availableAccountNames}
          defaultAccountingPeriodId={latestAccountingPeriod?.id ?? null}
          defaultStartDate={defaultStartDate.format("YYYY-MM-DD")}
          defaultEndDate={defaultEndDate.format("YYYY-MM-DD")}
        />
      </Stack>
      <AccountTrendsSummaryCards trends={trends} />
      <AccountTrendsIncomeSpendingCard trends={trends} />
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
        <AccountTrendChart
          mode={trends.mode}
          accountingPeriods={trends.accountingPeriods ?? null}
          dates={trends.dates ?? null}
        />
        <AccountTrendsChangeChart
          mode={trends.mode}
          accountingPeriods={trends.accountingPeriods ?? null}
          dates={trends.dates ?? null}
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
        <AccountTrendsListFrame
          data={[...trends.accounts.items]}
          isInOnboardingMode={isInOnboardingMode}
          totalCount={trends.accounts.totalCount}
        />
        <AccountTrendsBalanceEventListFrame
          data={[...trends.balanceEvents.items]}
          mode={trends.mode}
          totalCount={trends.balanceEvents.totalCount}
        />
      </Box>
    </Stack>
  );
};

export type { AccountTrendsSearchParams };
export default AccountTrends;
