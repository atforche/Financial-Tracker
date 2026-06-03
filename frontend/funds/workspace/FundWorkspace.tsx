import { Box, Stack } from "@mui/material";
import type { FundSortOrder } from "@/funds/types";
import FundWorkspaceActions from "@/funds/workspace/FundWorkspaceActions";
import FundWorkspaceFilter from "@/funds/workspace/FundWorkspaceFilter";
import FundWorkspaceListFrame from "@/funds/workspace/FundWorkspaceListFrame";
import type { JSX } from "react";
import getApiClient from "@/framework/data/getApiClient";
import { redirect } from "next/navigation";
import routes from "@/funds/routes";
import { rowsPerPage } from "@/framework/listframe/Constants";

type FundWorkspaceAction = "create" | "onboard" | "update" | "delete";

/**
 * Search parameters supported by the Funds workspace.
 */
interface FundWorkspaceSearchParams {
  search?: string;
  sort?: FundSortOrder;
  page?: number | string;
  selectedFundId?: string;
  action?: FundWorkspaceAction;
}

/**
 * Props for the FundWorkspace component.
 */
interface FundWorkspaceProps {
  readonly searchParams: Promise<FundWorkspaceSearchParams>;
}

const parsePageNumber = function (
  page: FundWorkspaceSearchParams["page"],
): number {
  const pageNumber =
    typeof page === "number" ? page : Number.parseInt(page ?? "1", 10);
  return Number.isFinite(pageNumber) && pageNumber > 0 ? pageNumber : 1;
};

const normalizeSearchParams = function (
  searchParams: FundWorkspaceSearchParams,
): FundWorkspaceSearchParams {
  return {
    ...(typeof searchParams.search === "string" && searchParams.search !== ""
      ? { search: searchParams.search }
      : {}),
    ...(typeof searchParams.sort === "string"
      ? { sort: searchParams.sort }
      : {}),
    ...(parsePageNumber(searchParams.page) > 1
      ? { page: parsePageNumber(searchParams.page) }
      : {}),
    ...(typeof searchParams.selectedFundId === "string"
      ? { selectedFundId: searchParams.selectedFundId }
      : {}),
    ...(typeof searchParams.action === "string"
      ? { action: searchParams.action }
      : {}),
  };
};

/**
 * Displays the fund workspace with list-backed inline actions.
 */
const FundWorkspace = async function ({
  searchParams,
}: FundWorkspaceProps): Promise<JSX.Element> {
  const resolvedSearchParams = normalizeSearchParams(await searchParams);
  const currentPage = parsePageNumber(resolvedSearchParams.page);
  const apiClient = getApiClient();

  const openAccountingPeriodsPromise = apiClient.GET(
    "/accounting-periods/open",
  );
  const fundsPromise = apiClient.GET("/funds", {
    params: {
      query: {
        Search: resolvedSearchParams.search ?? "",
        Sort: resolvedSearchParams.sort ?? null,
        Limit: rowsPerPage,
        Offset: (currentPage - 1) * rowsPerPage,
      },
    },
  });

  const [{ data: openAccountingPeriods }, { data: funds }] = await Promise.all([
    openAccountingPeriodsPromise,
    fundsPromise,
  ]);

  if (typeof openAccountingPeriods === "undefined") {
    throw new Error("Failed to fetch open accounting periods");
  }
  if (typeof funds === "undefined") {
    throw new Error("Failed to fetch funds");
  }

  const selectedFund =
    typeof resolvedSearchParams.selectedFundId === "string"
      ? (funds.items.find(
          (fund) => fund.id === resolvedSearchParams.selectedFundId,
        ) ?? null)
      : null;

  if (
    typeof resolvedSearchParams.selectedFundId === "string" &&
    selectedFund === null
  ) {
    const { ...remainingSearchParams } = resolvedSearchParams;
    const nextSearchParams =
      remainingSearchParams.action === "update" ||
      remainingSearchParams.action === "delete"
        ? (({ ...searchParamsWithoutAction }): FundWorkspaceSearchParams =>
            searchParamsWithoutAction)(remainingSearchParams)
        : remainingSearchParams;
    redirect(routes.workspace(nextSearchParams));
  }

  const isInOnboardingMode = openAccountingPeriods.length === 0;
  const redirectSearchParams = {
    ...(typeof resolvedSearchParams.search === "string"
      ? { search: resolvedSearchParams.search }
      : {}),
    ...(typeof resolvedSearchParams.sort === "string"
      ? { sort: resolvedSearchParams.sort }
      : {}),
    ...(currentPage > 1 ? { page: currentPage } : {}),
  };
  const updateRedirectUrl =
    selectedFund === null
      ? routes.workspace(redirectSearchParams)
      : routes.workspace({
          ...redirectSearchParams,
          selectedFundId: selectedFund.id,
        });

  return (
    <Stack spacing={3} sx={{ width: "100%" }}>
      <Stack spacing={3} sx={{ maxWidth: 1440, width: "100%" }}>
        <FundWorkspaceFilter />
      </Stack>
      <Box
        sx={{
          display: "grid",
          gap: 3,
          gridTemplateColumns:
            "repeat(auto-fit, minmax(min(100%, 600px), 1fr))",
        }}
      >
        <FundWorkspaceListFrame
          data={funds.items}
          totalCount={funds.totalCount}
          selectedFundId={selectedFund?.id ?? null}
          isInOnboardingMode={isInOnboardingMode}
        />
        <FundWorkspaceActions
          accountingPeriods={openAccountingPeriods}
          isInOnboardingMode={isInOnboardingMode}
          selectedFund={selectedFund}
          unassignedBalance={
            funds.items.find((fund) => fund.name === "Unassigned")
              ?.currentBalance.postedBalance ?? null
          }
          requestedAction={resolvedSearchParams.action ?? null}
          createRedirectUrl={routes.workspace(redirectSearchParams)}
          onboardRedirectUrl={routes.workspace(redirectSearchParams)}
          updateRedirectUrl={updateRedirectUrl}
          deleteRedirectUrl={routes.workspace(redirectSearchParams)}
        />
      </Box>
    </Stack>
  );
};

export type { FundWorkspaceAction, FundWorkspaceSearchParams };
export default FundWorkspace;
