"use client";

import { Paper, Stack, Typography } from "@mui/material";
import ArrowForwardOutlined from "@mui/icons-material/ArrowForwardOutlined";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import type { JSX } from "react";
import ListFrame from "@/framework/listframe/ListFrame";
import type { Transaction } from "@/transactions/types";
import formatCurrency from "@/framework/formatCurrency";
import routes from "@/transactions/routes";
import { useRouter } from "next/navigation";

interface TransactionOverviewProps {
  readonly transactions: Transaction[];
  readonly totalCount: number;
}

const getDebitFrom = function (transaction: Transaction): string {
  if ("debitAccount" in transaction && transaction.debitAccount !== null) {
    return transaction.debitAccount.accountName;
  }
  if ("debitFund" in transaction) {
    return transaction.debitFund.fundName;
  }
  return "";
};

const getCreditTo = function (transaction: Transaction): string {
  if ("creditAccount" in transaction && transaction.creditAccount !== null) {
    return transaction.creditAccount.accountName;
  }
  if ("creditFund" in transaction) {
    return transaction.creditFund.fundName;
  }
  return "";
};

const getAccountIds = function (transaction: Transaction): string[] {
  const accountIds = new Set<string>();

  if ("debitAccount" in transaction && transaction.debitAccount !== null) {
    accountIds.add(transaction.debitAccount.accountId);
  }
  if ("creditAccount" in transaction && transaction.creditAccount !== null) {
    accountIds.add(transaction.creditAccount.accountId);
  }

  return Array.from(accountIds);
};

/**
 * Overview component for Transactions.
 */
const TransactionOverview = function ({
  transactions,
  totalCount,
}: TransactionOverviewProps): JSX.Element {
  const router = useRouter();

  const handleRowClick = function (transaction: Transaction): void {
    router.push(
      routes.workspace({
        accountingPeriodIds: [transaction.accountingPeriodId],
        accountIds: getAccountIds(transaction),
        selectedTransactionId: transaction.id,
      }),
    );
  };

  const columns: ColumnDefinition<Transaction>[] = [
    {
      name: "date",
      headerContent: "Date",
      getBodyContent: (transaction) => transaction.date,
      minWidth: 120,
    },
    {
      name: "location",
      headerContent: "Location",
      getBodyContent: (transaction) => transaction.location,
      minWidth: 160,
    },
    {
      name: "debitFrom",
      headerContent: "Debit From",
      getBodyContent: getDebitFrom,
      minWidth: 160,
    },
    {
      name: "creditTo",
      headerContent: "Credit To",
      getBodyContent: getCreditTo,
      minWidth: 160,
    },
    {
      name: "amount",
      headerContent: "Amount",
      getBodyContent: (transaction) => formatCurrency(transaction.amount),
      alignment: "right",
      minWidth: 110,
    },
    {
      name: "open",
      headerContent: "",
      getBodyContent: () => (
        <ArrowForwardOutlined fontSize="small" color="action" />
      ),
      alignment: "right",
      minWidth: 44,
      maxWidth: 44,
    },
  ];

  return (
    <Paper sx={{ border: "1px solid", borderColor: "divider", p: 3 }}>
      <Stack spacing={2.5}>
        <Typography variant="h6" color="text.secondary">
          Unposted transactions
        </Typography>
        <ListFrame<Transaction>
          columns={columns}
          getId={(transaction) => transaction.id}
          data={transactions}
          totalCount={totalCount}
          onRowClick={handleRowClick}
          searchParamName="overviewTransactionSearch"
          pageParamName="overviewTransactionPage"
          initialEmptyState={{
            title: "No unposted transactions",
            description: "All current transactions are fully posted.",
            action: null,
          }}
          filteredEmptyState={{
            title: "No unposted transactions",
            description: "All current transactions are fully posted.",
            action: null,
          }}
        />
      </Stack>
    </Paper>
  );
};

export default TransactionOverview;
