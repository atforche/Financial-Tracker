import {
  AccountingPeriodSort,
  type AccountingPeriodWithTransactions,
} from "@/accounting-periods/types";
import { Stack, Typography } from "@mui/material";
import { getPageOffset, normalizePageValue } from "@/framework/listframe/page";
import AccountingPeriodCurrentSummaryCards from "@/accounting-periods/current/CurrentAccountingPeriodSummaryCards";
import AccountingPeriodCurrentTransactionListFrame from "@/accounting-periods/current/CurrentAccountingPeriodTransactionListFrame";
import ConstrainedContent from "@/framework/view/ConstrainedContent";
import IncomeSpendingCard from "@/transactions/IncomeSpendingCard";
import type { JSX } from "react";
import PageLayout from "@/framework/view/PageLayout";
import type { TransactionSort } from "@/transactions/types";
import getApiClient from "@/framework/data/getApiClient";
import getApiData from "@/framework/data/apiResponse";
import { isNotNullOrUndefined } from "@/framework/nullHelpers";
import { rowsPerPage } from "@/framework/listframe/Constants";

/**
 * Search parameters for the CurrentAccountingPeriod component.
 */
interface CurrentAccountingPeriodSearchParams {
  transactionSort?: TransactionSort;
  transactionPage?: number | string | null;
}

/**
 * Props for the CurrentAccountingPeriod component.
 */
interface CurrentAccountingPeriodProps {
  readonly searchParams: Promise<CurrentAccountingPeriodSearchParams>;
}

/**
 * Component that displays the current Accounting Period snapshot.
 */
const CurrentAccountingPeriod = async function ({
  searchParams,
}: CurrentAccountingPeriodProps): Promise<JSX.Element> {
  const { transactionSort, transactionPage } = await searchParams;
  const currentTransactionPage = normalizePageValue(transactionPage);

  const apiClient = getApiClient();
  const accountingPeriodsResponse = await apiClient.GET(
    "/accounting-periods",
    {
      params: {
        query: { Sort: AccountingPeriodSort.DateDescending, Limit: 1 },
      },
    },
  );
  const accountingPeriods = getApiData(accountingPeriodsResponse, "Failed to fetch accounting periods");
  const currentAccountingPeriod = accountingPeriods.items[0] ?? null;
  let current: AccountingPeriodWithTransactions | null = null;
  if (isNotNullOrUndefined(currentAccountingPeriod)) {
    current = getApiData(
      await apiClient.GET(
          "/accounting-periods/{accountingPeriodId}/transactions",
          {
            params: {
              path: { accountingPeriodId: currentAccountingPeriod.id },
              query: {
                ...(isNotNullOrUndefined(transactionSort)
                  ? { Sort: transactionSort }
                  : {}),
                Limit: rowsPerPage,
                Offset: getPageOffset(currentTransactionPage),
              },
            },
          },
        ),
      "Failed to fetch the current accounting period",
    );
  }

  return (
    <PageLayout>
      <ConstrainedContent>
        <Stack spacing={1}>
          <Typography variant="overline" color="text.secondary">
            Accounting Periods
          </Typography>
          <Typography variant="h5">Current Accounting Period</Typography>
          <Typography color="text.secondary">
            {current === null
              ? "No accounting periods are available yet."
              : `Snapshot for ${current.name}.`}
          </Typography>
        </Stack>
      </ConstrainedContent>
      <AccountingPeriodCurrentSummaryCards current={current} />
      <IncomeSpendingCard
        totalIncome={current?.totalIncome}
        totalSpending={current?.totalSpending}
      />
      <AccountingPeriodCurrentTransactionListFrame accountingPeriod={current} />
    </PageLayout>
  );
};

export type { CurrentAccountingPeriodSearchParams };
export default CurrentAccountingPeriod;
