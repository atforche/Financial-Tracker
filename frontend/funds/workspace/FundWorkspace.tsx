import { Box, Stack } from "@mui/material";
import { getPageOffset, normalizePageValue } from "@/framework/listframe/page";
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
  page?: number | string | null;
  selectedFundId?: string;
  action?: FundWorkspaceAction;
}

/**
 * Props for the FundWorkspace component.
 */
interface FundWorkspaceProps {
  readonly searchParams: Promise<FundWorkspaceSearchParams>;
}

/**
 * Displays the fund workspace with list-backed inline actions.
 */
const FundWorkspace = async function ({
  searchParams,
}: FundWorkspaceProps): Promise<JSX.Element> {
  const { search, sort, page, selectedFundId, action } = await searchParams;
  const apiClient = getApiClient();
  const currentPage = normalizePageValue(page);

  const anyAccountingPeriodsPromise = apiClient.GET("/accounting-periods", {
    params: {
      query: {
        Limit: 1,
      },
    },
  });
  const openAccountingPeriodsPromise = apiClient.GET(
    "/accounting-periods/open",
  );
  const fundsPromise = apiClient.GET("/funds", {
    params: {
      query: {
        Search: search ?? "",
        Sort: sort ?? null,
        Limit: rowsPerPage,
        Offset: getPageOffset(currentPage),
      },
    },
  });

  const [
    { data: accountingPeriod },
    { data: openAccountingPeriods },
    { data: funds },
  ] = await Promise.all([
    anyAccountingPeriodsPromise,
    openAccountingPeriodsPromise,
    fundsPromise,
  ]);
  if (typeof accountingPeriod === "undefined") {
    throw new Error("Failed to fetch accounting periods");
  }
  if (typeof openAccountingPeriods === "undefined") {
    throw new Error("Failed to fetch open accounting periods");
  }
  if (typeof funds === "undefined") {
    throw new Error("Failed to fetch funds");
  }
  funds.items = funds.items.filter((fund) => fund.name !== "Unassigned");

  const selectedFund =
    funds.items.find((fund) => fund.id === selectedFundId) ?? null;
  const isInOnboardingMode = accountingPeriod.items.length === 0;

  if (typeof selectedFundId === "string" && selectedFund === null) {
    redirect(
      routes.workspace({
        search: search ?? "",
        ...(typeof sort !== "undefined" ? { sort } : {}),
        ...(typeof page !== "undefined" ? { page: currentPage } : {}),
        selectedFundId: "",
        ...(typeof action !== "undefined" ? { action } : {}),
      }),
    );
  }

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
          requestedAction={action ?? null}
        />
      </Box>
    </Stack>
  );
};

export type { FundWorkspaceAction, FundWorkspaceSearchParams };
export default FundWorkspace;
