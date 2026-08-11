import {
  getPageOffset,
  getRowsPerPage,
  normalizePageValue,
} from "@/framework/listframe/page";
import { FundBalanceEventSort } from "@/funds/types";
import FundWorkspacePageHeader from "@/funds/workspace/FundWorkspacePageHeader";
import type { FundWorkspaceSearchParams } from "@/funds/workspace/types";
import type { JSX } from "react";
import PageLayout from "@/framework/view/PageLayout";
import ResponsivePageSize from "@/framework/listframe/ResponsivePageSize";
import ViewFundForm from "@/funds/workspace/ViewFundForm";
import createApiClient from "@/framework/data/createApiClient";
import dayjs from "dayjs";
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
  const { search, balanceEventPage, balanceEventSort, pageSize } =
    resolvedSearchParams;
  const apiClient = await createApiClient();
  const currentBalanceEventPage = normalizePageValue(balanceEventPage);
  const rowsPerPage = getRowsPerPage(pageSize);
  const balanceEventOffset = getPageOffset(
    currentBalanceEventPage,
    rowsPerPage,
  );
  const recentActivityEndDate = dayjs().format("YYYY-MM-DD");
  const recentActivityStartDate = dayjs()
    .subtract(60, "day")
    .format("YYYY-MM-DD");
  const fundsResponse = await apiClient.GET("/funds/with-balances");
  const funds = unwrapApiResponse(fundsResponse, "Failed to fetch funds");
  const fund = funds.items.find((item) => item.id === fundId);

  const workspaceSearchParams: FundWorkspaceSearchParams = {
    ...(typeof search !== "undefined" ? { search } : {}),
  };
  const detailSearchParams: FundWorkspaceSearchParams = {
    ...workspaceSearchParams,
    ...(typeof balanceEventPage !== "undefined" ? { balanceEventPage } : {}),
    ...(typeof balanceEventSort !== "undefined" ? { balanceEventSort } : {}),
  };
  const workspaceUrl = routes.workspace(workspaceSearchParams);

  if (typeof fund === "undefined") {
    redirect(workspaceUrl);
  }

  const [
    balanceEventsResponse,
    recentActivityResponse,
    recentActivityBalancesResponse,
  ] = await Promise.all([
    apiClient.GET("/funds/balance-events/date-range", {
      params: {
        query: {
          "Range.Start": recentActivityStartDate,
          "Range.End": recentActivityEndDate,
          "Filter.Names": [fund.name],
          Sort: balanceEventSort ?? FundBalanceEventSort.DateDescending,
          Limit: rowsPerPage,
          Offset: balanceEventOffset,
        },
      },
    }),
    apiClient.GET("/funds/balance-events/date-range", {
      params: {
        query: {
          "Range.Start": recentActivityStartDate,
          "Range.End": recentActivityEndDate,
          "Filter.Names": [fund.name],
          Sort: FundBalanceEventSort.Date,
          Limit: 500,
          Offset: 0,
        },
      },
    }),
    apiClient.GET("/funds/date-range", {
      params: {
        query: {
          "Range.Start": recentActivityStartDate,
          "Range.End": recentActivityEndDate,
          "Filter.Names": [fund.name],
          Limit: 1,
          Offset: 0,
        },
      },
    }),
  ]);
  const balanceEvents = unwrapApiResponse(
    balanceEventsResponse,
    "Failed to fetch fund balance events",
  );
  const recentActivity = unwrapApiResponse(
    recentActivityResponse,
    "Failed to fetch recent fund activity",
  );
  const recentActivityBalances = unwrapApiResponse(
    recentActivityBalancesResponse,
    "Failed to fetch recent fund balance history",
  );

  const currentUrl = routes.workspaceDetail(fund.id, detailSearchParams);
  const addTransactionHref = transactionRoutes.workspaceCreate({
    fundIds: [fund.id],
    returnUrl: currentUrl,
  });

  return (
    <PageLayout>
      <ResponsivePageSize desktopBreakpoint="xl" />
      <FundWorkspacePageHeader backHref={workspaceUrl} title="Fund Details" />
      <ViewFundForm
        fund={fund}
        redirectUrl={currentUrl}
        deleteRedirectUrl={workspaceUrl}
        recentBalanceEvents={balanceEvents.items}
        recentBalanceEventCount={balanceEvents.totalCount}
        recentActivityEvents={recentActivity.items}
        recentActivityBalances={recentActivityBalances.dates}
        trendsHref={routes.trends({
          mode: "date",
          fundName: [fund.name],
          startDate: recentActivityStartDate,
          endDate: recentActivityEndDate,
        })}
        addTransactionHref={addTransactionHref}
      />
    </PageLayout>
  );
};

export default FundWorkspaceDetailPage;
