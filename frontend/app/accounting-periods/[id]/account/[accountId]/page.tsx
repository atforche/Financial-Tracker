import { Stack, Typography } from "@mui/material";
import AccountBalanceFrame from "@/app/accounting-periods/[id]/account/[accountId]/AccountBalanceFrame";
import type { AccountTransactionSortOrder } from "@/data/accountTypes";
import Breadcrumbs from "@/framework/Breadcrumbs";
import CaptionedFrame from "@/framework/view/CaptionedFrame";
import CaptionedValue from "@/framework/view/CaptionedValue";
import type { JSX } from "react";
import SearchBar from "@/framework/listframe/SearchBar";
import TransactionListFrame from "@/app/accounting-periods/[id]/account/[accountId]/TransactionListFrame";
import getApiClient from "@/data/getApiClient";

/**
 * Props for the Page Component.
 */
interface PageProps {
  readonly params: Promise<{
    id: string;
    accountId: string;
  }>;
  readonly searchParams: Promise<{
    transactionSearch?: string;
    transactionSort?: AccountTransactionSortOrder;
  }>;
}

/**
 * Component that views the view for a single fund in the context of an accounting period.
 */
const Page = async function ({
  params,
  searchParams,
}: PageProps): Promise<JSX.Element> {
  const { id, accountId } = await params;
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
  const accountPromise = apiClient.GET(
    "/accounts/{accountId}/{accountingPeriodId}",
    {
      params: {
        path: {
          accountId,
          accountingPeriodId: id,
        },
      },
    },
  );
  const fundTransactionsPromise = apiClient.GET(
    "/accounts/{accountId}/transactions",
    {
      params: {
        path: {
          accountId,
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
    { data: accountData, error: accountError },
    { data: fundTransactionsData, error: fundTransactionsError },
  ] = await Promise.all([
    accountingPeriodPromise,
    accountPromise,
    fundTransactionsPromise,
  ]);

  if (
    typeof accountingPeriodData === "undefined" ||
    typeof accountData === "undefined" ||
    typeof fundTransactionsData === "undefined"
  ) {
    throw new Error(
      `Failed to fetch account with ID ${accountId} for accounting period with ID ${id}: ${accountingPeriodError?.detail ?? accountError?.detail ?? fundTransactionsError?.detail ?? "Unknown error"}`,
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
            label: accountData.name,
            href: `/accounting-periods/${id}/account/${accountId}`,
          },
        ]}
      />
      <CaptionedFrame caption="Details">
        <CaptionedValue caption="Name" value={accountData.name} />
        <CaptionedValue caption="Type" value={accountData.type.toString()} />
      </CaptionedFrame>
      <AccountBalanceFrame account={accountData} />
      <Stack spacing={2} style={{ maxWidth: 1000 }}>
        <Typography variant="h6">Transactions</Typography>
        <SearchBar paramName="transactionSearch" />
        <TransactionListFrame
          accountingPeriod={accountingPeriodData}
          account={accountData}
          data={fundTransactionsData.items}
          totalCount={fundTransactionsData.totalCount}
        />
      </Stack>
    </Stack>
  );
};

export default Page;
