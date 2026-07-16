"use client";

import { type Transaction, TransactionSort } from "@/transactions/types";
import {
  getTransactionAccountIds,
  getTransactionFundIds,
} from "@/transactions/postingHelpers";
import { useRouter, useSearchParams } from "next/navigation";
import type { AccountingPeriodTrendsSearchParams } from "@/accounting-periods/trends/AccountingPeriodTrends";
import { Button } from "@mui/material";
import type { JSX } from "react";
import ListFrame from "@/framework/listframe/ListFrame";
import createTransactionListColumns from "@/transactions/createTransactionListColumns";
import nameof from "@/framework/data/nameof";
import routes from "@/transactions/routes";
import tryParseEnum from "@/framework/data/tryParseEnum";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";

/**
 * Props for the AccountingPeriodTrendsTransactionListFrame component.
 */
interface AccountingPeriodTrendsTransactionListFrameProps {
  readonly transactions: Transaction[];
  readonly totalCount: number;
}

/**
 * List frame that displays transactions for the accounting period trends page.
 */
const AccountingPeriodTrendsTransactionListFrame = function ({
  transactions,
  totalCount,
}: AccountingPeriodTrendsTransactionListFrameProps): JSX.Element {
  const searchParams = useSearchParams();
  const router = useRouter();

  const sortParamName =
    nameof<AccountingPeriodTrendsSearchParams>("transactionSort");
  const pageParamName =
    nameof<AccountingPeriodTrendsSearchParams>("transactionPage");
  const startAccountingPeriodIdParamName =
    nameof<AccountingPeriodTrendsSearchParams>("startAccountingPeriodId");
  const endAccountingPeriodIdParamName =
    nameof<AccountingPeriodTrendsSearchParams>("endAccountingPeriodId");
  const updateParams = useSearchParamUpdater([pageParamName]);

  const setSort = function (sort: TransactionSort | null): void {
    updateParams((params) => {
      if (sort === null) {
        params.delete(sortParamName);
      } else {
        params.set(sortParamName, sort);
      }
    });
  };

  const currentSort = tryParseEnum(
    TransactionSort,
    searchParams.get(sortParamName) ?? "",
  );

  const hasActiveFilters =
    searchParams.has(startAccountingPeriodIdParamName) ||
    searchParams.has(endAccountingPeriodIdParamName);

  const openTransactionWorkspace = function (transaction: Transaction): void {
    router.push(
      routes.workspace({
        accountingPeriodIds: [transaction.accountingPeriodId],
        accountIds: getTransactionAccountIds(transaction),
        fundIds: getTransactionFundIds(transaction),
        selectedTransactionId: transaction.id,
      }),
    );
  };

  const columns = createTransactionListColumns({
    currentSort,
    setSort,
    openTransaction: openTransactionWorkspace,
    includeAccountingPeriod: true,
  });

  return (
    <ListFrame<Transaction>
      title="Transactions Across Selected Periods"
      columns={columns}
      getId={(transaction) => transaction.id}
      data={transactions}
      totalCount={totalCount}
      pageParamName={pageParamName}
      onRowClick={openTransactionWorkspace}
      hasActiveFilters={hasActiveFilters}
      initialEmptyState={{
        title: "No transactions found",
        description:
          "Try broadening the accounting period range to bring more transactions into view.",
        action: null,
      }}
      filteredEmptyState={{
        title: "No transactions found",
        description:
          "Try broadening the accounting period range to bring more transactions into view.",
        action: hasActiveFilters ? (
          <Button
            variant="outlined"
            onClick={() => {
              updateParams((params) => {
                params.delete(startAccountingPeriodIdParamName);
                params.delete(endAccountingPeriodIdParamName);
              });
            }}
          >
            Clear Filters
          </Button>
        ) : null,
      }}
    />
  );
};

export default AccountingPeriodTrendsTransactionListFrame;
