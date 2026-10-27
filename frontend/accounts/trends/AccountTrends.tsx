import {
  type AccountTrendsDataMode,
  type AccountTrendsSearchParams,
  getAccountTrendsSnapshot,
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
  matchesTrendRange,
  resolveTrendRange,
} from "@/framework/routes/trendRange";
import {
  normalizeAccountTypes,
  shouldPersistAccountTypes,
} from "@/accounts/accountTypeFilterHelpers";
import {
  normalizeRequestedAccountNames,
  shouldPersistAccountNames,
} from "@/accounts/accountNameFilterHelpers";
import AccountBalanceSummaryCards from "@/accounts/AccountBalanceSummaryCards";
import AccountTrendsFilter from "@/accounts/trends/AccountTrendsFilter";
import AccountTrendsListFrame from "@/accounts/trends/AccountTrendsListFrame";
import { AccountingPeriodSort } from "@/accounting-periods/types";
import { BalanceGranularityModel } from "@/framework/data/api";
import BalanceTrendChart from "@/framework/charts/BalanceTrendChart";
import BalanceTrendsChangeChart from "@/framework/charts/BalanceTrendsChangeChart";
import ConstrainedContent from "@/framework/view/ConstrainedContent";
import type { JSX } from "react";
import PageLayout from "@/framework/view/PageLayout";
import ResponsiveGrid from "@/framework/view/ResponsiveGrid";
import TrendsBackLink from "@/framework/view/TrendsBackLink";
import { buildBalanceTrendChartPoints } from "@/framework/charts/balanceTrendHelpers";
import createApiClient from "@/framework/data/createApiClient";
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
  const resolvedSearchParams = await searchParams;
  const { returnUrl, sort, page, pageSize, accountType, accountName } =
    resolvedSearchParams;

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
  const isInOnboardingMode = accountingPeriods.items.length === 0;
  const range = resolveTrendRange(
    resolvedSearchParams,
    accountingPeriods.items,
  );
  const currentMode = range.mode;
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

  if (!matchesTrendRange(resolvedSearchParams, range)) {
    redirect(routes.trends({ returnUrl, ...persistedFilters, ...range }));
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
    if (range.mode === "date") {
      const rangeQuery = {
        "Range.Start": range.startDate,
        "Range.End": range.endDate,
      };
      return unwrapApiResponse(
        await apiClient.GET("/accounts/date-range", {
          params: { query: { ...accountQuery, ...rangeQuery } },
        }),
        "Failed to load account trends",
      );
    }
    const rangeQuery = {
      "Range.Start": range.startAccountingPeriodId,
      "Range.End": range.endAccountingPeriodId,
    };
    return unwrapApiResponse(
      await apiClient.GET("/accounts/accounting-period-range", {
        params: { query: { ...accountQuery, ...rangeQuery } },
      }),
      "Failed to load account trends",
    );
  })();
  const modeValue: AccountTrendsDataMode =
    currentMode === "date" ? "Date" : "AccountingPeriod";
  const periodSummaries =
    "accountingPeriods" in trends ? trends.accountingPeriods : [];
  const dateSummaries = "dates" in trends ? trends.dates : [];
  const openingBalance = "dates" in trends ? trends.openingBalance : null;
  const granularity =
    "dates" in trends ? trends.granularity : BalanceGranularityModel.Day;
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
    granularity,
  });
  const currentRange =
    range.mode === "date"
      ? { startDate: range.startDate, endDate: range.endDate }
      : {
          startAccountingPeriodId: range.startAccountingPeriodId,
          endAccountingPeriodId: range.endAccountingPeriodId,
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
      returnUrl,
      mode: currentMode,
      ...persistedFilters,
      ...currentRange,
    }),
  });
  const snapshot = getAccountTrendsSnapshot(
    modeValue,
    periodSummaries,
    dateSummaries,
    openingBalance,
    range.mode === "date" ? range.startDate : null,
  );

  return (
    <PageLayout>
      <TrendsBackLink
        returnUrl={returnUrl}
        workspace="accounts"
        label="Back to Account Details"
      />
      <ConstrainedContent>
        <AccountTrendsFilter
          accountingPeriods={accountingPeriods.items}
          availableAccountNames={trends.availableAccountNames}
        />
      </ConstrainedContent>
      <AccountBalanceSummaryCards
        startingLabel={snapshot.startLabel}
        endingLabel={snapshot.endLabel}
        startingBalance={snapshot.startingBalance}
        endingBalance={snapshot.endingBalance}
      />
      <ResponsiveGrid columns={{ xs: 1, lg: 2 }}>
        <BalanceTrendChart
          chartPoints={balanceTrendChartPoints}
          xAxisLabel={modeValue === "Date" ? "Date" : "Accounting Period"}
        />
        <BalanceTrendsChangeChart
          mode={modeValue}
          accountingPeriods={periodSummaries}
          dates={dateSummaries}
          openingBalance={openingBalance?.totalBalance ?? null}
          granularity={granularity}
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

export default AccountTrends;
