import { Paper, Stack, Typography } from "@mui/material";
import type { AccountingPeriod } from "@/accounting-periods/types";
import type { JSX } from "react";
import TransactionTrendsByTypeCard from "@/transactions/trends/TransactionTrendsByTypeCard";
import getApiClient from "@/framework/data/getApiClient";

interface TransactionOverviewProps {
  readonly currentAccountingPeriod: AccountingPeriod | null;
}

/**
 * Overview component for transactions.
 */
const TransactionOverview = async function ({
  currentAccountingPeriod,
}: TransactionOverviewProps): Promise<JSX.Element> {
  if (currentAccountingPeriod === null) {
    return (
      <Paper sx={{ border: "1px solid", borderColor: "divider", p: 3 }}>
        <Stack spacing={2}>
          <Typography variant="h6" color="text.secondary">
            Current Transactions
          </Typography>
          <Typography color="text.secondary">
            No current accounting period is available to show transaction
            summaries.
          </Typography>
        </Stack>
      </Paper>
    );
  }

  const apiClient = getApiClient();
  const { data: trends } = await apiClient.GET("/transactions/trends", {
    params: {
      query: {
        Limit: 10,
        Offset: 0,
        StartAccountingPeriodId: currentAccountingPeriod.id,
        EndAccountingPeriodId: currentAccountingPeriod.id,
      },
    },
  });

  if (typeof trends === "undefined") {
    throw new Error("Failed to load transaction overview data");
  }

  return (
    <Paper sx={{ border: "1px solid", borderColor: "divider", p: 3 }}>
      <Stack spacing={2}>
        <Typography variant="h6" color="text.secondary">
          Current Transactions ({currentAccountingPeriod.name})
        </Typography>
        <TransactionTrendsByTypeCard trends={trends} />
      </Stack>
    </Paper>
  );
};

export default TransactionOverview;
