import { Button, Stack, Typography } from "@mui/material";
import type { AccountTransactionSortOrder } from "@/data/accountTypes";
import Breadcrumbs from "@/framework/Breadcrumbs";
import Caption from "@/framework/view/Caption";
import CaptionedFrame from "@/framework/view/CaptionedFrame";
import CaptionedValue from "@/framework/view/CaptionedValue";
import type { JSX } from "react";
import SearchBar from "@/framework/listframe/SearchBar";
import TransactionListFrame from "@/app/accounts/[id]/TransactionListFrame";
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
    transactionSort?: AccountTransactionSortOrder;
  }>;
}

/**
 * Component that displays the view for a single account.
 */
const Page = async function ({
  params,
  searchParams,
}: PageProps): Promise<JSX.Element> {
  const { id } = await params;
  const { transactionSearch, transactionSort } = await searchParams;

  const apiClient = getApiClient();
  const accountPromise = apiClient.GET("/accounts/{accountId}", {
    params: {
      path: {
        accountId: id,
      },
    },
  });
  const accountTransactionsPromise = apiClient.GET(
    "/accounts/{accountId}/transactions",
    {
      params: {
        path: {
          accountId: id,
        },
        query: {
          Search: transactionSearch ?? "",
          Sort: transactionSort ?? null,
        },
      },
    },
  );

  const [
    { data: accountData, error: accountError },
    { data: accountTransactionsData, error: accountTransactionsError },
  ] = await Promise.all([accountPromise, accountTransactionsPromise]);

  if (
    typeof accountData === "undefined" ||
    typeof accountTransactionsData === "undefined"
  ) {
    throw new Error(
      `Failed to fetch account with ID ${id}: ${accountError?.detail ?? accountTransactionsError?.detail ?? "Unknown error"}`,
    );
  }

  return (
    <Stack spacing={2}>
      <Stack
        direction="row"
        justifyContent="space-between"
        alignItems="center"
        maxWidth={1000}
      >
        <Breadcrumbs
          breadcrumbs={[
            { label: "Accounts", href: "/accounts" },
            { label: accountData.name, href: `/accounts/${id}` },
          ]}
        />
        <Stack direction="row" spacing={1}>
          <Button
            variant="contained"
            color="primary"
            href={`/accounts/${id}/update`}
          >
            Edit
          </Button>
          <Button
            variant="contained"
            color="error"
            href={`/accounts/${id}/delete`}
          >
            Delete
          </Button>
        </Stack>
      </Stack>
      <CaptionedFrame caption="Details">
        <CaptionedValue caption="Name" value={accountData.name} />
        <CaptionedValue caption="Type" value={accountData.type} />
      </CaptionedFrame>
      <CaptionedFrame caption="Balance">
        <CaptionedValue
          caption="Total"
          value={formatCurrency(accountData.currentBalance.postedBalance)}
        />
        {accountData.currentBalance.availableToSpend !== null ? (
          <CaptionedValue
            caption="Available to Spend"
            value={formatCurrency(accountData.currentBalance.availableToSpend)}
          />
        ) : null}
        {accountData.currentBalance.fundBalances.length > 0 ? (
          <>
            <br />
            <Caption caption="Fund Balances" />
            {accountData.currentBalance.fundBalances.map((fundBalance) => (
              <CaptionedValue
                key={fundBalance.fundName}
                caption={fundBalance.fundName}
                value={formatCurrency(fundBalance.amount)}
              />
            ))}
          </>
        ) : null}
      </CaptionedFrame>
      <Stack spacing={2} style={{ maxWidth: 1000 }}>
        <Typography variant="h6">Transactions</Typography>
        <SearchBar paramName="transactionSearch" />
        <TransactionListFrame
          account={accountData}
          data={accountTransactionsData.items}
          totalCount={accountTransactionsData.totalCount}
        />
      </Stack>
    </Stack>
  );
};

export default Page;
