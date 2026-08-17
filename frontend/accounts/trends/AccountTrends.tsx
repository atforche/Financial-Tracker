import type {
  AccountTrendsDataMode,
  AccountTrendsSearchParams,
} from "@/accounts/trends/helpers";
import type {
  AccountsInAccountingPeriodRange,
  AccountsInDateRange,
} from "@/accounts/types";
import {
  getPageOffset,
  getRowsPerPage,
  normalizePageValue,
} from "@/framework/listframe/page";
import {
  normalizeAccountTypes,
  shouldPersistAccountTypes,
} from "@/accounts/accountTypeFilterHelpers";
import {
  normalizeRequestedAccountNames,
  shouldPersistAccountNames,
} from "@/accounts/accountNameFilterHelpers";
import AccountTrendsChangeChart from "@/accounts/trends/AccountTrendsChangeChart";
import AccountTrendsFilter from "@/accounts/trends/AccountTrendsFilter";
import AccountTrendsListFrame from "@/accounts/trends/AccountTrendsListFrame";
import AccountTrendsSummaryCards from "@/accounts/trends/AccountTrendsSummaryCards";
import { AccountingPeriodSort } from "@/accounting-periods/types";
import BalanceTrendChart from "@/framework/charts/BalanceTrendChart";
import ConstrainedContent from "@/framework/view/ConstrainedContent";
import type { JSX } from "react";
import PageLayout from "@/framework/view/PageLayout";
import ResponsiveGrid from "@/framework/view/ResponsiveGrid";
import ResponsivePageSize from "@/framework/listframe/ResponsivePageSize";
import type { TrendRangeMode } from "@/framework/routes/trendRange";
import { buildBalanceTrendChartPoints } from "@/framework/charts/balanceTrendHelpers";
import createApiClient from "@/framework/data/createApiClient";
import dayjs from "dayjs";
import { redirect } from "next/navigation";
import routes from "@/accounts/routes";
import { toRepeatedSearchParams } from "@/framework/routes/helpers";
import transactionRoutes from "@/transactions/routes";
import unwrapApiResponse from "@/framework/data/unwrapApiResponse";

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
    pageSize,
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

  const apiClient = await createApiClient();
  const accountingPeriodsPromise = apiClient.GET("/accounting-periods", {
    params: {
      query: {
        Sort: AccountingPeriodSort.DateDescending,
        Limit: 500,
        Offset: 0,
      },
    },
  });
  const accountingPeriods = unwrapApiResponse(
    await accountingPeriodsPromise,
    "Failed to fetch accounting periods",
  );
  const latestAccountingPeriod = accountingPeriods.items[0] ?? null;
  const isInOnboardingMode = latestAccountingPeriod === null;
  const currentMode: TrendRangeMode =
    typeof mode === "undefined" || isInOnboardingMode ? "date" : mode;
  const currentAccountTypes = normalizeAccountTypes(
    toRepeatedSearchParams(accountType),
  );
  const currentAccountNames = normalizeRequestedAccountNames(
    toRepeatedSearchParams(accountName),
  );
  const currentPage = normalizePageValue(page);
  const rowsPerPage = getRowsPerPage(pageSize);

  const persistedFilters = {
    ...(typeof sort === "string" ? { sort } : {}),
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
        mode: "accounting-period",
        ...persistedFilters,
        startAccountingPeriodId: latestAccountingPeriod.id,
        endAccountingPeriodId: latestAccountingPeriod.id,
      }),
    );
  }

  const filterQuery = {
    ...(shouldPersistAccountTypes(currentAccountTypes)
      ? { "Filter.Types": [...currentAccountTypes] }
      : {}),
    ...(shouldPersistAccountNames(currentAccountNames)
      ? { "Filter.Names": [...currentAccountNames] }
      : {}),
  };
  const accountQuery = {
    ...(typeof sort === "string" ? { Sort: sort } : {}),
    ...filterQuery,
    Limit: rowsPerPage,
    Offset: getPageOffset(currentPage, rowsPerPage),
  };
  const trends = await (async function (): Promise<
    AccountsInDateRange | AccountsInAccountingPeriodRange
  > {
    if (currentMode === "date") {
      const range = {
        "Range.Start": startDate ?? defaultStartDate.format("YYYY-MM-DD"),
        "Range.End": endDate ?? defaultEndDate.format("YYYY-MM-DD"),
      };
      return unwrapApiResponse(
        await apiClient.GET("/accounts/date-range", {
          params: { query: { ...accountQuery, ...range } },
        }),
        "Failed to load account trends",
      );
    }
    const range = {
      "Range.Start":
        startAccountingPeriodId ?? latestAccountingPeriod?.id ?? "",
      "Range.End": endAccountingPeriodId ?? latestAccountingPeriod?.id ?? "",
    };
    return unwrapApiResponse(
      await apiClient.GET("/accounts/accounting-period-range", {
        params: { query: { ...accountQuery, ...range } },
      }),
      "Failed to load account trends",
    );
  })();
  const modeValue: AccountTrendsDataMode =
    currentMode === "date" ? "Date" : "AccountingPeriod";
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
  const currentRange =
    currentMode === "date"
      ? {
          startDate: startDate ?? defaultStartDate.format("YYYY-MM-DD"),
          endDate: endDate ?? defaultEndDate.format("YYYY-MM-DD"),
        }
      : {
          startAccountingPeriodId:
            startAccountingPeriodId ?? latestAccountingPeriod?.id ?? "",
          endAccountingPeriodId:
            endAccountingPeriodId ?? latestAccountingPeriod?.id ?? "",
        };
  const transactionWorkspaceHref = transactionRoutes.workspace({
    ...(currentMode === "date"
      ? currentRange
      : {
          accountingPeriodIds: accountingPeriods.items
            .slice(
              Math.min(
                accountingPeriods.items.findIndex(
                  (period) =>
                    period.id === currentRange.startAccountingPeriodId,
                ),
                accountingPeriods.items.findIndex(
                  (period) => period.id === currentRange.endAccountingPeriodId,
                ),
              ),
              Math.max(
                accountingPeriods.items.findIndex(
                  (period) =>
                    period.id === currentRange.startAccountingPeriodId,
                ),
                accountingPeriods.items.findIndex(
                  (period) => period.id === currentRange.endAccountingPeriodId,
                ),
              ) + 1,
            )
            .map((period) => period.id),
        }),
    ...(shouldPersistAccountTypes(currentAccountTypes)
      ? { accountTypes: currentAccountTypes }
      : {}),
    ...(shouldPersistAccountNames(currentAccountNames)
      ? { accountNames: currentAccountNames }
      : {}),
    returnUrl: routes.trends({
      mode: currentMode,
      ...persistedFilters,
      ...currentRange,
    }),
  });

  return (
    <PageLayout>
      <ResponsivePageSize desktopBreakpoint="xl" />
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
      <AccountTrendsListFrame
        data={trends.accounts.items}
        isInOnboardingMode={isInOnboardingMode}
        totalCount={trends.accounts.totalCount}
        transactionWorkspaceHref={transactionWorkspaceHref}
      />
    </PageLayout>
  );
};

export type { AccountTrendsSearchParams };
export default AccountTrends;
