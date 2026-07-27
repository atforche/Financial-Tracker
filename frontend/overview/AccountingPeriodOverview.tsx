import type { AccountingPeriod } from "@/accounting-periods/types";
import AccountingPeriodTrendsSummaryCards from "@/accounting-periods/trends/AccountingPeriodTrendsSummaryCards";
import IncomeSpendingCard from "@/transactions/IncomeSpendingCard";
import type { JSX } from "react";
import { Stack } from "@mui/material";
import createApiClient from "@/framework/data/createApiClient";
import unwrapApiResponse from "@/framework/data/unwrapApiResponse";

/**
 * Props for the AccountingPeriodOverview component.
 */
interface AccountingPeriodOverviewProps {
  readonly latestAccountingPeriod: AccountingPeriod | null;
}

/**
 * Overview component for accounting periods.
 */
const AccountingPeriodOverview = async function ({
  latestAccountingPeriod,
}: AccountingPeriodOverviewProps): Promise<JSX.Element> {
  const apiClient = createApiClient();
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
  const range =
    rangeResponse === null
      ? null
      : unwrapApiResponse(
          rangeResponse,
          "Failed to load accounting period overview",
        );
  const periods = range?.accountingPeriods.items ?? [];
  return (
    <Stack spacing={2}>
      <AccountingPeriodTrendsSummaryCards accountingPeriods={periods} />
      <IncomeSpendingCard
        totalIncome={range?.totalIncome}
        totalSpending={range?.totalSpending}
      />
    </Stack>
  );
};

export default AccountingPeriodOverview;
