"use client";

import { type Transaction, TransactionSort } from "@/transactions/types";
import {
  getTransactionAccountIds,
  getTransactionFundIds,
} from "@/transactions/postingHelpers";
import {
  getTransactionDestinationLabel,
  getTransactionSourceLabel,
} from "@/transactions/current/helpers";
import { useRouter, useSearchParams } from "next/navigation";
import ArrowForwardOutlined from "@mui/icons-material/ArrowForwardOutlined";
import { Button } from "@mui/material";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import type { JSX } from "react";
import ListFrame from "@/framework/listframe/ListFrame";
import ListFrameActionButton from "@/framework/listframe/ListFrameActionButton";
import type { TransactionTrendsSearchParams } from "@/transactions/trends/TransactionTrends";
import createColumnSortProps from "@/framework/listframe/createColumnSortProps";
import { formatCurrency } from "@/framework/currencyHelpers";
import parseEnumValue from "@/framework/data/parseEnumValue";
import propertyName from "@/framework/data/propertyName";
import routes from "@/transactions/routes";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";

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
  const router = useRouter();

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

  const setSort = function (sort: TransactionSort | null): void {
    updateParams((params) => {
      if (sort === null) {
        params.delete(sortParamName);
      } else {
        params.set(sortParamName, sort);
      }
    });
  };

  const currentSort = parseEnumValue(
    TransactionSort,
    searchParams.get(sortParamName) ?? "",
  );

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

  const hasActiveFilters =
    searchParams.get(modeParamName) === "date" ||
    searchParams.getAll(transactionTypeParamName).length > 0 ||
    searchParams.getAll(accountNameParamName).length > 0 ||
    searchParams.getAll(fundNameParamName).length > 0 ||
    searchParams.has(startAccountingPeriodIdParamName) ||
    searchParams.has(endAccountingPeriodIdParamName) ||
    searchParams.has(startDateParamName) ||
    searchParams.has(endDateParamName);

  const getSortProps = createColumnSortProps(currentSort, setSort);

  const columns: ColumnDefinition<Transaction>[] = [
    {
      name: "date",
      headerContent: "Date",
      getBodyContent: (transaction) => transaction.date,
      ...getSortProps(TransactionSort.Date, TransactionSort.DateDescending),
      minWidth: 125,
    },
    {
      name: "accountingPeriod",
      headerContent: "Accounting Period",
      getBodyContent: (transaction) => transaction.accountingPeriodName,
      ...getSortProps(
        TransactionSort.AccountingPeriod,
        TransactionSort.AccountingPeriodDescending,
      ),
      minWidth: 165,
    },
    {
      name: "description",
      headerContent: "Description",
      getBodyContent: (transaction) => transaction.description,
      ...getSortProps(
        TransactionSort.Description,
        TransactionSort.DescriptionDescending,
      ),
      minWidth: 150,
    },
    {
      name: "source",
      headerContent: "Source",
      getBodyContent: getTransactionSourceLabel,
      ...getSortProps(TransactionSort.Source, TransactionSort.SourceDescending),
      minWidth: 170,
    },
    {
      name: "destination",
      headerContent: "Destination",
      getBodyContent: getTransactionDestinationLabel,
      ...getSortProps(
        TransactionSort.Destination,
        TransactionSort.DestinationDescending,
      ),
      minWidth: 170,
    },
    {
      name: "amount",
      headerContent: "Amount",
      getBodyContent: (transaction) => formatCurrency(transaction.amount),
      ...getSortProps(TransactionSort.Amount, TransactionSort.AmountDescending),
      alignment: "right",
      minWidth: 150,
    },
    {
      name: "actions",
      headerContent: "",
      getBodyContent: (transaction) => (
        <ListFrameActionButton
          size="small"
          color="primary"
          onClick={(event) => {
            event.stopPropagation();
            openTransactionWorkspace(transaction);
          }}
          ariaLabel={`Open transaction ${transaction.id}`}
        >
          <ArrowForwardOutlined fontSize="small" color="action" />
        </ListFrameActionButton>
      ),
      alignment: "right",
      minWidth: 52,
      maxWidth: 52,
    },
  ];

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
