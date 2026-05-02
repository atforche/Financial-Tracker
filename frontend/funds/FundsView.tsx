import Breadcrumbs from "@/framework/Breadcrumbs";
import FundListFrame from "@/funds/FundListFrame";
import type { FundSortOrder } from "@/funds/types";
import type { JSX } from "react";
import SearchBar from "@/framework/listframe/SearchBar";
import { Stack } from "@mui/material";
import breadcrumbs from "@/funds/breadcrumbs";
import getApiClient from "@/framework/data/getApiClient";
import nameof from "@/framework/data/nameof";

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
  const { search, sort } = await searchParams;

  const apiClient = getApiClient();
  const { data } = await apiClient.GET("/funds", {
    params: {
      query: {
        Search: search ?? "",
        Sort: sort ?? null,
      },
    },
  });
  if (typeof data === "undefined") {
    throw new Error(`Failed to fetch funds`);
  }

  return (
    <Stack spacing={2}>
      <Breadcrumbs breadcrumbs={breadcrumbs.index()} />
      <SearchBar paramName={nameof<FundsViewSearchParams>("search")} />
      <FundListFrame data={data.items} totalCount={data.totalCount} />
    </Stack>
  );
};

export type { FundsViewSearchParams };
export default FundsView;
