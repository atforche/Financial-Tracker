import type { JSX } from "react";
import OverviewCompositionPanel from "@/overview/OverviewCompositionPanel";
import OverviewCurrentPeriodPanel from "@/overview/OverviewCurrentPeriodPanel";
import type { OverviewData } from "@/overview/types";
import OverviewHero from "@/overview/OverviewHero";
import OverviewMetrics from "@/overview/OverviewMetrics";
import OverviewQuickActions from "@/overview/OverviewQuickActions";
import { Stack } from "@mui/material";
import getApiClient from "@/framework/data/getApiClient";

/**
 * Loads all data required by the overview page.
 */
const getOverviewData = async function (): Promise<OverviewData> {
  const apiClient = getApiClient();
  const accountSummaryPromise = apiClient.GET("/accounts/summary");
  const fundSummaryPromise = apiClient.GET("/funds/summary");
  const openAccountingPeriodsPromise = apiClient.GET(
    "/accounting-periods/open",
  );
  const accountingPeriodsPromise = apiClient.GET("/accounting-periods", {
    params: {
      query: {
        Search: "",
        Sort: null,
        Limit: 1,
        Offset: 0,
      },
    },
  });
  const accountsPromise = apiClient.GET("/accounts", {
    params: {
      query: {
        Search: "",
        Sort: null,
        Limit: 1,
        Offset: 0,
      },
    },
  });
  const fundsPromise = apiClient.GET("/funds", {
    params: {
      query: {
        Search: "",
        Sort: null,
        Limit: 1,
        Offset: 0,
      },
    },
  });

  const [
    { data: accountSummary },
    { data: fundSummary },
    { data: openAccountingPeriods },
    { data: accountingPeriods },
    { data: accounts },
    { data: funds },
  ] = await Promise.all([
    accountSummaryPromise,
    fundSummaryPromise,
    openAccountingPeriodsPromise,
    accountingPeriodsPromise,
    accountsPromise,
    fundsPromise,
  ]);

  if (
    typeof accountSummary === "undefined" ||
    typeof fundSummary === "undefined" ||
    typeof openAccountingPeriods === "undefined" ||
    typeof accountingPeriods === "undefined" ||
    typeof accounts === "undefined" ||
    typeof funds === "undefined"
  ) {
    throw new Error("Failed to fetch overview data");
  }

  return {
    accountSummary,
    fundSummary,
    currentAccountingPeriod: openAccountingPeriods[0] ?? null,
    openAccountingPeriods,
    totalAccountingPeriods: accountingPeriods.totalCount,
    totalAccounts: accounts.totalCount,
    totalFunds: funds.totalCount,
  };
};

/**
 * Component that displays the Overview view.
 */
const OverviewView = async function (): Promise<JSX.Element> {
  const data = await getOverviewData();

  return (
    <Stack spacing={3} sx={{ maxWidth: 1440 }}>
      <OverviewHero data={data} />
      <OverviewMetrics data={data} />
      <Stack
        sx={{
          display: "grid",
          gap: 3,
          gridTemplateColumns: {
            xs: "1fr",
            xl: "minmax(0, 1.05fr) minmax(0, 0.95fr)",
          },
        }}
      >
        <OverviewCurrentPeriodPanel data={data} />
        <OverviewCompositionPanel data={data} />
      </Stack>
      <OverviewQuickActions data={data} />
    </Stack>
  );
};

export default OverviewView;
