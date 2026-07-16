import { Paper, Stack, Typography } from "@mui/material";
import AccountingPeriodTrendsSummaryCards from "@/accounting-periods/trends/AccountingPeriodTrendsSummaryCards";
import IncomeSpendingCard from "@/transactions/IncomeSpendingCard";
import type { JSX } from "react";
import getApiClient from "@/framework/data/getApiClient";
import getApiData from "@/framework/data/apiResponse";

/**
 * Overview component for accounting periods.
 */
const AccountingPeriodOverview = async function (): Promise<JSX.Element> {
  const apiClient = getApiClient();
  const accountingPeriodsResponse = await apiClient.GET(
    "/accounting-periods",
    {
      params: { query: { Sort: "DateDescending", Limit: 1, Offset: 0 } },
    },
  );
  const accountingPeriods = getApiData(accountingPeriodsResponse, "Failed to load accounting periods");
  const latestAccountingPeriod = accountingPeriods.items[0] ?? null;
  const rangeResponse =
    latestAccountingPeriod === null
      ? null
      : await apiClient.GET("/accounting-periods/range", {
          params: {
            query: {
              "Range.Start": latestAccountingPeriod.id,
              "Range.End": latestAccountingPeriod.id,
              Limit: 1,
              Offset: 0,
            },
          },
        });
  const range = rangeResponse === null ? null : getApiData(rangeResponse, "Failed to load accounting period overview");
  const periods = range?.accountingPeriods.items ?? [];
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
          <AccountingPeriodTrendsSummaryCards accountingPeriods={periods} />
          <IncomeSpendingCard
            totalIncome={range?.totalIncome}
            totalSpending={range?.totalSpending}
          />
        </Stack>
      </Stack>
    </Paper>
  );
};

export default AccountingPeriodOverview;
