"use client";

import {
  getTransactionDestinationLabel,
  getTransactionSourceLabel,
} from "@/transactions/transactionListHelpers";
import { Button } from "@mui/material";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import type { JSX } from "react";
import ListFrame from "@/framework/listframe/ListFrame";
import type { Route } from "next";
import type { Transaction } from "@/transactions/types";
import { formatCurrency } from "@/framework/currencyHelpers";
import { useRouter } from "next/navigation";

interface LocationTrendsTransactionListFrameProps {
  readonly transactions: readonly Transaction[];
  readonly totalCount: number;
  readonly hasSelectedLocations: boolean;
  readonly transactionWorkspaceHref: Route;
}

/** Displays Transactions matched by the selected Location trends filters. */
const LocationTrendsTransactionListFrame = function ({
  transactions,
  totalCount,
  hasSelectedLocations,
  transactionWorkspaceHref,
}: LocationTrendsTransactionListFrameProps): JSX.Element {
  const router = useRouter();
  const columns: readonly ColumnDefinition<Transaction>[] = [
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
      minWidth: 170,
    },
    {
      name: "source",
      headerContent: "Source",
      getBodyContent: getTransactionSourceLabel,
      minWidth: 130,
    },
    {
      name: "destination",
      headerContent: "Destination",
      getBodyContent: getTransactionDestinationLabel,
      minWidth: 130,
    },
    {
      name: "amount",
      headerContent: "Amount",
      getBodyContent: (transaction) => formatCurrency(transaction.amount),
      alignment: "right",
      minWidth: 110,
    },
  ];

  return (
    <ListFrame
      title="Transactions"
      headerContent={
        <Button
          variant="outlined"
          onClick={() => {
            router.push(transactionWorkspaceHref);
          }}
        >
          View transactions
        </Button>
      }
      columns={columns}
      getId={(transaction) => transaction.id}
      data={transactions}
      totalCount={totalCount}
      pageParamName="page"
      hasActiveFilters={hasSelectedLocations}
      initialEmptyState={{
        title: "Select Locations",
        description: "Choose one or more Locations to view their activity.",
        action: null,
      }}
      filteredEmptyState={{
        title: "No Matching Transactions",
        description:
          "No transactions affected the selected Locations in this range.",
        action: null,
      }}
    />
  );
};

export default LocationTrendsTransactionListFrame;
