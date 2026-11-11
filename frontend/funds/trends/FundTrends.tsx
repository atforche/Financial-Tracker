import type {
  FundsInAccountingPeriodRange,
  FundsInDateRange,
} from "@/funds/types";
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
  normalizeRequestedFundNames,
  shouldPersistFundNames,
} from "@/funds/trends/fundNameFilter";
import BalanceTrendCharts from "@/framework/charts/BalanceTrendCharts";
import FundBalanceStatCards from "@/funds/FundBalanceStatCards";
import FundTrendsFilter from "@/funds/trends/FundTrendsFilter";
import FundTrendsListFrame from "@/funds/trends/FundTrendsListFrame";
import type { FundTrendsSearchParams } from "@/funds/trends/helpers";
import type { JSX } from "react";
import PageLayout from "@/framework/view/PageLayout";
import createApiClient from "@/framework/data/createApiClient";
import getTrendSnapshot from "@/framework/charts/getTrendSnapshot";
import { periodTab } from "@/balance-events/resolveEntityHistory";
import { redirect } from "next/navigation";
import routes from "@/funds/routes";
import { toRepeatedSearchParams } from "@/framework/routes/helpers";
import transactionRoutes from "@/transactions/routes";
import unwrapApiResponse from "@/framework/data/unwrapApiResponse";

/**
 * Props for the FundTrends component.
 */
interface FundTrendsProps {
  readonly searchParams: Promise<FundTrendsSearchParams>;
}

/**
 * Component that displays the Funds view.
 */
const FundTrends = async function ({
  searchParams,
}: FundTrendsProps): Promise<JSX.Element> {
  const resolvedSearchParams = await searchParams;
  const { returnUrl, sort, page, fundName } = resolvedSearchParams;

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
  const currentFundNames = normalizeRequestedFundNames(
    toRepeatedSearchParams(fundName),
  );
  const currentPage = normalizePageValue(page);

  const persistedFilters = {
    ...(typeof sort === "string" ? { sort } : {}),
    ...(shouldPersistFundNames(currentFundNames)
      ? { fundName: currentFundNames }
      : {}),
  };

  if (!matchesTrendRange(resolvedSearchParams, range)) {
    redirect(routes.trends({ returnUrl, ...persistedFilters, ...range }));
  }

  const filterQuery = {
    ...(shouldPersistFundNames(currentFundNames)
      ? { "Filter.Names": [...currentFundNames] }
      : {}),
  };
  const fundQuery = {
    ...(typeof sort === "string" ? { Sort: sort } : {}),
    ...filterQuery,
    Limit: rowsPerPage,
    Offset: getPageOffset(currentPage),
  };
  const trends = await (async function (): Promise<
    FundsInDateRange | FundsInAccountingPeriodRange
  > {
    if (range.mode === "date") {
      const rangeQuery = {
        "Range.Start": range.startDate,
        "Range.End": range.endDate,
      };
      return unwrapApiResponse(
        await apiClient.GET("/funds/date-range", {
          params: { query: { ...fundQuery, ...rangeQuery } },
        }),
        "Failed to load fund trends",
      );
    }
    const rangeQuery = {
      "Range.Start": range.startAccountingPeriodId,
      "Range.End": range.endAccountingPeriodId,
    };
    return unwrapApiResponse(
      await apiClient.GET("/funds/accounting-period-range", {
        params: { query: { ...fundQuery, ...rangeQuery } },
      }),
      "Failed to load fund trends",
    );
  })();
  const selectedFundIds = trends.funds.items
    .filter((fund) =>
      currentFundNames.some(
        (selectedFundName) =>
          selectedFundName.toLocaleLowerCase() ===
          fund.name.toLocaleLowerCase(),
      ),
    )
    .map((fund) => fund.id);
  const hasResolvedSelectedFundIds =
    selectedFundIds.length === currentFundNames.length;
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
    ...(hasResolvedSelectedFundIds && selectedFundIds.length > 0
      ? { fundIds: selectedFundIds }
      : {}),
    ...(!hasResolvedSelectedFundIds && shouldPersistFundNames(currentFundNames)
      ? { fundNames: currentFundNames }
      : {}),
    returnUrl: trendsUrl,
  });

  const snapshot = getTrendSnapshot(
    "accountingPeriods" in trends ? trends.accountingPeriods : [],
    "dates" in trends ? trends.dates : [],
    "dates" in trends ? (trends.openingBalance ?? null) : null,
    range.mode === "date" ? range.startDate : null,
    { totalBalance: 0, totalAssignedBalance: 0, totalUnassignedBalance: 0 },
  );

  return (
    <PageLayout>
      <FundTrendsFilter
        accountingPeriods={accountingPeriods.items}
        availableFundNames={trends.availableFundNames}
        returnUrl={returnUrl}
      />
      <FundBalanceStatCards
        startingBalance={snapshot.startingBalance}
        endingBalance={snapshot.endingBalance}
        changeDescription={`since ${snapshot.startLabel}`}
      />
      <BalanceTrendCharts trends={trends} />
      <FundTrendsListFrame
        detailParams={{
          ...currentRange,
          ...(range.mode === "date" ? {} : { tab: periodTab }),
          returnUrl: trendsUrl,
        }}
        data={trends.funds.items}
        isInOnboardingMode={isInOnboardingMode}
        totalCount={trends.funds.totalCount}
        transactionWorkspaceHref={transactionWorkspaceHref}
      />
    </PageLayout>
  );
};

export default FundTrends;
