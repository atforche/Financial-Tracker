import FundWorkspaceCards from "@/funds/workspace/FundWorkspaceCards";
import FundWorkspaceFilter from "@/funds/workspace/FundWorkspaceFilter";
import type { JSX } from "react";
import { Stack } from "@mui/material";
import getApiClient from "@/framework/data/getApiClient";

/**
 * Search parameters supported by the Funds workspace.
 */
interface FundWorkspaceSearchParams {
  search?: string;
  balanceEventPage?: number | string | null;
}

/**
 * Props for the FundWorkspace component.
 */
interface FundWorkspaceProps {
  readonly searchParams: Promise<FundWorkspaceSearchParams>;
}

/**
 * Displays the fund workspace with card-backed navigation.
 */
const FundWorkspace = async function ({
  searchParams,
}: FundWorkspaceProps): Promise<JSX.Element> {
  const { search } = await searchParams;
  const apiClient = getApiClient();
  const anyAccountingPeriodsPromise = apiClient.GET("/accounting-periods", {
    params: { query: { Limit: 1 } },
  });
  const fundsPromise = apiClient.GET("/funds/with-balances", {
    params: { query: { "Filter.NameSearch": search ?? "" } },
  });
  const [{ data: accountingPeriod }, { data: funds }] = await Promise.all([
    anyAccountingPeriodsPromise,
    fundsPromise,
  ]);

  if (typeof accountingPeriod === "undefined") {
    throw new Error("Failed to fetch accounting periods");
  }
  if (typeof funds === "undefined") {
    throw new Error("Failed to fetch funds");
  }

  const visibleFunds = funds.items.filter((fund) => fund.name !== "Unassigned");
  const isInOnboardingMode = accountingPeriod.items.length === 0;

  return (
    <Stack spacing={3} sx={{ width: "100%" }}>
      <FundWorkspaceFilter isInOnboardingMode={isInOnboardingMode} />
      <FundWorkspaceCards
        data={visibleFunds}
        isInOnboardingMode={isInOnboardingMode}
      />
    </Stack>
  );
};

export type { FundWorkspaceSearchParams };
export default FundWorkspace;
