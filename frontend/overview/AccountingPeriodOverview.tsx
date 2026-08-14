import type { AccountingPeriod } from "@/accounting-periods/types";
import AccountingPeriodTrendsSummaryCards from "@/accounting-periods/trends/AccountingPeriodTrendsSummaryCards";
import ExpectedGoalContributionsActualCard from "@/accounting-periods/workspace/ExpectedGoalContributionsActualCard";
import ExpectedIncomeActualCard from "@/accounting-periods/workspace/ExpectedIncomeActualCard";
import IncomeSpendingCard from "@/transactions/IncomeSpendingCard";
import type { JSX } from "react";
import ResponsiveGrid from "@/framework/view/ResponsiveGrid";
import { Stack } from "@mui/material";
import createApiClient from "@/framework/data/createApiClient";
import unwrapApiResponse from "@/framework/data/unwrapApiResponse";

/**
 * Props for the AccountingPeriodOverview component.
 */
interface AccountingPeriodOverviewProps {
  readonly currentAccountingPeriod: AccountingPeriod | null;
  readonly latestAccountingPeriod: AccountingPeriod | null;
}

/**
 * Overview component for accounting periods.
 */
const AccountingPeriodOverview = async function ({
  currentAccountingPeriod,
  latestAccountingPeriod,
}: AccountingPeriodOverviewProps): Promise<JSX.Element> {
  const accountingPeriod = currentAccountingPeriod ?? latestAccountingPeriod;
  const apiClient = await createApiClient();
  const rangeResponse =
    accountingPeriod === null
      ? null
      : await apiClient.GET("/accounting-periods/range", {
          params: {
            query: {
              "Range.Start": accountingPeriod.id,
              "Range.End": accountingPeriod.id,
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
  const latestPeriod = periods.at(0);
  return (
    <Stack spacing={2}>
      <AccountingPeriodTrendsSummaryCards
        accountingPeriods={periods}
        showPeriodLabels={false}
      />
      <ResponsiveGrid minimumColumnWidth={320} spacing={2}>
        <IncomeSpendingCard
          totalIncome={range?.totalIncome}
          totalSpending={range?.totalSpending}
        />
        <ExpectedIncomeActualCard
          expectedIncome={latestPeriod?.expectedIncome}
          actualIncome={latestPeriod?.actualIncome}
        />
        <ExpectedGoalContributionsActualCard
          expectedGoalContributions={latestPeriod?.expectedGoalContributions}
          actualGoalContributions={latestPeriod?.actualGoalContributions}
        />
      </ResponsiveGrid>
    </Stack>
  );
};

export default AccountingPeriodOverview;
