import { AccountingPeriodSort, type AccountingPeriodWithTransactions } from "@/accounting-periods/types";
import { Stack, Typography } from "@mui/material";
import { getPageOffset, normalizePageValue } from "@/framework/listframe/page";
import AccountingPeriodCurrentIncomeSpendingCard from "@/accounting-periods/current/CurrentAccountingPeriodIncomeSpendingCard";
import AccountingPeriodCurrentSummaryCards from "@/accounting-periods/current/CurrentAccountingPeriodSummaryCards";
import AccountingPeriodCurrentTransactionListFrame from "@/accounting-periods/current/CurrentAccountingPeriodTransactionListFrame";
import type { JSX } from "react";
import type { TransactionSort } from "@/transactions/types";
import getApiClient from "@/framework/data/getApiClient";
import isNotNullOrUndefined from "@/framework/isNotNullOrUndefined";
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
  const { data: accountingPeriods } = await apiClient.GET(
    "/accounting-periods",
    { params: { query: { Sort: AccountingPeriodSort.DateDescending, Limit: 1 } } },
  );
  const currentAccountingPeriod = accountingPeriods?.items[0] ?? null;
  let current: AccountingPeriodWithTransactions | null = null;
  if (isNotNullOrUndefined(currentAccountingPeriod)) {
    current =
      (
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
        )
      ).data ?? null;
  }

  return (
    <Stack spacing={3} sx={{ width: "100%" }}>
      <Stack spacing={1} sx={{ maxWidth: 1440, width: "100%" }}>
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
      <AccountingPeriodCurrentSummaryCards current={current} />
      <AccountingPeriodCurrentIncomeSpendingCard current={current} />
      <AccountingPeriodCurrentTransactionListFrame current={current} />
    </Stack>
  );
};

export type { CurrentAccountingPeriodSearchParams };
export default CurrentAccountingPeriod;
