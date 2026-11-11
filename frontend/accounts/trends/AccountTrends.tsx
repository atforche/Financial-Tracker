import type {
  AccountsInAccountingPeriodRange,
  AccountsInDateRange,
} from "@/accounts/types";
import {
  getAccountingPeriodIdsInRange,
  matchesTrendRange,
  resolveTrendRange,
} from "@/framework/routes/trendRange";
import {
  getPageOffset,
  normalizePageValue,
  rowsPerPage,
} from "@/framework/view/pagination";
import {
  normalizeAccountTypes,
  shouldPersistAccountTypes,
} from "@/accounts/accountTypeFilterHelpers";
import {
  normalizeRequestedAccountNames,
  shouldPersistAccountNames,
} from "@/accounts/accountNameFilterHelpers";
import AccountTrendsFilter from "@/accounts/trends/AccountTrendsFilter";
import AccountTrendsListFrame from "@/accounts/trends/AccountTrendsListFrame";
import type { AccountTrendsSearchParams } from "@/accounts/trends/helpers";
import BalanceStatCards from "@/accounts/BalanceStatCards";
import BalanceTrendCharts from "@/framework/charts/BalanceTrendCharts";
import type { JSX } from "react";
import PageLayout from "@/framework/view/PageLayout";
import createApiClient from "@/framework/data/createApiClient";
import getTrendSnapshot from "@/framework/charts/getTrendSnapshot";
import { periodTab } from "@/balance-events/resolveEntityHistory";
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
  const { returnUrl, sort, page, accountType, accountName } =
    resolvedSearchParams;

  const apiClient = await createApiClient();
  const accountingPeriodsPromise = apiClient.GET("/accounting-periods", {
    params: {
      query: {
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
    Offset: getPageOffset(currentPage),
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
  const currentRange =
    range.mode === "date"
      ? { startDate: range.startDate, endDate: range.endDate }
      : {
          startAccountingPeriodId: range.startAccountingPeriodId,
          endAccountingPeriodId: range.endAccountingPeriodId,
        };
  const trendsUrl = routes.trends({
    returnUrl,
    mode: currentMode,
    ...persistedFilters,
    ...currentRange,
  });
  const transactionWorkspaceHref = transactionRoutes.workspace({
    ...(range.mode === "date"
      ? currentRange
      : {
          accountingPeriodIds: getAccountingPeriodIdsInRange(
            accountingPeriods.items,
            range.startAccountingPeriodId,
            range.endAccountingPeriodId,
          ),
        }),
    ...(shouldPersistAccountTypes(currentAccountTypes)
      ? { accountTypes: currentAccountTypes }
      : {}),
    ...(shouldPersistAccountNames(currentAccountNames)
      ? { accountNames: currentAccountNames }
      : {}),
    returnUrl: trendsUrl,
  });

  const snapshot = getTrendSnapshot(
    "accountingPeriods" in trends ? trends.accountingPeriods : [],
    "dates" in trends ? trends.dates : [],
    "dates" in trends ? (trends.openingBalance ?? null) : null,
    range.mode === "date" ? range.startDate : null,
    {
      totalBalance: 0,
      totalTrackedBalance: 0,
      totalUntrackedBalance: 0,
      balanceByAccountType: [],
    },
  );

  return (
    <PageLayout>
      <AccountTrendsFilter
        accountingPeriods={accountingPeriods.items}
        availableAccountNames={trends.availableAccountNames}
        returnUrl={returnUrl}
      />
      <BalanceStatCards
        openingBalance={snapshot.startingBalance}
        closingBalance={snapshot.endingBalance}
        totalLabel="Total balance"
        changeDescription={`since ${snapshot.startLabel}`}
      />
      <BalanceTrendCharts trends={trends} />
      <AccountTrendsListFrame
        detailParams={{
          ...currentRange,
          ...(range.mode === "date" ? {} : { tab: periodTab }),
          returnUrl: trendsUrl,
        }}
        data={trends.accounts.items}
        isInOnboardingMode={isInOnboardingMode}
        totalCount={trends.accounts.totalCount}
        transactionWorkspaceHref={transactionWorkspaceHref}
      />
    </PageLayout>
  );
};

export default AccountTrends;
