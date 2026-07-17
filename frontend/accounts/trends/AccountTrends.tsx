import type {
  AccountBalanceEventSortValue,
  AccountType,
  AccountWithBalanceRangeSortValue,
  AccountsInAccountingPeriodRange,
  AccountsInDateRange,
} from "@/accounts/types";
import { getPageOffset, normalizePageValue } from "@/framework/listframe/page";
import {
  normalizeAccountTypes,
  shouldPersistAccountTypes,
} from "@/accounts/trends/accountTypeFilter";
import {
  normalizeRequestedAccountNames,
  shouldPersistAccountNames,
} from "@/accounts/trends/accountNameFilter";
import AccountTrendsBalanceEventListFrame from "@/accounts/trends/AccountTrendsBalanceEventListFrame";
import AccountTrendsChangeChart from "@/accounts/trends/AccountTrendsChangeChart";
import AccountTrendsFilter from "@/accounts/trends/AccountTrendsFilter";
import AccountTrendsListFrame from "@/accounts/trends/AccountTrendsListFrame";
import AccountTrendsSummaryCards from "@/accounts/trends/AccountTrendsSummaryCards";
import BalanceTrendChart from "@/framework/charts/BalanceTrendChart";
import ConstrainedContent from "@/framework/view/ConstrainedContent";
import IncomeSpendingCard from "@/transactions/IncomeSpendingCard";
import type { JSX } from "react";
import PageLayout from "@/framework/view/PageLayout";
import ResponsiveGrid from "@/framework/view/ResponsiveGrid";
import { buildBalanceTrendChartPoints } from "@/framework/charts/helpers";
import dayjs from "dayjs";
import getApiClient from "@/framework/data/getApiClient";
import getApiData from "@/framework/data/apiResponse";
import { redirect } from "next/navigation";
import routes from "@/accounts/routes";
import { rowsPerPage } from "@/framework/listframe/Constants";
import { toRepeatedSearchParams } from "@/framework/routes/helpers";

/**
 * URL mode values used to filter the Accounts trends.
 */
type AccountsTrendsFilterMode = "accounting-period" | "date";

/**
 * Search parameters for the account trends.
 */
interface AccountTrendsSearchParams {
  sort?: AccountWithBalanceRangeSortValue;
  page?: number | string | null;
  balanceEventSort?: AccountBalanceEventSortValue;
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
        Sort: "DateDescending",
        Limit: 500,
        Offset: 0,
      },
    },
  });
  const accountingPeriods = getApiData(
    await accountingPeriodsPromise,
    "Failed to fetch accounting periods",
  );
  const latestAccountingPeriod = accountingPeriods.items[0] ?? null;
  const isInOnboardingMode = typeof latestAccountingPeriod === "undefined";
  const currentMode: AccountsTrendsFilterMode =
    typeof mode === "undefined" || isInOnboardingMode ? "date" : mode;
  const currentAccountTypes = normalizeAccountTypes(
    toRepeatedSearchParams(accountType),
  );
  const currentAccountNames = normalizeRequestedAccountNames(
    toRepeatedSearchParams(accountName),
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

  // Values are assigned by the active range mode below.
  // eslint-disable-next-line @typescript-eslint/init-declarations
  let trends: AccountsInDateRange | AccountsInAccountingPeriodRange | undefined;
  // eslint-disable-next-line @typescript-eslint/init-declarations
  let balanceEvents;
  const accountQuery = {
    ...(typeof sort === "string" ? { Sort: sort } : {}),
    ...(shouldPersistAccountTypes(currentAccountTypes)
      ? { "Filter.Types": [...currentAccountTypes] }
      : {}),
    ...(shouldPersistAccountNames(currentAccountNames)
      ? { "Filter.Names": [...currentAccountNames] }
      : {}),
    Limit: rowsPerPage,
    Offset: getPageOffset(currentPage),
  };
  const balanceEventQuery = {
    ...(typeof balanceEventSort === "string" ? { Sort: balanceEventSort } : {}),
    ...(shouldPersistAccountTypes(currentAccountTypes)
      ? { "Filter.Types": [...currentAccountTypes] }
      : {}),
    ...(shouldPersistAccountNames(currentAccountNames)
      ? { "Filter.Names": [...currentAccountNames] }
      : {}),
    Limit: rowsPerPage,
    Offset: getPageOffset(currentBalanceEventPage),
  };
  if (currentMode === "date") {
    const range = {
      "Range.Start": startDate ?? defaultStartDate.format("YYYY-MM-DD"),
      "Range.End": endDate ?? defaultEndDate.format("YYYY-MM-DD"),
    };
    const [accountResponse, balanceEventResponse] = await Promise.all([
      apiClient.GET("/accounts/date-range", {
        params: { query: { ...accountQuery, ...range } },
      }),
      apiClient.GET("/balance-events/accounts/date-range", {
        params: { query: { ...balanceEventQuery, ...range } },
      }),
    ]);
    trends = getApiData(accountResponse, "Failed to load account trends");
    balanceEvents = getApiData(
      balanceEventResponse,
      "Failed to load account balance events",
    );
  } else {
    const range = {
      "Range.Start":
        startAccountingPeriodId ?? latestAccountingPeriod?.id ?? "",
      "Range.End": endAccountingPeriodId ?? latestAccountingPeriod?.id ?? "",
    };
    const [accountResponse, balanceEventResponse] = await Promise.all([
      apiClient.GET("/accounts/accounting-period-range", {
        params: { query: { ...accountQuery, ...range } },
      }),
      apiClient.GET("/balance-events/accounts/accounting-period-range", {
        params: { query: { ...balanceEventQuery, ...range } },
      }),
    ]);
    trends = getApiData(accountResponse, "Failed to load account trends");
    balanceEvents = getApiData(
      balanceEventResponse,
      "Failed to load account balance events",
    );
  }
  const modeValue = currentMode === "date" ? "Date" : "AccountingPeriod";
  const periodSummaries =
    "accountingPeriods" in trends ? trends.accountingPeriods : [];
  const dateSummaries = "dates" in trends ? trends.dates : [];
  const chartPeriods = periodSummaries.map((summary) => ({
    accountingPeriodId: summary.accountingPeriod.id,
    accountingPeriodName: summary.accountingPeriod.name,
    year: summary.accountingPeriod.year,
    month: summary.accountingPeriod.month,
    totalOpeningBalance: summary.openingBalance.totalBalance,
    totalClosingBalance: summary.closingBalance.totalBalance,
  }));
  const balanceTrendChartPoints = buildBalanceTrendChartPoints({
    mode: modeValue,
    accountingPeriods: chartPeriods,
    dates: dateSummaries,
  });

  return (
    <PageLayout>
      <ConstrainedContent>
        <AccountTrendsFilter
          accountingPeriods={accountingPeriods.items}
          availableAccountNames={trends.availableAccountNames}
          defaultAccountingPeriodId={latestAccountingPeriod?.id ?? null}
          defaultStartDate={defaultStartDate.format("YYYY-MM-DD")}
          defaultEndDate={defaultEndDate.format("YYYY-MM-DD")}
        />
      </ConstrainedContent>
      <AccountTrendsSummaryCards
        mode={modeValue}
        accountingPeriods={periodSummaries}
        dates={dateSummaries}
      />
      <IncomeSpendingCard
        totalIncome={trends.totalIncome}
        totalSpending={trends.totalSpending}
      />
      <ResponsiveGrid columns={{ xs: 1, lg: 2 }}>
        <BalanceTrendChart
          chartPoints={balanceTrendChartPoints}
          xAxisLabel={modeValue === "Date" ? "Date" : "Accounting Period"}
        />
        <AccountTrendsChangeChart
          mode={modeValue}
          accountingPeriods={periodSummaries}
          dates={dateSummaries}
        />
      </ResponsiveGrid>
      <ResponsiveGrid minimumColumnWidth={800}>
        <AccountTrendsListFrame
          data={[...trends.accounts.items]}
          isInOnboardingMode={isInOnboardingMode}
          totalCount={trends.accounts.totalCount}
        />
        <AccountTrendsBalanceEventListFrame
          data={[...balanceEvents.items]}
          mode={modeValue}
          totalCount={balanceEvents.totalCount}
        />
      </ResponsiveGrid>
    </PageLayout>
  );
};

export type { AccountTrendsSearchParams };
export default AccountTrends;
