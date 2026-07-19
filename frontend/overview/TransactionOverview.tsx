import { Stack, Typography } from "@mui/material";
import type { AccountingPeriod } from "@/accounting-periods/types";
import ContentSurface from "@/framework/view/ContentSurface";
import type { JSX } from "react";
import TransactionsByTypeCard from "@/transactions/TransactionsByTypeCard";
import createApiClient from "@/framework/data/createApiClient";
import unwrapApiResponse from "@/framework/data/unwrapApiResponse";

/**
 * Props for the TransactionOverview component.
 */
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
      <ContentSurface>
        <Stack spacing={2}>
          <Typography variant="h6" color="text.secondary">
            Current Transactions
          </Typography>
          <Typography color="text.secondary">
            No current accounting period is available to show transaction
            summaries.
          </Typography>
        </Stack>
      </ContentSurface>
    );
  }
  const apiClient = createApiClient();
  const response = await apiClient.GET(
    "/transactions/accounting-period-range",
    {
      params: {
        query: {
          "Range.Start": currentAccountingPeriod.id,
          "Range.End": currentAccountingPeriod.id,
          Limit: 10,
          Offset: 0,
        },
      },
    },
  );
  const data = unwrapApiResponse(
    response,
    "Failed to load transaction overview data",
  );
  return (
    <ContentSurface>
      <Stack spacing={2}>
        <Typography variant="h6" color="text.secondary">
          Current Transactions ({currentAccountingPeriod.name})
        </Typography>
        <TransactionsByTypeCard transactionTypes={data.transactionTypes} />
      </Stack>
    </ContentSurface>
  );
};

export default TransactionOverview;
