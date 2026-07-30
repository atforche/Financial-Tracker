"use client";

import { Button } from "@mui/material";
import type { JSX } from "react";
import ListFrame from "@/framework/listframe/ListFrame";
import type { Transaction } from "@/transactions/types";
import type { TransactionTrendsSearchParams } from "@/transactions/trends/TransactionTrends";
import createTransactionListColumns from "@/transactions/createTransactionListColumns";
import propertyName from "@/framework/data/propertyName";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";
import { useSearchParams } from "next/navigation";
import useTransactionList from "@/transactions/useTransactionList";

/**
 * Props for the TransactionTrendsListFrame component.
 */
interface TransactionTrendsListFrameProps {
  readonly data: Transaction[] | null;
  readonly totalCount: number | null;
}

/**
 * List frame that displays transactions for the trends page.
 */
const TransactionTrendsListFrame = function ({
  data,
  totalCount,
}: TransactionTrendsListFrameProps): JSX.Element {
  const searchParams = useSearchParams();

  const sortParamName = propertyName<TransactionTrendsSearchParams>("sort");
  const pageParamName = propertyName<TransactionTrendsSearchParams>("page");
  const modeParamName = propertyName<TransactionTrendsSearchParams>("mode");
  const transactionTypeParamName =
    propertyName<TransactionTrendsSearchParams>("transactionType");
  const accountNameParamName =
    propertyName<TransactionTrendsSearchParams>("accountName");
  const fundNameParamName =
    propertyName<TransactionTrendsSearchParams>("fundName");
  const startAccountingPeriodIdParamName =
    propertyName<TransactionTrendsSearchParams>("startAccountingPeriodId");
  const endAccountingPeriodIdParamName =
    propertyName<TransactionTrendsSearchParams>("endAccountingPeriodId");
  const startDateParamName =
    propertyName<TransactionTrendsSearchParams>("startDate");
  const endDateParamName =
    propertyName<TransactionTrendsSearchParams>("endDate");

  const updateParams = useSearchParamUpdater([pageParamName]);
  const { currentSort, openTransactionWorkspace, setSort } = useTransactionList(
    sortParamName,
    pageParamName,
  );

  const hasActiveFilters =
    searchParams.get(modeParamName) === "date" ||
    searchParams.getAll(transactionTypeParamName).length > 0 ||
    searchParams.getAll(accountNameParamName).length > 0 ||
    searchParams.getAll(fundNameParamName).length > 0 ||
    searchParams.has(startAccountingPeriodIdParamName) ||
    searchParams.has(endAccountingPeriodIdParamName) ||
    searchParams.has(startDateParamName) ||
    searchParams.has(endDateParamName);

  const columns = createTransactionListColumns({
    currentSort,
    includeAccountingPeriod: true,
    openTransaction: openTransactionWorkspace,
    setSort,
  });

  return (
    <ListFrame<Transaction>
      title="Matching Transactions"
      columns={columns}
      getId={(transaction) => transaction.id}
      data={data}
      totalCount={totalCount}
      pageParamName={pageParamName}
      onRowClick={openTransactionWorkspace}
      hasActiveFilters={hasActiveFilters}
      initialEmptyState={{
        title: "No matching transactions",
        description:
          "Try widening the date range or clearing some filters to see more results.",
        action: null,
      }}
      filteredEmptyState={{
        title: "No matching transactions",
        description:
          "Try widening the date range or clearing some filters to see more results.",
        action: hasActiveFilters ? (
          <Button
            variant="outlined"
            onClick={() => {
              updateParams((params) => {
                params.delete(transactionTypeParamName);
                params.delete(accountNameParamName);
                params.delete(fundNameParamName);
                params.delete(startAccountingPeriodIdParamName);
                params.delete(endAccountingPeriodIdParamName);
                params.delete(startDateParamName);
                params.delete(endDateParamName);
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

export default TransactionTrendsListFrame;
