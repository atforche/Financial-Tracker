import { Stack, Typography } from "@mui/material";
import { getPageOffset, normalizePageValue } from "@/framework/listframe/page";
import AccountingPeriodCurrentIncomeSpendingCard from "@/accounting-periods/current/CurrentAccountingPeriodIncomeSpendingCard";
import AccountingPeriodCurrentSummaryCards from "@/accounting-periods/current/CurrentAccountingPeriodSummaryCards";
import AccountingPeriodCurrentTransactionListFrame from "@/accounting-periods/current/CurrentAccountingPeriodTransactionListFrame";
import type { CurrentAccountingPeriod as CurrentAccountingPeriodModel } from "@/accounting-periods/types";
import type { JSX } from "react";
import type { TransactionSortOrder } from "@/transactions/transaction";
import getApiClient from "@/framework/data/getApiClient";
import { rowsPerPage } from "@/framework/listframe/Constants";

/**
 * Search parameters for the CurrentAccountingPeriod component.
 */
interface CurrentAccountingPeriodSearchParams {
  transactionSort?: TransactionSortOrder;
  transactionPage?: number | string | null;
}

/**
 * Props for the CurrentAccountingPeriod component.
 */
interface CurrentAccountingPeriodProps {
  readonly searchParams: Promise<CurrentAccountingPeriodSearchParams>;
}

const createEmptyCurrent = function (): CurrentAccountingPeriodModel {
  return {
    accountingPeriod: null,
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
 * Component that displays the current Accounting Period snapshot.
 */
const CurrentAccountingPeriod = async function ({
  searchParams,
}: CurrentAccountingPeriodProps): Promise<JSX.Element> {
  const { transactionSort, transactionPage } = await searchParams;
  const currentTransactionPage = normalizePageValue(transactionPage);

  const apiClient = getApiClient();
  const current: CurrentAccountingPeriodModel =
    (
      await apiClient.GET("/accounting-periods/current", {
        params: {
          query: {
            ...(typeof transactionSort === "string"
              ? { TransactionSort: transactionSort }
              : {}),
            TransactionLimit: rowsPerPage,
            TransactionOffset: getPageOffset(currentTransactionPage),
          },
        },
      })
    ).data ?? createEmptyCurrent();

  const accountingPeriod = current.accountingPeriod ?? null;

  return (
    <Stack spacing={3} sx={{ width: "100%" }}>
      <Stack spacing={1} sx={{ maxWidth: 1440, width: "100%" }}>
        <Typography variant="overline" color="text.secondary">
          Accounting Periods
        </Typography>
        <Typography variant="h5">Current Accounting Period</Typography>
        <Typography color="text.secondary">
          {accountingPeriod === null
            ? "No accounting periods are available yet."
            : `Snapshot for ${accountingPeriod.name}.`}
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
