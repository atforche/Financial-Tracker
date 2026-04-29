import Breadcrumbs from "@/framework/Breadcrumbs";
import FundListFrame from "@/app/funds/FundListFrame";
import type { FundSortOrder } from "@/data/fundTypes";
import type { JSX } from "react";
import SearchBar from "@/framework/listframe/SearchBar";
import { Stack } from "@mui/material";
import getApiClient from "@/data/getApiClient";
import { routeBreadcrumbs } from "@/framework/routes";

/**
 * Props for the Page component.
 */
interface PageProps {
  readonly searchParams: Promise<{
    fundSearch?: string;
    fundSort?: FundSortOrder;
  }>;
}

/**
 * Component that displays the Funds view.
 */
const Page = async function ({
  searchParams,
}: PageProps): Promise<JSX.Element> {
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

export default Page;
