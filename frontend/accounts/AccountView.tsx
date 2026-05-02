import { Button, Stack, Typography } from "@mui/material";
import type { AccountTransactionSortOrder } from "@/accounts/types";
import Breadcrumbs from "@/framework/Breadcrumbs";
import CaptionedFrame from "@/framework/view/CaptionedFrame";
import CaptionedValue from "@/framework/view/CaptionedValue";
import type { JSX } from "react";
import SearchBar from "@/framework/listframe/SearchBar";
import TransactionListFrame from "@/accounts/AccountTransactionListFrame";
import breadcrumbs from "@/accounts/breadcrumbs";
import formatCurrency from "@/framework/formatCurrency";
import getApiClient from "@/framework/data/getApiClient";
import nameof from "@/framework/data/nameof";
import routes from "@/accounts/routes";

/**
 * Parameters for the AccountView component.
 */
interface AccountViewParams {
  id: string;
}

/**
 * Search parameters for the AccountView component.
 */
interface AccountViewSearchParams {
  search?: string;
  sort?: AccountTransactionSortOrder;
  page?: number;
}

/**
 * Props for the AccountView component.
 */
interface AccountViewProps {
  readonly params: Promise<AccountViewParams>;
  readonly searchParams: Promise<AccountViewSearchParams>;
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
  const { search, sort } = await searchParams;

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
          Search: search ?? "",
          Sort: sort ?? null,
        },
      },
    },
  );

  const [
    { data: account, error: accountError },
    { data: transactions, error: transactionError },
  ] = await Promise.all([accountPromise, transactionPromise]);

  if (typeof account === "undefined" || typeof transactions === "undefined") {
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
        <Breadcrumbs breadcrumbs={breadcrumbs.detail(account)} />
        <Stack direction="row" spacing={1}>
          <Button
            variant="contained"
            color="primary"
            href={routes.update({ id }, {})}
          >
            Edit
          </Button>
          <Button
            variant="contained"
            color="error"
            href={routes.delete({ id }, {})}
          >
            Delete
          </Button>
        </Stack>
      </Stack>
      <CaptionedFrame caption="Details">
        <CaptionedValue caption="Name" value={account.name} />
        <CaptionedValue caption="Type" value={account.type} />
      </CaptionedFrame>
      <CaptionedFrame caption="Balance">
        <CaptionedValue
          caption="Posted Balance"
          value={formatCurrency(account.currentBalance.postedBalance)}
        />
        <CaptionedValue
          caption="Available to Spend"
          value={formatAmount(account.currentBalance.availableToSpend)}
        />
        <CaptionedValue
          caption="Pending Debit"
          value={formatCurrency(account.currentBalance.pendingDebitAmount)}
        />
        <CaptionedValue
          caption="Pending Credit"
          value={formatCurrency(account.currentBalance.pendingCreditAmount)}
        />
      </CaptionedFrame>
      <Stack spacing={2} style={{ maxWidth: 1000 }}>
        <Typography variant="h6">Transactions</Typography>
        <SearchBar paramName={nameof<AccountViewSearchParams>("search")} />
        <TransactionListFrame
          account={account}
          data={transactions.items}
          totalCount={transactions.totalCount}
        />
      </Stack>
    </Stack>
  );
};

export type { AccountViewParams, AccountViewSearchParams };
export default AccountView;
