import { Button, Stack, Typography } from "@mui/material";
import {
  getPageOffset,
  normalizePageValue,
  rowsPerPage,
} from "@/framework/listframe/page";
import type { AccountWorkspaceSearchParams } from "@/accounts/workspace/types";
import ArrowBack from "@mui/icons-material/ArrowBack";
import type { JSX } from "react";
import Link from "next/link";
import PageLayout from "@/framework/view/PageLayout";
import ViewAccountForm from "@/accounts/workspace/ViewAccountForm";
import createApiClient from "@/framework/data/createApiClient";
import { getTransactionAccountBalanceEvents } from "@/transactions/postingHelpers";
import { redirect } from "next/navigation";
import routes from "@/accounts/routes";
import transactionRoutes from "@/transactions/routes";
import unwrapApiResponse from "@/framework/data/unwrapApiResponse";

/**
 * Props for the AccountWorkspaceDetailPage component.
 */
interface AccountWorkspaceDetailPageProps {
  readonly params: Promise<{
    accountId: string;
  }>;
  readonly searchParams: Promise<AccountWorkspaceSearchParams>;
}

/**
 * Page for viewing details of a single account within the workspace.
 */
const AccountWorkspaceDetailPage = async function ({
  params,
  searchParams,
}: AccountWorkspaceDetailPageProps): Promise<JSX.Element> {
  const { accountId } = await params;
  const resolvedSearchParams = await searchParams;
  const { search, accountType, balanceEventPage } = resolvedSearchParams;
  const apiClient = createApiClient();
  const currentBalanceEventPage = normalizePageValue(balanceEventPage);
  const [accountsResponse, transactionsResponse] = await Promise.all([
    apiClient.GET("/accounts/with-balances"),
    apiClient.GET("/transactions", {
      params: { query: { "Filter.AccountIds": [accountId] } },
    }),
  ]);
  const accounts = unwrapApiResponse(
    accountsResponse,
    "Failed to fetch accounts",
  );
  const transactions = unwrapApiResponse(
    transactionsResponse,
    "Failed to fetch account transactions",
  );
  const account = accounts.items.find((item) => item.id === accountId);

  const workspaceSearchParams: AccountWorkspaceSearchParams = {
    ...(typeof search !== "undefined" ? { search } : {}),
    ...(typeof accountType !== "undefined" ? { accountType } : {}),
  };
  const detailSearchParams: AccountWorkspaceSearchParams = {
    ...workspaceSearchParams,
    ...(typeof balanceEventPage !== "undefined" ? { balanceEventPage } : {}),
  };
  const workspaceUrl = routes.workspace(workspaceSearchParams);

  if (typeof account === "undefined") {
    redirect(workspaceUrl);
  }

  const allBalanceEvents = transactions.items
    .flatMap(getTransactionAccountBalanceEvents)
    .filter((event) => event.account.id === account.id);
  const balanceEventOffset = getPageOffset(currentBalanceEventPage);
  const balanceEvents = allBalanceEvents.slice(
    balanceEventOffset,
    balanceEventOffset + rowsPerPage,
  );

  const currentUrl = routes.workspaceDetail(account.id, detailSearchParams);
  const addTransactionHref = transactionRoutes.workspaceCreate({
    accountIds: [account.id],
    returnUrl: currentUrl,
  });

  return (
    <PageLayout>
      <Stack spacing={2.5}>
        <Link
          href={workspaceUrl}
          style={{ alignSelf: "flex-start", textDecoration: "none" }}
        >
          <Button component="span" startIcon={<ArrowBack />}>
            Back to Workspace
          </Button>
        </Link>
        <Typography variant="h4">Account Details</Typography>
      </Stack>
      <ViewAccountForm
        account={account}
        redirectUrl={currentUrl}
        recentBalanceEvents={balanceEvents}
        recentBalanceEventCount={allBalanceEvents.length}
        addTransactionHref={addTransactionHref}
      />
    </PageLayout>
  );
};

export default AccountWorkspaceDetailPage;
