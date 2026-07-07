import { Button, Stack, Typography } from "@mui/material";
import type { AccountWorkspaceSearchParams } from "@/accounts/workspace/AccountWorkspace";
import ArrowBack from "@mui/icons-material/ArrowBack";
import type { JSX } from "react";
import Link from "next/link";
import ViewAccountForm from "@/accounts/workspace/ViewAccountForm";
import getApiClient from "@/framework/data/getApiClient";
import { redirect } from "next/navigation";
import routes from "@/accounts/routes";

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
  const { search, sort, page } = resolvedSearchParams;
  const apiClient = getApiClient();
  const { data: account } = await apiClient.GET("/accounts/{accountId}", {
    params: {
      path: {
        accountId,
      },
    },
  });

  const workspaceSearchParams: AccountWorkspaceSearchParams = {
    ...(typeof search !== "undefined" ? { search } : {}),
    ...(typeof sort !== "undefined" ? { sort } : {}),
    ...(typeof page !== "undefined" ? { page } : {}),
  };
  const workspaceUrl = routes.workspace(workspaceSearchParams);

  if (typeof account === "undefined") {
    redirect(workspaceUrl);
  }

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
        redirectUrl={routes.workspaceDetail(account.id, workspaceSearchParams)}
      />
    </Stack>
  );
};

export const dynamic = "force-dynamic";
export default AccountWorkspaceDetailPage;
