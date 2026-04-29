import { Button, Stack, Typography } from "@mui/material";
import routes, { routeBreadcrumbs } from "@/framework/routes";
import Breadcrumbs from "@/framework/Breadcrumbs";
import CaptionedFrame from "@/framework/view/CaptionedFrame";
import CaptionedValue from "@/framework/view/CaptionedValue";
import type { FundTransactionSortOrder } from "@/data/fundTypes";
import type { JSX } from "react";
import SearchBar from "@/framework/listframe/SearchBar";
import TransactionListFrame from "@/app/funds/[id]/TransactionListFrame";
import formatCurrency from "@/framework/formatCurrency";
import getApiClient from "@/data/getApiClient";

/**
 * Props for the Page Component.
 */
interface PageProps {
  readonly params: Promise<{
    id: string;
  }>;
  readonly searchParams: Promise<{
    transactionSearch?: string;
    transactionSort?: FundTransactionSortOrder;
  }>;
}

/**
 * Component that displays the view for a single fund.
 */
const Page = async function ({
  params,
  searchParams,
}: PageProps): Promise<JSX.Element> {
  const { id } = await params;
  const { transactionSearch, transactionSort } = await searchParams;

  const apiClient = getApiClient();
  const fundPromise = apiClient.GET("/funds/{fundId}", {
    params: {
      path: {
        fundId: id,
      },
    },
  });
  const fundTransactionsPromise = apiClient.GET(
    "/funds/{fundId}/transactions",
    {
      params: {
        path: {
          fundId: id,
        },
        query: {
          Search: transactionSearch ?? "",
          Sort: transactionSort ?? null,
        },
      },
    },
  );

  const [
    { data: fundData, error: fundError },
    { data: fundTransactionsData, error: fundTransactionsError },
  ] = await Promise.all([fundPromise, fundTransactionsPromise]);

  if (
    typeof fundData === "undefined" ||
    typeof fundTransactionsData === "undefined"
  ) {
    throw new Error(
      `Failed to fetch fund with ID ${id}: ${fundError?.detail ?? fundTransactionsError?.detail ?? "Unknown error"}`,
    );
  }

  const isSystemFund = fundData.name === "Unassigned";
  return (
    <Stack spacing={2}>
      <Stack
        direction="row"
        justifyContent="space-between"
        alignItems="center"
        maxWidth={1000}
      >
        <Breadcrumbs
          breadcrumbs={routeBreadcrumbs.funds.detail({
            id,
            name: fundData.name,
          })}
        />
        <Stack direction="row" spacing={1}>
          <Button
            variant="contained"
            color="primary"
            href={routes.funds.update(id)}
            disabled={isSystemFund}
          >
            Edit
          </Button>
          <Button
            variant="contained"
            color="error"
            href={routes.funds.delete(id)}
            disabled={isSystemFund}
          >
            Delete
          </Button>
        </Stack>
      </Stack>
      <CaptionedFrame caption="Details">
        <CaptionedValue caption="Name" value={fundData.name} />
        <CaptionedValue caption="Description" value={fundData.description} />
        <br />
        <CaptionedValue
          caption="Posted Balance"
          value={formatCurrency(fundData.currentBalance.postedBalance)}
        />
      </CaptionedFrame>
      <Stack spacing={2} style={{ maxWidth: 1000 }}>
        <Typography variant="h6">Transactions</Typography>
        <SearchBar paramName="transactionSearch" />
        <TransactionListFrame
          fund={fundData}
          data={fundTransactionsData.items}
          totalCount={fundTransactionsData.totalCount}
        />
      </Stack>
    </Stack>
  );
};

export default Page;
