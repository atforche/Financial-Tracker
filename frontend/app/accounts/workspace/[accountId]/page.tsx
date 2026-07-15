import { Button, Stack, Typography } from "@mui/material";
import { getPageOffset, normalizePageValue } from "@/framework/listframe/page";
import type { AccountWorkspaceSearchParams } from "@/accounts/workspace/AccountWorkspace";
import ArrowBack from "@mui/icons-material/ArrowBack";
import type { JSX } from "react";
import Link from "next/link";
import ViewAccountForm from "@/accounts/workspace/ViewAccountForm";
import getApiClient from "@/framework/data/getApiClient";
import { redirect } from "next/navigation";
import routes from "@/accounts/routes";
import { rowsPerPage } from "@/framework/listframe/Constants";
import transactionRoutes from "@/transactions/routes";
import { getTransactionAccountBalanceEvents } from "@/transactions/postingHelpers";

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
  const apiClient = getApiClient();
  const currentBalanceEventPage = normalizePageValue(balanceEventPage);
  const [{ data: accounts }, { data: transactions }] = await Promise.all([
    apiClient.GET("/accounts/with-balances"),
    apiClient.GET("/transactions", {
      params: { query: { "Filter.AccountIds": [accountId] } },
    }),
  ]);
  const account = accounts?.items.find((item) => item.id === accountId);

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

  const allBalanceEvents =
    transactions?.items
      .flatMap(getTransactionAccountBalanceEvents)
      .filter((event) => event.account.id === account.id) ?? [];
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
    <Stack spacing={3} sx={{ width: "100%" }}>
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
    </Stack>
  );
};

export const dynamic = "force-dynamic";
export default AccountWorkspaceDetailPage;
