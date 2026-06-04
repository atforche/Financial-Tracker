import {
  type AccountingPeriodDashboard as AccountingPeriodDashboardModel,
  AccountingPeriodSortOrder,
  type AccountingPeriodTransactionSortOrder,
} from "@/accounting-periods/types";
import { Box, Stack } from "@mui/material";
import AccountingPeriodDashboardChangeChart from "@/accounting-periods/dashboard/AccountingPeriodDashboardChangeChart";
import AccountingPeriodDashboardFilter from "@/accounting-periods/dashboard/AccountingPeriodDashboardFilter";
import AccountingPeriodDashboardIncomeSpendingCard from "@/accounting-periods/dashboard/AccountingPeriodDashboardIncomeSpendingCard";
import AccountingPeriodDashboardListFrame from "@/accounting-periods/dashboard/AccountingPeriodDashboardListFrame";
import AccountingPeriodDashboardSummaryCards from "@/accounting-periods/dashboard/AccountingPeriodDashboardSummaryCards";
import AccountingPeriodDashboardTransactionListFrame from "@/accounting-periods/dashboard/AccountingPeriodDashboardTransactionListFrame";
import AccountingPeriodDashboardTrendChart from "@/accounting-periods/dashboard/AccountingPeriodDashboardTrendChart";
import type { JSX } from "react";
import getApiClient from "@/framework/data/getApiClient";
import { rowsPerPage } from "@/framework/listframe/Constants";

/**
 * Search parameters for the AccountingPeriodDashboard component.
 */
interface AccountingPeriodDashboardSearchParams {
  sort?: AccountingPeriodSortOrder;
  page?: number | null;
  transactionSort?: AccountingPeriodTransactionSortOrder;
  transactionPage?: number | string;
  startAccountingPeriodId?: string;
  endAccountingPeriodId?: string;
}

/**
 * Props for the AccountingPeriodDashboard component.
 */
interface AccountingPeriodDashboardProps {
  readonly searchParams: Promise<AccountingPeriodDashboardSearchParams>;
}

const createEmptyDashboard = function (): AccountingPeriodDashboardModel {
  return {
    accountingPeriods: {
      items: [],
      totalCount: 0,
    },
    transactions: {
      items: [],
      totalCount: 0,
    },
    totalIncome: 0,
    totalSpending: 0,
  };
};

/**
 * Component that displays the Accounting Period dashboard.
 */
const AccountingPeriodDashboard = async function ({
  searchParams,
}: AccountingPeriodDashboardProps): Promise<JSX.Element> {
  const {
    sort,
    page,
    transactionSort,
    transactionPage,
    startAccountingPeriodId,
    endAccountingPeriodId,
  } = await searchParams;

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

  if (typeof accountingPeriods === "undefined") {
    throw new Error("Failed to fetch accounting periods");
  }

  const latestAccountingPeriod = accountingPeriods.items[0] ?? null;
  const dashboard: AccountingPeriodDashboardModel =
    latestAccountingPeriod === null
      ? createEmptyDashboard()
      : ((
          await apiClient.GET("/accounting-periods/dashboard", {
            params: {
              query: {
                ...(typeof sort === "string" ? { Sort: sort } : {}),
                ...(typeof transactionSort === "string"
                  ? { TransactionSort: transactionSort }
                  : {}),
                Limit: rowsPerPage,
                TransactionLimit: rowsPerPage,
                ...(typeof page === "number" && page > 0
                  ? { Offset: (page - 1) * rowsPerPage }
                  : {}),
                ...(typeof transactionPage === "number" && transactionPage > 0
                  ? { TransactionOffset: (transactionPage - 1) * rowsPerPage }
                  : {}),
                ...{
                  StartAccountingPeriodId:
                    typeof startAccountingPeriodId === "string"
                      ? startAccountingPeriodId
                      : latestAccountingPeriod.id,
                  EndAccountingPeriodId:
                    typeof endAccountingPeriodId === "string"
                      ? endAccountingPeriodId
                      : latestAccountingPeriod.id,
                },
              },
            },
          })
        ).data ?? createEmptyDashboard());

  return (
    <Stack spacing={3} sx={{ width: "100%" }}>
      <Stack spacing={3} sx={{ maxWidth: 1440, width: "100%" }}>
        <AccountingPeriodDashboardFilter
          accountingPeriods={accountingPeriods.items}
          defaultAccountingPeriodId={latestAccountingPeriod?.id ?? null}
          disabled={latestAccountingPeriod === null}
        />
      </Stack>
      <AccountingPeriodDashboardSummaryCards dashboard={dashboard} />
      <AccountingPeriodDashboardIncomeSpendingCard dashboard={dashboard} />
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
        <AccountingPeriodDashboardTrendChart
          accountingPeriods={dashboard.accountingPeriods.items}
        />
        <AccountingPeriodDashboardChangeChart
          accountingPeriods={dashboard.accountingPeriods.items}
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
        <AccountingPeriodDashboardListFrame
          data={dashboard.accountingPeriods.items}
          totalCount={dashboard.accountingPeriods.totalCount}
        />
        <AccountingPeriodDashboardTransactionListFrame dashboard={dashboard} />
      </Box>
    </Stack>
  );
};

export type { AccountingPeriodDashboardSearchParams };
export default AccountingPeriodDashboard;
