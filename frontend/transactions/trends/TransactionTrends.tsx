import type {
  Transaction,
  TransactionSort,
  TransactionType,
} from "@/transactions/types";
import { getPageOffset, normalizePageValue } from "@/framework/listframe/page";
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
import { rowsPerPage } from "@/framework/listframe/Constants";
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

  const apiClient = createApiClient();
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
    ...(typeof sort === "string" ? { Sort: sort } : {}),
  };
  const endpoint =
    currentMode === "date"
      ? "/transactions/date-range"
      : "/transactions/accounting-period-range";
  const listRequest =
    endpoint === "/transactions/date-range"
      ? apiClient.GET(endpoint, {
          params: {
            query: {
              ...query,
              Limit: rowsPerPage,
              Offset: getPageOffset(currentPage),
            },
          },
        })
      : apiClient.GET(endpoint, {
          params: {
            query: {
              ...query,
              Limit: rowsPerPage,
              Offset: getPageOffset(currentPage),
            },
          },
        });
  const summaryRequest =
    endpoint === "/transactions/date-range"
      ? apiClient.GET(endpoint, { params: { query } })
      : apiClient.GET(endpoint, { params: { query } });
  const [listResponse, summaryResponse] = await Promise.all([
    listRequest,
    summaryRequest,
  ]);
  const listData = unwrapApiResponse(
    listResponse,
    "Failed to load transactions",
  );
  const summaryData = unwrapApiResponse(
    summaryResponse,
    "Failed to load transaction summary",
  );
  const allTransactions = summaryData.transactions.items;
  const filteredTransactions = allTransactions.filter(
    (transaction) =>
      !shouldPersistTransactionTypes(currentTransactionTypes) ||
      currentTransactionTypes.includes(transaction.transactionType),
  );
  const transactions = shouldPersistTransactionTypes(currentTransactionTypes)
    ? {
        items: filteredTransactions.slice(
          getPageOffset(currentPage),
          getPageOffset(currentPage) + rowsPerPage,
        ),
        totalCount: filteredTransactions.length,
      }
    : listData.transactions;
  const groupTransactions = function (
    getKey: (transaction: Transaction) => string,
  ): Map<string, Transaction[]> {
    return Map.groupBy(filteredTransactions, getKey);
  };
  const dateSummaries = Array.from(
    groupTransactions((transaction) => transaction.date),
    ([date, groupedTransactions]) => ({
      date,
      totalAmount: groupedTransactions.reduce(
        (total, transaction) => total + transaction.amount,
        0,
      ),
      totalCount: groupedTransactions.length,
    }),
  );
  const accountingPeriodSummaries = Array.from(
    groupTransactions((transaction) => transaction.accountingPeriodId),
    ([accountingPeriodId, groupedTransactions]) => ({
      accountingPeriodId,
      accountingPeriodName: groupedTransactions[0]?.accountingPeriodName ?? "",
      totalAmount: groupedTransactions.reduce(
        (total, transaction) => total + transaction.amount,
        0,
      ),
      totalCount: groupedTransactions.length,
    }),
  );

  return (
    <PageLayout>
      <ConstrainedContent>
        <TransactionTrendsFilter
          accountingPeriods={accountingPeriods.items}
          availableAccountNames={summaryData.availableAccountNames}
          availableFundNames={summaryData.availableFundNames}
          defaultAccountingPeriodId={latestAccountingPeriod?.id ?? null}
          defaultStartDate={defaultStartDate.format("YYYY-MM-DD")}
          defaultEndDate={defaultEndDate.format("YYYY-MM-DD")}
        />
      </ConstrainedContent>
      <TransactionsByTypeCard transactionTypes={summaryData.transactionTypes} />
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
        data={[...transactions.items]}
        totalCount={transactions.totalCount}
      />
    </PageLayout>
  );
};

export type { TransactionTrendsSearchParams };
export default TransactionTrends;
