import {
  type AccountingPeriodDashboard,
  AccountingPeriodSortOrder,
} from "@/accounting-periods/types";
import { Paper, Stack, Typography } from "@mui/material";
import AccountingPeriodDashboardIncomeSpendingCard from "@/accounting-periods/dashboard/AccountingPeriodDashboardIncomeSpendingCard";
import AccountingPeriodDashboardSummaryCards from "@/accounting-periods/dashboard/AccountingPeriodDashboardSummaryCards";
import type { JSX } from "react";
import getApiClient from "@/framework/data/getApiClient";

const createEmptyDashboard = function (): AccountingPeriodDashboard {
  return {
    accountingPeriods: { items: [], totalCount: 0 },
    transactions: { items: [], totalCount: 0 },
    totalIncome: 0,
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

  const dashboard =
    latestAccountingPeriod === null
      ? createEmptyDashboard()
      : ((
          await apiClient.GET("/accounting-periods/dashboard", {
            params: {
              query: {
                Limit: 10,
                TransactionLimit: 10,
                StartAccountingPeriodId: latestAccountingPeriod.id,
                EndAccountingPeriodId: latestAccountingPeriod.id,
              },
            },
          })
        ).data ?? createEmptyDashboard());

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
          <AccountingPeriodDashboardSummaryCards dashboard={dashboard} />
          <AccountingPeriodDashboardIncomeSpendingCard dashboard={dashboard} />
        </Stack>
      </Stack>
    </Paper>
  );
};

export default AccountingPeriodOverview;
