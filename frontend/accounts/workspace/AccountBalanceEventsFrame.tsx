"use client";

import {
  AccountTrendsBalanceEventType,
  type AccountWorkspaceBalanceEvent,
} from "@/accounts/types";
import { Box, Button } from "@mui/material";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import Frame from "@/framework/view/Frame";
import type { JSX } from "react";
import Link from "next/link";
import ListFrame from "@/framework/listframe/ListFrame";
import formatCurrency from "@/framework/formatCurrency";

const dateFormatter = new Intl.DateTimeFormat("en-US", {
  month: "short",
  day: "numeric",
  year: "numeric",
});

const formatAccountBalanceEventType = function (
  balanceEvent: AccountWorkspaceBalanceEvent,
): string {
  const baseLabel =
    balanceEvent.type === AccountTrendsBalanceEventType.Debit
      ? "Debit"
      : "Credit";

  return balanceEvent.isPosted
    ? baseLabel
    : `Pending ${baseLabel.toLowerCase()}`;
};

/**
 * Props for the AccountBalanceEventsFrame component.
 */
interface AccountBalanceEventsFrameProps {
  readonly data: AccountWorkspaceBalanceEvent[] | null;
  readonly totalCount: number | null;
  readonly addTransactionHref: string;
}

/**
 * Displays recent account balance events within the account workspace.
 */
const AccountBalanceEventsFrame = function ({
  data,
  totalCount,
  addTransactionHref,
}: AccountBalanceEventsFrameProps): JSX.Element {
  const columns: ColumnDefinition<AccountWorkspaceBalanceEvent>[] = [
    {
      name: "date",
      headerContent: "Event Date",
      getBodyContent: (balanceEvent) =>
        dateFormatter.format(new Date(`${balanceEvent.date}T00:00:00`)),
      minWidth: 135,
    },
    {
      name: "type",
      headerContent: "Type",
      getBodyContent: (balanceEvent) => (
        <Box
          component="span"
          sx={{
            color:
              balanceEvent.type === AccountTrendsBalanceEventType.Debit
                ? "warning.dark"
                : "info.dark",
            fontWeight: 600,
          }}
        >
          {formatAccountBalanceEventType(balanceEvent)}
        </Box>
      ),
      minWidth: 130,
    },
    {
      name: "amount",
      headerContent: "Amount",
      getBodyContent: (balanceEvent) => formatCurrency(balanceEvent.amount),
      alignment: "right",
      minWidth: 120,
    },
    {
      name: "before",
      headerContent: "Balance Before",
      getBodyContent: (balanceEvent) =>
        formatCurrency(balanceEvent.previousBalance),
      alignment: "right",
      minWidth: 150,
    },
    {
      name: "after",
      headerContent: "Balance After",
      getBodyContent: (balanceEvent) => formatCurrency(balanceEvent.newBalance),
      alignment: "right",
      minWidth: 150,
    },
  ];

  return (
    <Box sx={{ maxWidth: 1200, width: "100%" }}>
      <Frame
        title="Recent Balance Events"
        color="info"
        headerContent={
          <Button
            component={Link}
            href={addTransactionHref}
            variant="contained"
          >
            Add Transaction
          </Button>
        }
      >
        <ListFrame<AccountWorkspaceBalanceEvent>
          columns={columns}
          getId={(balanceEvent) =>
            `${balanceEvent.transactionId}-${balanceEvent.date}-${balanceEvent.type}-${balanceEvent.amount}`
          }
          data={data}
          totalCount={totalCount}
          searchParamName="balanceEventSearch"
          pageParamName="balanceEventPage"
          hasActiveFilters={false}
          initialEmptyState={{
            title: "No balance events yet",
            description:
              "Create or post a transaction for this account to start building its balance history.",
            action: (
              <Button
                component={Link}
                href={addTransactionHref}
                variant="contained"
              >
                Add Transaction
              </Button>
            ),
          }}
        />
      </Frame>
    </Box>
  );
};

export default AccountBalanceEventsFrame;
