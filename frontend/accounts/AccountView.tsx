import { Button, Stack, Typography } from "@mui/material";
import routes, { routeBreadcrumbs } from "@/framework/routes";
import type { AccountTransactionSortOrder } from "@/accounts/types";
import Breadcrumbs from "@/framework/Breadcrumbs";
import CaptionedFrame from "@/framework/view/CaptionedFrame";
import CaptionedValue from "@/framework/view/CaptionedValue";
import type { JSX } from "react";
import SearchBar from "@/framework/listframe/SearchBar";
import TransactionListFrame from "@/accounts/AccountTransactionListFrame";
import formatCurrency from "@/framework/formatCurrency";
import getApiClient from "@/framework/data/getApiClient";

/**
 * Props for the AccountView component.
 */
interface AccountViewProps {
  readonly params: Promise<{
    id: string;
  }>;
  readonly searchParams: Promise<{
    transactionSearch?: string;
    transactionSort?: AccountTransactionSortOrder;
  }>;
}

const formatAmount = function (value: number | null): string {
  return value === null ? "-" : formatCurrency(value);
};

/**
 * Component that displays the view for a single account.
 */
const AccountView = async function ({
  params,
  searchParams,
}: AccountViewProps): Promise<JSX.Element> {
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
  const transactionPromise = apiClient.GET(
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
    { data: transactionData, error: transactionError },
  ] = await Promise.all([accountPromise, transactionPromise]);

  if (
    typeof accountData === "undefined" ||
    typeof transactionData === "undefined"
  ) {
    throw new Error(
      `Failed to fetch account with ID ${id}: ${accountError?.detail ?? transactionError?.detail ?? "Unknown error"}`,
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
          breadcrumbs={routeBreadcrumbs.accounts.detail({
            id,
            name: accountData.name,
          })}
        />
        <Stack direction="row" spacing={1}>
          <Button
            variant="contained"
            color="primary"
            href={routes.accounts.update(id)}
          >
            Edit
          </Button>
          <Button
            variant="contained"
            color="error"
            href={routes.accounts.delete(id)}
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
          caption="Posted Balance"
          value={formatCurrency(accountData.currentBalance.postedBalance)}
        />
        <CaptionedValue
          caption="Available to Spend"
          value={formatAmount(accountData.currentBalance.availableToSpend)}
        />
        <CaptionedValue
          caption="Pending Debit"
          value={formatCurrency(accountData.currentBalance.pendingDebitAmount)}
        />
        <CaptionedValue
          caption="Pending Credit"
          value={formatCurrency(accountData.currentBalance.pendingCreditAmount)}
        />
      </CaptionedFrame>
      <Stack spacing={2} style={{ maxWidth: 1000 }}>
        <Typography variant="h6">Transactions</Typography>
        <SearchBar paramName="transactionSearch" />
        <TransactionListFrame
          account={accountData}
          data={transactionData.items}
          totalCount={transactionData.totalCount}
        />
      </Stack>
    </Stack>
  );
};

export default AccountView;
