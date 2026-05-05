import Breadcrumbs from "@/framework/Breadcrumbs";
import FundListFrame from "@/funds/index/FundListFrame";
import type { FundSortOrder } from "@/funds/types";
import type { JSX } from "react";
import SearchBar from "@/framework/listframe/SearchBar";
import { Stack } from "@mui/material";
import breadcrumbs from "@/funds/breadcrumbs";
import getApiClient from "@/framework/data/getApiClient";
import nameof from "@/framework/data/nameof";

/**
 * Search parameters for the FundIndexView component.
 */
interface FundIndexViewSearchParams {
  search?: string;
  sort?: FundSortOrder;
  page?: number;
}

/**
 * Props for the FundIndexView component.
 */
interface FundIndexViewProps {
  readonly searchParams: Promise<FundIndexViewSearchParams>;
}

/**
 * Component that displays the FundIndex view.
 */
const FundIndexView = async function ({
  searchParams,
}: FundIndexViewProps): Promise<JSX.Element> {
  const { search, sort } = await searchParams;

  const apiClient = getApiClient();
  const { data: funds } = await apiClient.GET("/funds", {
    params: {
      query: {
        Search: search ?? "",
        Sort: sort ?? null,
      },
    },
  });
  if (typeof funds === "undefined") {
    throw new Error(`Failed to fetch funds`);
  }

  return (
    <Stack spacing={2}>
      <Breadcrumbs breadcrumbs={breadcrumbs.index()} />
      <SearchBar paramName={nameof<FundIndexViewSearchParams>("search")} />
      <FundListFrame data={funds.items} totalCount={funds.totalCount} />
    </Stack>
  );
};

export type { FundIndexViewSearchParams };
export default FundIndexView;
