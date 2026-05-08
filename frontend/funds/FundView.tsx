import { Button, Stack, Typography } from "@mui/material";
import Breadcrumbs from "@/framework/Breadcrumbs";
import CaptionedFrame from "@/framework/view/CaptionedFrame";
import CaptionedValue from "@/framework/view/CaptionedValue";
import type { FundTransactionSortOrder } from "@/funds/types";
import type { JSX } from "react";
import SearchBar from "@/framework/listframe/SearchBar";
import TransactionListFrame from "@/funds/FundTransactionListFrame";
import breadcrumbs from "@/funds/breadcrumbs";
import formatCurrency from "@/framework/formatCurrency";
import getApiClient from "@/framework/data/getApiClient";
import nameof from "@/framework/data/nameof";
import routes from "@/funds/routes";

/**
 * Parameters for the FundView component.
 */
interface FundViewParams {
  id: string;
}

/**
 * Search parameters for the FundView component.
 */
interface FundViewSearchParams {
  search?: string;
  sort?: FundTransactionSortOrder;
  page?: number;
}

/**
 * Props for the FundView component.
 */
interface FundViewProps {
  readonly params: Promise<FundViewParams>;
  readonly searchParams: Promise<FundViewSearchParams>;
}

/**
 * Component that displays the view for a single fund.
 */
const FundView = async function ({
  params,
  searchParams,
}: FundViewProps): Promise<JSX.Element> {
  const { id } = await params;
  const { search, sort } = await searchParams;

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
          Search: search ?? "",
          Sort: sort ?? null,
        },
      },
    },
  );

  const [
    { data: fund, error: fundError },
    { data: transactions, error: fundTransactionsError },
  ] = await Promise.all([fundPromise, fundTransactionsPromise]);

  if (typeof fund === "undefined" || typeof transactions === "undefined") {
    throw new Error(
      `Failed to fetch fund with ID ${id}: ${fundError?.detail ?? fundTransactionsError?.detail ?? "Unknown error"}`,
    );
  }

  const isSystemFund = fund.name === "Unassigned";
  return (
    <Stack spacing={2}>
      <Stack
        direction="row"
        justifyContent="space-between"
        alignItems="center"
        maxWidth={1000}
      >
        <Breadcrumbs breadcrumbs={breadcrumbs.detail(fund)} />
        <Stack direction="row" spacing={1}>
          <Button
            variant="contained"
            color="primary"
            href={routes.update({ id }, {})}
            disabled={isSystemFund}
          >
            Edit
          </Button>
          <Button
            variant="contained"
            color="error"
            href={routes.delete({ id }, {})}
            disabled={isSystemFund}
          >
            Delete
          </Button>
        </Stack>
      </Stack>
      <CaptionedFrame caption="Details">
        <CaptionedValue caption="Name" value={fund.name} />
        <CaptionedValue caption="Description" value={fund.description} />
        <br />
        <CaptionedValue
          caption="Posted Balance"
          value={formatCurrency(fund.currentBalance.postedBalance)}
        />
      </CaptionedFrame>
      <Stack spacing={2} style={{ maxWidth: 1000 }}>
        <Typography variant="h6">Transactions</Typography>
        <SearchBar paramName={nameof<FundViewSearchParams>("search")} />
        <TransactionListFrame
          fund={fund}
          data={transactions.items}
          totalCount={transactions.totalCount}
        />
      </Stack>
    </Stack>
  );
};

export type { FundViewParams, FundViewSearchParams };
export default FundView;
