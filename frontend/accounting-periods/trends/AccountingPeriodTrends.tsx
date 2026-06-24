import {
  AccountingPeriodSortOrder,
  type AccountingPeriodTrends as AccountingPeriodTrendsModel,
} from "@/accounting-periods/types";
import { Box, Stack } from "@mui/material";
import { getPageOffset, normalizePageValue } from "@/framework/listframe/page";
import AccountingPeriodTrendChart from "@/accounting-periods/trends/AccountingPeriodTrendChart";
import AccountingPeriodTrendsChangeChart from "@/accounting-periods/trends/AccountingPeriodTrendsChangeChart";
import AccountingPeriodTrendsFilter from "@/accounting-periods/trends/AccountingPeriodTrendsFilter";
import AccountingPeriodTrendsIncomeSpendingCard from "@/accounting-periods/trends/AccountingPeriodTrendsIncomeSpendingCard";
import AccountingPeriodTrendsListFrame from "@/accounting-periods/trends/AccountingPeriodTrendsListFrame";
import AccountingPeriodTrendsSummaryCards from "@/accounting-periods/trends/AccountingPeriodTrendsSummaryCards";
import AccountingPeriodTrendsTransactionListFrame from "@/accounting-periods/trends/AccountingPeriodTrendsTransactionListFrame";
import type { JSX } from "react";
import type { TransactionSortOrder } from "@/transactions/transaction";
import getApiClient from "@/framework/data/getApiClient";
import { rowsPerPage } from "@/framework/listframe/Constants";

/**
 * Search parameters for the AccountingPeriodTrends component.
 */
interface AccountingPeriodTrendsSearchParams {
  sort?: AccountingPeriodSortOrder;
  page?: number | string | null;
  transactionSort?: TransactionSortOrder;
  transactionPage?: number | string | null;
  startAccountingPeriodId?: string;
  endAccountingPeriodId?: string;
}

/**
 * Props for the AccountingPeriodTrends component.
 */
interface AccountingPeriodTrendsProps {
  readonly searchParams: Promise<AccountingPeriodTrendsSearchParams>;
}

const createEmptyTrends = function (): AccountingPeriodTrendsModel {
  return {
    accountingPeriods: {
      items: [],
      totalCount: 0,
    },
    transactions: {
      items: [],
      totalCount: 0,
    },
    totalIncome: {
      total: 0,
      tracked: 0,
      untracked: 0,
    },
    totalSpending: 0,
  };
};

/**
 * Component that displays the Accounting Period trends.
 */
const AccountingPeriodTrends = async function ({
  searchParams,
}: AccountingPeriodTrendsProps): Promise<JSX.Element> {
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
  const currentPage = normalizePageValue(page);
  const currentTransactionPage = normalizePageValue(transactionPage);

  if (typeof accountingPeriods === "undefined") {
    throw new Error("Failed to fetch accounting periods");
  }

  const latestAccountingPeriod = accountingPeriods.items[0] ?? null;
  const trends: AccountingPeriodTrendsModel =
    latestAccountingPeriod === null
      ? createEmptyTrends()
      : ((
          await apiClient.GET("/accounting-periods/trends", {
            params: {
              query: {
                ...(typeof sort === "string" ? { Sort: sort } : {}),
                ...(typeof transactionSort === "string"
                  ? { TransactionSort: transactionSort }
                  : {}),
                Limit: rowsPerPage,
                TransactionLimit: rowsPerPage,
                Offset: getPageOffset(currentPage),
                TransactionOffset: getPageOffset(currentTransactionPage),
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
        ).data ?? createEmptyTrends());

  return (
    <Stack spacing={3} sx={{ width: "100%" }}>
      <Stack spacing={3} sx={{ maxWidth: 1440, width: "100%" }}>
        <AccountingPeriodTrendsFilter
          accountingPeriods={accountingPeriods.items}
          defaultAccountingPeriodId={latestAccountingPeriod?.id ?? null}
          disabled={latestAccountingPeriod === null}
        />
      </Stack>
      <AccountingPeriodTrendsSummaryCards trends={trends} />
      <AccountingPeriodTrendsIncomeSpendingCard trends={trends} />
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
        <AccountingPeriodTrendChart
          accountingPeriods={trends.accountingPeriods.items}
        />
        <AccountingPeriodTrendsChangeChart
          accountingPeriods={trends.accountingPeriods.items}
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
        <AccountingPeriodTrendsListFrame
          data={trends.accountingPeriods.items}
          totalCount={trends.accountingPeriods.totalCount}
        />
        <AccountingPeriodTrendsTransactionListFrame trends={trends} />
      </Box>
    </Stack>
  );
};

export type { AccountingPeriodTrendsSearchParams };
export default AccountingPeriodTrends;
