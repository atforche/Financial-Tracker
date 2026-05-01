import Breadcrumbs from "@/framework/Breadcrumbs";
import FundListFrame from "@/funds/FundListFrame";
import type { FundSortOrder } from "@/funds/types";
import type { JSX } from "react";
import SearchBar from "@/framework/listframe/SearchBar";
import { Stack } from "@mui/material";
import getApiClient from "@/framework/data/getApiClient";
import { routeBreadcrumbs } from "@/framework/routes";

/**
 * Search parameters for the FundsView component.
 */
interface FundsViewSearchParams {
  fundSearch?: string;
  fundSort?: FundSortOrder;
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
  const { fundSearch, fundSort } = await searchParams;

  const apiClient = getApiClient();
  const { data } = await apiClient.GET("/funds", {
    params: {
      query: {
        Search: fundSearch ?? "",
        Sort: fundSort ?? null,
      },
    },
  });
  if (typeof data === "undefined") {
    throw new Error(`Failed to fetch funds`);
  }

  return (
    <Stack spacing={2}>
      <Breadcrumbs breadcrumbs={routeBreadcrumbs.funds.index()} />
      <SearchBar paramName="search" />
      <FundListFrame data={data.items} totalCount={data.totalCount} />
    </Stack>
  );
};

export type { FundsViewSearchParams };
export default FundsView;
