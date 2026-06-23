import {
  AccountingPeriodSortOrder,
  type AccountingPeriodTrends,
} from "@/accounting-periods/types";
import { Paper, Stack, Typography } from "@mui/material";
import AccountingPeriodTrendsIncomeSpendingCard from "@/accounting-periods/trends/AccountingPeriodTrendsIncomeSpendingCard";
import AccountingPeriodTrendsSummaryCards from "@/accounting-periods/trends/AccountingPeriodTrendsSummaryCards";
import type { JSX } from "react";
import getApiClient from "@/framework/data/getApiClient";

const createEmptyTrends = function (): AccountingPeriodTrends {
  return {
    accountingPeriods: { items: [], totalCount: 0 },
    transactions: { items: [], totalCount: 0 },
    totalIncome: {
      total: 0,
      tracked: 0,
      untracked: 0,
    },
    totalSpending: 0,
  };
};

/**
 * Overview component for accounting periods.
 */
const AccountingPeriodOverview = async function (): Promise<JSX.Element> {
  const apiClient = getApiClient();
  const accountingPeriodsResponse = await apiClient.GET("/accounting-periods", {
    params: {
      query: {
        Search: "",
        Sort: AccountingPeriodSortOrder.DateDescending,
        Limit: 1,
        Offset: 0,
      },
    },
  });

  const latestAccountingPeriod =
    accountingPeriodsResponse.data?.items[0] ?? null;

  const trends =
    latestAccountingPeriod === null
      ? createEmptyTrends()
      : ((
          await apiClient.GET("/accounting-periods/trends", {
            params: {
              query: {
                Limit: 10,
                TransactionLimit: 10,
                StartAccountingPeriodId: latestAccountingPeriod.id,
                EndAccountingPeriodId: latestAccountingPeriod.id,
              },
            },
          })
        ).data ?? createEmptyTrends());

  return (
    <Paper sx={{ border: "1px solid", borderColor: "divider", p: 3 }}>
      <Stack spacing={2}>
        <Typography variant="h6" color="text.secondary">
          Current Accounting Period:
          {latestAccountingPeriod !== null
            ? ` ${latestAccountingPeriod.name}`
            : " None available"}
        </Typography>
        <Stack spacing={2}>
          <AccountingPeriodTrendsSummaryCards trends={trends} />
          <AccountingPeriodTrendsIncomeSpendingCard trends={trends} />
        </Stack>
      </Stack>
    </Paper>
  );
};

export default AccountingPeriodOverview;
