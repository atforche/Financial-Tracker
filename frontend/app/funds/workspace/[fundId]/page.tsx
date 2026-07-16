import { Button, Stack, Typography } from "@mui/material";
import { getPageOffset, normalizePageValue } from "@/framework/listframe/page";
import ArrowBack from "@mui/icons-material/ArrowBack";
import type { FundWorkspaceSearchParams } from "@/funds/workspace/FundWorkspace";
import type { JSX } from "react";
import Link from "next/link";
import ViewFundForm from "@/funds/workspace/ViewFundForm";
import getApiClient from "@/framework/data/getApiClient";
import getApiData from "@/framework/data/apiResponse";
import { getTransactionFundBalanceEvents } from "@/transactions/postingHelpers";
import { redirect } from "next/navigation";
import routes from "@/funds/routes";
import { rowsPerPage } from "@/framework/listframe/Constants";
import transactionRoutes from "@/transactions/routes";

/**
 * Props for the FundWorkspaceDetailPage component.
 */
interface FundWorkspaceDetailPageProps {
  readonly params: Promise<{ fundId: string }>;
  readonly searchParams: Promise<FundWorkspaceSearchParams>;
}

/**
 * Page for viewing details of a single fund within the workspace.
 */
const FundWorkspaceDetailPage = async function ({
  params,
  searchParams,
}: FundWorkspaceDetailPageProps): Promise<JSX.Element> {
  const { fundId } = await params;
  const resolvedSearchParams = await searchParams;
  const { search, balanceEventPage } = resolvedSearchParams;
  const apiClient = getApiClient();
  const currentBalanceEventPage = normalizePageValue(balanceEventPage);
  const [fundsResponse, transactionsResponse] = await Promise.all([
    apiClient.GET("/funds/with-balances"),
    apiClient.GET("/transactions", {
      params: { query: { "Filter.FundIds": [fundId] } },
    }),
  ]);
  const funds = getApiData(fundsResponse, "Failed to fetch funds");
  const transactions = getApiData(
    transactionsResponse,
    "Failed to fetch fund transactions",
  );
  const fund = funds.items.find((item) => item.id === fundId);

  const workspaceSearchParams: FundWorkspaceSearchParams = {
    ...(typeof search !== "undefined" ? { search } : {}),
  };
  const detailSearchParams: FundWorkspaceSearchParams = {
    ...workspaceSearchParams,
    ...(typeof balanceEventPage !== "undefined" ? { balanceEventPage } : {}),
  };
  const workspaceUrl = routes.workspace(workspaceSearchParams);

  if (typeof fund === "undefined") {
    redirect(workspaceUrl);
  }

  const allBalanceEvents = transactions.items
    .flatMap(getTransactionFundBalanceEvents)
    .filter((event) => event.fund.id === fund.id);
  const balanceEventOffset = getPageOffset(currentBalanceEventPage);
  const balanceEvents = allBalanceEvents.slice(
    balanceEventOffset,
    balanceEventOffset + rowsPerPage,
  );

  const currentUrl = routes.workspaceDetail(fund.id, detailSearchParams);
  const addTransactionHref = transactionRoutes.workspaceCreate({
    fundIds: [fund.id],
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
        <Typography variant="h4">Fund Details</Typography>
      </Stack>
      <ViewFundForm
        fund={fund}
        redirectUrl={currentUrl}
        recentBalanceEvents={balanceEvents}
        recentBalanceEventCount={allBalanceEvents.length}
        addTransactionHref={addTransactionHref}
      />
    </Stack>
  );
};

export const dynamic = "force-dynamic";
export default FundWorkspaceDetailPage;
