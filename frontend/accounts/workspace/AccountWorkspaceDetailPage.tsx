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
import dayjs from "dayjs";
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
  const { search, accountType, balanceEventPage, balanceEventSort } =
    resolvedSearchParams;
  const apiClient = await createApiClient();
  const currentBalanceEventPage = normalizePageValue(balanceEventPage);
  const balanceEventOffset = getPageOffset(currentBalanceEventPage);
  const [accountsResponse, balanceEventsResponse] = await Promise.all([
    apiClient.GET("/accounts/with-balances"),
    apiClient.GET("/accounts/{accountId}/balance-events", {
      params: {
        path: { accountId },
        query: {
          "Range.Start": dayjs().subtract(60, "day").format("YYYY-MM-DD"),
          "Range.End": dayjs().format("YYYY-MM-DD"),
          Limit: rowsPerPage,
          Offset: balanceEventOffset,
          ...(balanceEventSort === undefined ? {} : { Sort: balanceEventSort }),
        },
      },
    }),
  ]);
  const accounts = unwrapApiResponse(
    accountsResponse,
    "Failed to fetch accounts",
  );
  const balanceEvents = unwrapApiResponse(
    balanceEventsResponse,
    "Failed to fetch account balance events",
  );
  const account = accounts.items.find((item) => item.id === accountId);

  const workspaceSearchParams: AccountWorkspaceSearchParams = {
    ...(typeof search !== "undefined" ? { search } : {}),
    ...(typeof accountType !== "undefined" ? { accountType } : {}),
  };
  const detailSearchParams: AccountWorkspaceSearchParams = {
    ...workspaceSearchParams,
    ...(typeof balanceEventPage !== "undefined" ? { balanceEventPage } : {}),
    ...(typeof balanceEventSort !== "undefined" ? { balanceEventSort } : {}),
  };
  const workspaceUrl = routes.workspace(workspaceSearchParams);

  if (typeof account === "undefined") {
    redirect(workspaceUrl);
  }

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
        deleteRedirectUrl={workspaceUrl}
        recentBalanceEvents={balanceEvents.items}
        recentBalanceEventCount={balanceEvents.totalCount}
        addTransactionHref={addTransactionHref}
      />
    </PageLayout>
  );
};

export default AccountWorkspaceDetailPage;
