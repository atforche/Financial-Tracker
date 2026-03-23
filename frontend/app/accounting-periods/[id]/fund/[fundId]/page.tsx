import { Stack, Typography } from "@mui/material";
import Breadcrumbs from "@/framework/Breadcrumbs";
import CaptionedFrame from "@/framework/view/CaptionedFrame";
import CaptionedValue from "@/framework/view/CaptionedValue";
import FundBalanceFrame from "@/app/accounting-periods/[id]/fund/[fundId]/FundBalanceFrame";
import type { FundTransactionSortOrder } from "@/data/fundTypes";
import type { JSX } from "react";
import SearchBar from "@/framework/listframe/SearchBar";
import TransactionListFrame from "@/app/accounting-periods/[id]/fund/[fundId]/TransactionListFrame";
import getApiClient from "@/data/getApiClient";

/**
 * Props for the Page Component.
 */
interface PageProps {
  readonly params: Promise<{
    id: string;
    fundId: string;
  }>;
  readonly searchParams: Promise<{
    transactionSearch?: string;
    transactionSort?: FundTransactionSortOrder;
  }>;
}

/**
 * Component that views the view for a single fund in the context of an accounting period.
 */
const Page = async function ({
  params,
  searchParams,
}: PageProps): Promise<JSX.Element> {
  const { id, fundId } = await params;
  const { transactionSearch, transactionSort } = await searchParams;

  const apiClient = getApiClient();
  const accountingPeriodPromise = apiClient.GET(
    "/accounting-periods/{accountingPeriodId}",
    {
      params: {
        path: {
          accountingPeriodId: id,
        },
      },
    },
  );
  const fundPromise = apiClient.GET("/funds/{fundId}/{accountingPeriodId}", {
    params: {
      path: {
        fundId,
        accountingPeriodId: id,
      },
    },
  });
  const fundTransactionsPromise = apiClient.GET(
    "/funds/{fundId}/transactions",
    {
      params: {
        path: {
          fundId,
        },
        query: {
          AccountingPeriodId: id,
          Search: transactionSearch ?? "",
          Sort: transactionSort ?? null,
        },
      },
    },
  );
  const [
    { data: accountingPeriodData, error: accountingPeriodError },
    { data: fundData, error: fundError },
    { data: fundTransactionsData, error: fundTransactionsError },
  ] = await Promise.all([
    accountingPeriodPromise,
    fundPromise,
    fundTransactionsPromise,
  ]);

  if (
    typeof accountingPeriodData === "undefined" ||
    typeof fundData === "undefined" ||
    typeof fundTransactionsData === "undefined"
  ) {
    throw new Error(
      `Failed to fetch fund with ID ${fundId} for accounting period with ID ${id}: ${accountingPeriodError?.detail ?? fundError?.detail ?? fundTransactionsError?.detail ?? "Unknown error"}`,
    );
  }

  return (
    <Stack spacing={2}>
      <Breadcrumbs
        breadcrumbs={[
          { label: "Accounting Periods", href: "/accounting-periods" },
          {
            label: accountingPeriodData.name,
            href: `/accounting-periods/${id}`,
          },
          {
            label: fundData.name,
            href: `/accounting-periods/${id}/fund/${fundId}`,
          },
        ]}
      />
      <CaptionedFrame caption="Details">
        <CaptionedValue caption="Name" value={fundData.name} />
        <CaptionedValue caption="Description" value={fundData.description} />
      </CaptionedFrame>
      <FundBalanceFrame fund={fundData} />
      <Stack spacing={2} style={{ maxWidth: 1000 }}>
        <Typography variant="h6">Transactions</Typography>
        <SearchBar paramName="transactionSearch" />
        <TransactionListFrame
          accountingPeriod={accountingPeriodData}
          fund={fundData}
          data={fundTransactionsData.items}
          totalCount={fundTransactionsData.totalCount}
        />
      </Stack>
    </Stack>
  );
};

export default Page;
