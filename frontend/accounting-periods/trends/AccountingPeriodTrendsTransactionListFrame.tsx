"use client";

import { Button, IconButton } from "@mui/material";
import {
  type Transaction,
  TransactionSort,
  type TransactionSortValue,
} from "@/transactions/transaction";
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
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import createColumnSortProps from "@/framework/listframe/createColumnSortProps";
import type { JSX } from "react";
import ListFrame from "@/framework/listframe/ListFrame";
import formatCurrency from "@/framework/formatCurrency";
import routes from "@/transactions/routes";
import tryParseEnum from "@/framework/data/tryParseEnum";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";

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

  const sortParamName = "transactionSort";
  const pageParamName = "transactionPage";
  const searchParamName = "transactionSearch";
  const modeParamName = "mode";
  const startAccountingPeriodIdParamName = "startAccountingPeriodId";
  const endAccountingPeriodIdParamName = "endAccountingPeriodId";

  const updateParams = useSearchParamUpdater([pageParamName]);

  const setSort = function (sort: TransactionSortValue | null): void {
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
    searchParams.get(modeParamName) === "accounting-period" ||
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
      minWidth: 100,
    },
    {
      name: "destination",
      headerContent: "Destination",
      getBodyContent: getTransactionDestinationLabel,
      ...getSortProps(
        TransactionSort.Destination,
        TransactionSort.DestinationDescending,
      ),
      minWidth: 100,
    },
    {
      name: "amount",
      headerContent: "Amount",
      getBodyContent: (transaction) => formatCurrency(transaction.amount),
      ...getSortProps(TransactionSort.Amount, TransactionSort.AmountDescending),
      alignment: "right",
      minWidth: 100,
    },
    {
      name: "actions",
      headerContent: "",
      getBodyContent: (transaction) => (
        <IconButton
          size="small"
          color="primary"
          onClick={(event) => {
            event.stopPropagation();
            openTransactionWorkspace(transaction);
          }}
          aria-label={`Open transaction ${transaction.id}`}
        >
          <ArrowForwardOutlined fontSize="small" color="action" />
        </IconButton>
      ),
      alignment: "right",
      minWidth: 52,
      maxWidth: 52,
    },
  ];

  return (
    <ListFrame<Transaction>
      title="Transactions Across Selected Periods"
      columns={columns}
      getId={(transaction) => transaction.id}
      data={transactions}
      totalCount={totalCount}
      pageParamName={pageParamName}
      searchParamName={searchParamName}
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
