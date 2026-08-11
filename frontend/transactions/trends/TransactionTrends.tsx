import type { TransactionSort, TransactionType } from "@/transactions/types";
import {
  getPageOffset,
  getRowsPerPage,
  normalizePageValue,
} from "@/framework/listframe/page";
import {
  normalizeRequestedAccountNames,
  shouldPersistAccountNames,
} from "@/accounts/accountNameFilterHelpers";
import {
  normalizeRequestedFundNames,
  shouldPersistFundNames,
} from "@/funds/trends/fundNameFilter";
import {
  normalizeTransactionTypes,
  shouldPersistTransactionTypes,
} from "@/transactions/trends/transactionTypeFilter";
import { AccountingPeriodSort } from "@/accounting-periods/types";
import ConstrainedContent from "@/framework/view/ConstrainedContent";
import type { JSX } from "react";
import PageLayout from "@/framework/view/PageLayout";
import ResponsiveGrid from "@/framework/view/ResponsiveGrid";
import TransactionTrendsAmountChart from "@/transactions/trends/TransactionTrendsAmountChart";
import TransactionTrendsCountChart from "@/transactions/trends/TransactionTrendsCountChart";
import TransactionTrendsFilter from "@/transactions/trends/TransactionTrendsFilter";
import TransactionTrendsListFrame from "@/transactions/trends/TransactionTrendsListFrame";
import TransactionsByTypeCard from "@/transactions/TransactionsByTypeCard";
import createApiClient from "@/framework/data/createApiClient";
import dayjs from "dayjs";
import { redirect } from "next/navigation";
import routes from "@/transactions/routes";
import { toRepeatedSearchParams } from "@/framework/routes/helpers";
import unwrapApiResponse from "@/framework/data/unwrapApiResponse";

/**
 * URL mode values used to filter the Transactions trends.
 */
type TransactionsTrendsFilterMode = "accounting-period" | "date";

/**
 * Search parameters for the transaction trends.
 */
interface TransactionTrendsSearchParams {
  sort?: TransactionSort;
  page?: number | string | null;
  pageSize?: number | string | null;
  mode?: TransactionsTrendsFilterMode;
  transactionType?: TransactionType | readonly TransactionType[];
  accountName?: string | readonly string[];
  fundName?: string | readonly string[];
  startAccountingPeriodId?: string;
  endAccountingPeriodId?: string;
  startDate?: string;
  endDate?: string;
}

/**
 * Props for the TransactionTrends component.
 */
interface TransactionTrendsProps {
  readonly searchParams: Promise<TransactionTrendsSearchParams>;
}

/**
 * Component that displays the Transactions trends.
 */
const TransactionTrends = async function ({
  searchParams,
}: TransactionTrendsProps): Promise<JSX.Element> {
  const {
    sort,
    page,
    pageSize,
    mode,
    transactionType,
    accountName,
    fundName,
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
  const currentMode: TransactionsTrendsFilterMode =
    typeof mode === "undefined" || isInOnboardingMode ? "date" : mode;
  const currentTransactionTypes = normalizeTransactionTypes(
    toRepeatedSearchParams(transactionType),
  );
  const currentAccountNames = normalizeRequestedAccountNames(
    toRepeatedSearchParams(accountName),
  );
  const currentFundNames = normalizeRequestedFundNames(
    toRepeatedSearchParams(fundName),
  );
  const currentPage = normalizePageValue(page);
  const rowsPerPage = getRowsPerPage(pageSize);

  const persistedFilters = {
    ...(typeof sort === "string" ? { sort } : {}),
    ...(shouldPersistTransactionTypes(currentTransactionTypes)
      ? { transactionType: currentTransactionTypes }
      : {}),
    ...(shouldPersistAccountNames(currentAccountNames)
      ? { accountName: currentAccountNames }
      : {}),
    ...(shouldPersistFundNames(currentFundNames)
      ? { fundName: currentFundNames }
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

  const [accountsResponse, fundsResponse] = await Promise.all([
    apiClient.GET("/accounts"),
    apiClient.GET("/funds"),
  ]);
  const accounts = unwrapApiResponse(
    accountsResponse,
    "Failed to fetch accounts",
  );
  const funds = unwrapApiResponse(fundsResponse, "Failed to fetch funds");
  const accountIds = accounts.items
    .filter((account) => currentAccountNames.includes(account.name))
    .map((account) => account.id);
  const fundIds = funds.items
    .filter((fund) => currentFundNames.includes(fund.name))
    .map((fund) => fund.id);
  const range =
    currentMode === "date"
      ? {
          start:
            typeof startDate === "string"
              ? startDate
              : defaultStartDate.format("YYYY-MM-DD"),
          end:
            typeof endDate === "string"
              ? endDate
              : defaultEndDate.format("YYYY-MM-DD"),
        }
      : {
          start: startAccountingPeriodId ?? latestAccountingPeriod?.id ?? "",
          end: endAccountingPeriodId ?? latestAccountingPeriod?.id ?? "",
        };
  const query = {
    "Range.Start": range.start,
    "Range.End": range.end,
    ...(shouldPersistAccountNames(currentAccountNames)
      ? { "Filter.AccountIds": accountIds }
      : {}),
    ...(shouldPersistFundNames(currentFundNames)
      ? { "Filter.FundIds": fundIds }
      : {}),
    ...(shouldPersistTransactionTypes(currentTransactionTypes)
      ? { "Filter.Types": [...currentTransactionTypes] }
      : {}),
    ...(typeof sort === "string" ? { Sort: sort } : {}),
  };
  // eslint-disable-next-line @typescript-eslint/explicit-function-return-type
  const { listData, trendsData } = await (async function () {
    const pagedQuery = {
      ...query,
      Limit: rowsPerPage,
      Offset: getPageOffset(currentPage, rowsPerPage),
    };
    if (currentMode === "date") {
      const [listResponse, trendsResponse] = await Promise.all([
        apiClient.GET("/transactions/date-range", {
          params: { query: pagedQuery },
        }),
        apiClient.GET("/transactions/trends/date-range", {
          params: { query },
        }),
      ]);
      return {
        listData: unwrapApiResponse(
          listResponse,
          "Failed to load transactions",
        ),
        trendsData: unwrapApiResponse(
          trendsResponse,
          "Failed to load transaction trends",
        ),
      };
    }
    const [listResponse, trendsResponse] = await Promise.all([
      apiClient.GET("/transactions/accounting-period-range", {
        params: { query: pagedQuery },
      }),
      apiClient.GET("/transactions/trends/accounting-period-range", {
        params: { query },
      }),
    ]);
    return {
      listData: unwrapApiResponse(listResponse, "Failed to load transactions"),
      trendsData: unwrapApiResponse(
        trendsResponse,
        "Failed to load transaction trends",
      ),
    };
  })();
  const dateSummaries = trendsData.dates;
  const accountingPeriodSummaries = trendsData.accountingPeriods.map(
    (summary) => ({
      accountingPeriodId: summary.accountingPeriod.id,
      accountingPeriodName: summary.accountingPeriod.name,
      totalAmount: summary.totalAmount,
      totalCount: summary.totalCount,
    }),
  );

  return (
    <PageLayout>
      <ConstrainedContent>
        <TransactionTrendsFilter
          accountingPeriods={accountingPeriods.items}
          availableAccountNames={trendsData.availableAccountNames}
          availableFundNames={trendsData.availableFundNames}
          defaultAccountingPeriodId={latestAccountingPeriod?.id ?? null}
          defaultStartDate={defaultStartDate.format("YYYY-MM-DD")}
          defaultEndDate={defaultEndDate.format("YYYY-MM-DD")}
        />
      </ConstrainedContent>
      <TransactionsByTypeCard transactionTypes={trendsData.transactionTypes} />
      <ResponsiveGrid columns={{ xs: 1, lg: 2 }}>
        <TransactionTrendsCountChart
          mode={currentMode === "date" ? "Date" : "AccountingPeriod"}
          accountingPeriods={accountingPeriodSummaries}
          dates={dateSummaries}
        />
        <TransactionTrendsAmountChart
          mode={currentMode === "date" ? "Date" : "AccountingPeriod"}
          accountingPeriods={accountingPeriodSummaries}
          dates={dateSummaries}
        />
      </ResponsiveGrid>
      <TransactionTrendsListFrame
        data={[...listData.transactions.items]}
        totalCount={listData.transactions.totalCount}
      />
    </PageLayout>
  );
};

export type { TransactionTrendsSearchParams };
export default TransactionTrends;
