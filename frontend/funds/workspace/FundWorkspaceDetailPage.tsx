import {
  getPageOffset,
  normalizePageValue,
  rowsPerPage,
} from "@/framework/listframe/page";
import { FundBalanceEventSort } from "@/funds/types";
import FundWorkspacePageHeader from "@/funds/workspace/FundWorkspacePageHeader";
import type { FundWorkspaceSearchParams } from "@/funds/workspace/types";
import type { JSX } from "react";
import PageLayout from "@/framework/view/PageLayout";
import ViewFundForm from "@/funds/workspace/ViewFundForm";
import createApiClient from "@/framework/data/createApiClient";
import { redirect } from "next/navigation";
import routes from "@/funds/routes";
import transactionRoutes from "@/transactions/routes";
import unwrapApiResponse from "@/framework/data/unwrapApiResponse";

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
  const apiClient = createApiClient();
  const currentBalanceEventPage = normalizePageValue(balanceEventPage);
  const balanceEventOffset = getPageOffset(currentBalanceEventPage);
  const fundsResponse = await apiClient.GET("/funds/with-balances");
  const funds = unwrapApiResponse(fundsResponse, "Failed to fetch funds");
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

  const balanceEventsResponse = await apiClient.GET(
    "/funds/balance-events/date-range",
    {
      params: {
        query: {
          "Range.Start": "0001-01-01",
          "Range.End": "9999-12-31",
          "Filter.Names": [fund.name],
          Sort: FundBalanceEventSort.DateDescending,
          Limit: rowsPerPage,
          Offset: balanceEventOffset,
        },
      },
    },
  );
  const balanceEvents = unwrapApiResponse(
    balanceEventsResponse,
    "Failed to fetch fund balance events",
  );

  const currentUrl = routes.workspaceDetail(fund.id, detailSearchParams);
  const addTransactionHref = transactionRoutes.workspaceCreate({
    fundIds: [fund.id],
    returnUrl: currentUrl,
  });

  return (
    <PageLayout>
      <FundWorkspacePageHeader backHref={workspaceUrl} title="Fund Details" />
      <ViewFundForm
        fund={fund}
        redirectUrl={currentUrl}
        recentBalanceEvents={balanceEvents.items}
        recentBalanceEventCount={balanceEvents.totalCount}
        addTransactionHref={addTransactionHref}
      />
    </PageLayout>
  );
};

export default FundWorkspaceDetailPage;
