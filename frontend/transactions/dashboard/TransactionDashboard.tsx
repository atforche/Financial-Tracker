import { Box, Stack } from "@mui/material";
import {
  TransactionDashboardMode,
  type TransactionDashboard as TransactionDashboardModel,
  type TransactionSortOrder,
  type TransactionType,
} from "@/transactions/types";
import {
  normalizeRequestedAccountNames,
  shouldPersistAccountNames,
} from "@/accounts/dashboard/accountNameFilter";
import {
  normalizeRequestedFundNames,
  shouldPersistFundNames,
} from "@/funds/dashboard/fundNameFilter";
import {
  normalizeTransactionTypes,
  shouldPersistTransactionTypes,
} from "@/transactions/dashboard/transactionTypeFilter";
import { AccountingPeriodSortOrder } from "@/accounting-periods/types";
import type { JSX } from "react";
import TransactionDashboardAmountChart from "@/transactions/dashboard/TransactionDashboardAmountChart";
import TransactionDashboardByTypeCard from "@/transactions/dashboard/TransactionDashboardByTypeCard";
import TransactionDashboardCountChart from "@/transactions/dashboard/TransactionDashboardCountChart";
import TransactionDashboardFilter from "@/transactions/dashboard/TransactionDashboardFilter";
import TransactionDashboardListFrame from "@/transactions/dashboard/TransactionDashboardListFrame";
import dayjs from "dayjs";
import getApiClient from "@/framework/data/getApiClient";
import { redirect } from "next/navigation";
import routes from "@/transactions/routes";
import { rowsPerPage } from "@/framework/listframe/Constants";

/**
 * URL mode values used to filter the Transactions dashboard.
 */
type TransactionsDashboardFilterMode = "accounting-period" | "date";

/**
 * Search parameters for the transaction dashboard.
 */
interface TransactionDashboardSearchParams {
  sort?: TransactionSortOrder;
  page?: number | null;
  mode?: TransactionsDashboardFilterMode;
  transactionType?: TransactionType | readonly TransactionType[];
  accountName?: string | readonly string[];
  fundName?: string | readonly string[];
  startAccountingPeriodId?: string;
  endAccountingPeriodId?: string;
  startDate?: string;
  endDate?: string;
}

/**
 * Props for the TransactionDashboard component.
 */
interface TransactionDashboardProps {
  readonly searchParams: Promise<TransactionDashboardSearchParams>;
}

const createEmptyDashboard = function (): TransactionDashboardModel {
  return {
    mode: TransactionDashboardMode.Date,
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
 * Component that displays the Transactions dashboard.
 */
const TransactionDashboard = async function ({
  searchParams,
}: TransactionDashboardProps): Promise<JSX.Element> {
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
  const currentMode: TransactionsDashboardFilterMode =
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
        mode: "accounting-period",
        ...persistedFilters,
        startAccountingPeriodId: latestAccountingPeriod.id,
        endAccountingPeriodId: latestAccountingPeriod.id,
      }),
    );
  }

  const transactionDashboardPromise = apiClient.GET("/transactions/dashboard", {
    params: {
      query: {
        ...(typeof sort === "string" ? { Sort: sort } : {}),
        Limit: rowsPerPage,
        ...(typeof page === "number" && page > 0
          ? { Offset: (page - 1) * rowsPerPage }
          : {}),
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
  const { data: dashboard } = await transactionDashboardPromise;
  const resolvedDashboard = dashboard ?? createEmptyDashboard();

  return (
    <Stack spacing={3} sx={{ width: "100%" }}>
      <Stack spacing={3} sx={{ maxWidth: 1440, width: "100%" }}>
        <TransactionDashboardFilter
          accountingPeriods={accountingPeriods?.items ?? []}
          availableAccountNames={resolvedDashboard.availableAccountNames}
          availableFundNames={resolvedDashboard.availableFundNames}
          defaultAccountingPeriodId={latestAccountingPeriod?.id ?? null}
          defaultStartDate={defaultStartDate.format("YYYY-MM-DD")}
          defaultEndDate={defaultEndDate.format("YYYY-MM-DD")}
        />
      </Stack>
      <TransactionDashboardByTypeCard dashboard={resolvedDashboard} />
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
        <TransactionDashboardCountChart
          mode={resolvedDashboard.mode}
          accountingPeriods={resolvedDashboard.accountingPeriods ?? null}
          dates={resolvedDashboard.dates ?? null}
        />
        <TransactionDashboardAmountChart
          mode={resolvedDashboard.mode}
          accountingPeriods={resolvedDashboard.accountingPeriods ?? null}
          dates={resolvedDashboard.dates ?? null}
        />
      </Box>
      <TransactionDashboardListFrame
        data={[...resolvedDashboard.transactions.items]}
        totalCount={resolvedDashboard.transactions.totalCount}
      />
    </Stack>
  );
};

export type { TransactionDashboardSearchParams };
export default TransactionDashboard;
