import { Box, Stack } from "@mui/material";
import {
  type TransactionSortOrder,
  TransactionTrendsMode,
  type TransactionTrends as TransactionTrendsModel,
  type TransactionType,
} from "@/transactions/types";
import { getPageOffset, normalizePageValue } from "@/framework/listframe/page";
import {
  normalizeRequestedAccountNames,
  shouldPersistAccountNames,
} from "@/accounts/trends/accountNameFilter";
import {
  normalizeRequestedFundNames,
  shouldPersistFundNames,
} from "@/funds/trends/fundNameFilter";
import {
  normalizeTransactionTypes,
  shouldPersistTransactionTypes,
} from "@/transactions/trends/transactionTypeFilter";
import { AccountingPeriodSortOrder } from "@/accounting-periods/types";
import type { JSX } from "react";
import TransactionTrendsAmountChart from "@/transactions/trends/TransactionTrendsAmountChart";
import TransactionTrendsByTypeCard from "@/transactions/trends/TransactionTrendsByTypeCard";
import TransactionTrendsCountChart from "@/transactions/trends/TransactionTrendsCountChart";
import TransactionTrendsFilter from "@/transactions/trends/TransactionTrendsFilter";
import TransactionTrendsListFrame from "@/transactions/trends/TransactionTrendsListFrame";
import dayjs from "dayjs";
import getApiClient from "@/framework/data/getApiClient";
import { redirect } from "next/navigation";
import routes from "@/transactions/routes";
import { rowsPerPage } from "@/framework/listframe/Constants";

/**
 * URL mode values used to filter the Transactions trends.
 */
type TransactionsTrendsFilterMode = "accounting-period" | "date";

/**
 * Search parameters for the transaction trends.
 */
interface TransactionTrendsSearchParams {
  sort?: TransactionSortOrder;
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

const createEmptyTrends = function (): TransactionTrendsModel {
  return {
    mode: TransactionTrendsMode.Date,
    transactions: {
      items: [],
      totalCount: 0,
    },
    availableAccountNames: [],
    availableFundNames: [],
    transactionTypes: [],
    accountingPeriods: null,
    dates: null,
  };
};

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
  const isInOnboardingMode = latestAccountingPeriod === null;
  const currentMode: TransactionsTrendsFilterMode =
    typeof mode === "undefined" || isInOnboardingMode ? "date" : mode;
  const currentTransactionTypes = normalizeTransactionTypes(
    Array.isArray(transactionType)
      ? transactionType
      : typeof transactionType === "string"
        ? [transactionType]
        : [],
  );
  const currentAccountNames = normalizeRequestedAccountNames(
    Array.isArray(accountName)
      ? accountName
      : typeof accountName === "string"
        ? [accountName]
        : [],
  );
  const currentFundNames = normalizeRequestedFundNames(
    Array.isArray(fundName)
      ? fundName
      : typeof fundName === "string"
        ? [fundName]
        : [],
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

  const transactionTrendsPromise = apiClient.GET("/transactions/trends", {
    params: {
      query: {
        ...(typeof sort === "string" ? { Sort: sort } : {}),
        Limit: rowsPerPage,
        Offset: getPageOffset(currentPage),
        ...(shouldPersistTransactionTypes(currentTransactionTypes)
          ? { TransactionType: [...currentTransactionTypes] }
          : {}),
        ...(shouldPersistAccountNames(currentAccountNames)
          ? { AccountName: [...currentAccountNames] }
          : {}),
        ...(shouldPersistFundNames(currentFundNames)
          ? { FundName: [...currentFundNames] }
          : {}),
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
  const { data: trends } = await transactionTrendsPromise;
  const resolvedTrends = trends ?? createEmptyTrends();

  return (
    <Stack spacing={3} sx={{ width: "100%" }}>
      <Stack spacing={3} sx={{ maxWidth: 1440, width: "100%" }}>
        <TransactionTrendsFilter
          accountingPeriods={accountingPeriods?.items ?? []}
          availableAccountNames={resolvedTrends.availableAccountNames}
          availableFundNames={resolvedTrends.availableFundNames}
          defaultAccountingPeriodId={latestAccountingPeriod?.id ?? null}
          defaultStartDate={defaultStartDate.format("YYYY-MM-DD")}
          defaultEndDate={defaultEndDate.format("YYYY-MM-DD")}
        />
      </Stack>
      <TransactionTrendsByTypeCard trends={resolvedTrends} />
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
        <TransactionTrendsCountChart
          mode={resolvedTrends.mode}
          accountingPeriods={resolvedTrends.accountingPeriods ?? null}
          dates={resolvedTrends.dates ?? null}
        />
        <TransactionTrendsAmountChart
          mode={resolvedTrends.mode}
          accountingPeriods={resolvedTrends.accountingPeriods ?? null}
          dates={resolvedTrends.dates ?? null}
        />
      </Box>
      <TransactionTrendsListFrame
        data={[...resolvedTrends.transactions.items]}
        totalCount={resolvedTrends.transactions.totalCount}
      />
    </Stack>
  );
};

export type { TransactionTrendsSearchParams };
export default TransactionTrends;
