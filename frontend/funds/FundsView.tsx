import { Box, Stack } from "@mui/material";
import type { FundSortOrder, FundSummary } from "@/funds/types";
import Breadcrumbs from "@/framework/Breadcrumbs";
import FundListFrame from "@/funds/FundListFrame";
import type { JSX } from "react";
import SearchBar from "@/framework/listframe/SearchBar";
import SummaryCard from "@/framework/view/SummaryCard";
import breadcrumbs from "@/funds/breadcrumbs";
import formatCurrency from "@/framework/formatCurrency";
import getApiClient from "@/framework/data/getApiClient";
import nameof from "@/framework/data/nameof";
import { rowsPerPage } from "@/framework/listframe/Constants";

/**
 * Search parameters for the FundsView component.
 */
interface FundsViewSearchParams {
  search?: string;
  sort?: FundSortOrder;
  page?: number;
}

/**
 * Props for the FundsView component.
 */
interface FundsViewProps {
  readonly searchParams: Promise<FundsViewSearchParams>;
}

/**
 * Component that displays the Funds view.
 */
const FundsView = async function ({
  searchParams,
}: FundsViewProps): Promise<JSX.Element> {
  const { search, sort, page } = await searchParams;

  const apiClient = getApiClient();
  const fundsPromise = apiClient.GET("/funds", {
    params: {
      query: {
        Search: search ?? "",
        Sort: sort ?? null,
        Limit: rowsPerPage,
        Offset: ((page ?? 1) - 1) * rowsPerPage,
      },
    },
  });
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
  const summaryPromise = apiClient.GET("/funds/summary");

  const [{ data: funds }, { data: accountingPeriods }, { data: summary }] =
    await Promise.all([fundsPromise, accountingPeriodsPromise, summaryPromise]);

  if (
    typeof funds === "undefined" ||
    typeof accountingPeriods === "undefined" ||
    typeof summary === "undefined"
  ) {
    throw new Error(`Failed to fetch funds`);
  }

  const fundSummary: FundSummary = summary;

  return (
    <Stack spacing={2}>
      <Breadcrumbs breadcrumbs={breadcrumbs.index()} />
      <Box
        sx={{
          display: "grid",
          gap: 2,
          gridTemplateColumns: {
            xs: "1fr",
            md: "repeat(3, minmax(0, 1fr))",
          },
        }}
      >
        <SummaryCard
          title="Total Tracked Balance"
          value={formatCurrency(fundSummary.totalTrackedBalance)}
          description="Sum of all fund balances"
        />
        <SummaryCard
          title="Total Assigned Balance"
          value={formatCurrency(fundSummary.totalAssignedBalance)}
          description="All fund balances except Unassigned"
        />
        <SummaryCard
          title="Total Unassigned Balance"
          value={formatCurrency(fundSummary.totalUnassignedBalance)}
          description="Current balance of the Unassigned fund"
        />
      </Box>
      <SearchBar
        searchParamName={nameof<FundsViewSearchParams>("search")}
        pageParamName={nameof<FundsViewSearchParams>("page")}
      />
      <FundListFrame
        data={funds.items}
        isInOnboardingMode={accountingPeriods.totalCount === 0}
        totalCount={funds.totalCount}
      />
    </Stack>
  );
};

export type { FundsViewSearchParams };
export default FundsView;
