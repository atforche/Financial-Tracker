import { Paper, Stack, Typography } from "@mui/material";
import Breadcrumbs from "@/framework/Breadcrumbs";
import FundListFrame from "@/funds/FundListFrame";
import type { FundSortOrder } from "@/funds/types";
import type { JSX } from "react";
import SearchBar from "@/framework/listframe/SearchBar";
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
  const unassignedFundPromise = apiClient.GET("/funds/unassigned");

  const [
    { data: funds },
    { data: accountingPeriods },
    { data: unassignedFund },
  ] = await Promise.all([
    fundsPromise,
    accountingPeriodsPromise,
    unassignedFundPromise,
  ]);

  if (
    typeof funds === "undefined" ||
    typeof accountingPeriods === "undefined"
  ) {
    throw new Error(`Failed to fetch funds`);
  }

  return (
    <Stack spacing={2}>
      <Breadcrumbs breadcrumbs={breadcrumbs.index()} />
      {typeof unassignedFund !== "undefined" && (
        <Paper
          sx={{
            p: 2,
            border: "1px solid",
            maxWidth: 400,
          }}
        >
          <Stack spacing={0.5}>
            <Typography variant="overline" color="text.secondary">
              Unassigned Fund
            </Typography>
            <Typography variant="h4">
              {formatCurrency(unassignedFund.currentBalance.postedBalance)}
            </Typography>
            <Typography variant="body2" color="text.secondary">
              Current unassigned balance
            </Typography>
          </Stack>
        </Paper>
      )}
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
