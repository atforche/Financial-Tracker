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
import transactionRoutes from "@/transactions/routes";
import { useRouter } from "next/navigation";

interface RecentLocationTransactionsFrameProps {
  readonly transactions: readonly Transaction[];
  readonly totalCount: number;
  readonly headerContent: ReactNode;
  readonly locationId: string;
  readonly returnUrl: string;
}

/** Displays recent transactions affecting one Location. */
const RecentLocationTransactionsFrame = function ({
  transactions,
  totalCount,
  headerContent,
  locationId,
  returnUrl,
}: RecentLocationTransactionsFrameProps): JSX.Element {
  const router = useRouter();
  const openTransaction = function (transaction: Transaction): void {
    router.push(
      transactionRoutes.workspaceDetail(transaction.id, {
        locationIds: [locationId],
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
        title: "No Transactions",
        description: "No transactions have affected this Location.",
        action: null,
      }}
    />
  );
};

export default RecentLocationTransactionsFrame;
