"use client";

import type { JSX, ReactNode } from "react";
import {
  getTransactionDestinationLabel,
  getTransactionSourceLabel,
} from "@/transactions/transactionListHelpers";
import ArrowForwardOutlined from "@mui/icons-material/ArrowForwardOutlined";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import ListFrame from "@/framework/listframe/ListFrame";
import ListFrameActionButton from "@/framework/listframe/ListFrameActionButton";
import type { Transaction } from "@/transactions/types";
import { formatCurrency } from "@/framework/currencyHelpers";
import routes from "@/transactions/routes";
import { useRouter } from "next/navigation";

/**
 * Props for the RecentTransactionsFrame component.
 */
interface RecentTransactionsFrameProps {
  readonly transactions: readonly Transaction[];
  readonly totalCount: number;
  readonly headerContent?: ReactNode;
  readonly returnUrl: string;
}

/**
 * Displays the accounting period's transactions in a standard list frame.
 */
const RecentTransactionsFrame = function ({
  transactions,
  totalCount,
  headerContent,
  returnUrl,
}: RecentTransactionsFrameProps): JSX.Element {
  const router = useRouter();
  const openTransaction = function (transaction: Transaction): void {
    router.push(
      routes.workspaceDetail(transaction.id, {
        accountingPeriodIds: [transaction.accountingPeriodId],
        returnUrl,
      }),
    );
  };
  const columns: ColumnDefinition<Transaction>[] = [
    {
      name: "date",
      headerContent: "Date",
      getBodyContent: (transaction) => transaction.date,
      minWidth: 110,
    },
    {
      name: "description",
      headerContent: "Description",
      getBodyContent: (transaction) => transaction.description,
      mobilePrimary: true,
      minWidth: 150,
    },
    {
      name: "source",
      headerContent: "Source",
      getBodyContent: getTransactionSourceLabel,
      minWidth: 100,
    },
    {
      name: "destination",
      headerContent: "Destination",
      getBodyContent: getTransactionDestinationLabel,
      minWidth: 100,
    },
    {
      name: "amount",
      headerContent: "Amount",
      getBodyContent: (transaction) => formatCurrency(transaction.amount),
      alignment: "right",
      minWidth: 100,
    },
    {
      name: "actions",
      headerContent: "",
      getBodyContent: (transaction) => (
        <ListFrameActionButton
          ariaLabel={`View transaction ${transaction.id}`}
          onClick={(event) => {
            event.stopPropagation();
            openTransaction(transaction);
          }}
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
    <ListFrame
      title="Recent Transactions"
      headerContent={headerContent}
      columns={columns}
      getId={(transaction) => transaction.id}
      data={transactions}
      totalCount={totalCount}
      pageParamName="transactionPage"
      onRowClick={openTransaction}
      initialEmptyState={{
        title: "No transactions",
        description: "No transactions have been recorded for this period.",
        action: null,
      }}
    />
  );
};

export default RecentTransactionsFrame;
