import {
  getPageOffset,
  getRowsPerPage,
  normalizePageValue,
} from "@/framework/listframe/page";
import type { JSX } from "react";
import type { LocationSortModel } from "@/framework/data/api";
import LocationWorkspaceFilter from "@/locations/LocationWorkspaceFilter";
import LocationWorkspaceListFrame from "@/locations/LocationWorkspaceListFrame";
import PageLayout from "@/framework/view/PageLayout";
import createApiClient from "@/framework/data/createApiClient";
import unwrapApiResponse from "@/framework/data/unwrapApiResponse";

/** Search parameters supported by the Location workspace. */
interface LocationWorkspaceSearchParams {
  readonly search?: string;
  readonly sort?: LocationSortModel;
  readonly page?: number | string | null;
  readonly pageSize?: number | string | null;
  readonly transactionPage?: number | string | null;
}

interface LocationWorkspaceProps {
  readonly searchParams: Promise<LocationWorkspaceSearchParams>;
}

/** Displays the searchable Location workspace. */
const LocationWorkspace = async function ({
  searchParams,
}: LocationWorkspaceProps): Promise<JSX.Element> {
  const { search, sort, page, pageSize } = await searchParams;
  const client = await createApiClient();
  const rowsPerPage = getRowsPerPage(pageSize);
  const locationsResponse = await client.GET("/locations", {
    params: {
      query: {
        "Filter.NameSearch": search ?? "",
        Sort: sort ?? null,
        Limit: rowsPerPage,
        Offset: getPageOffset(normalizePageValue(page), rowsPerPage),
      },
    },
  });
  const locations = unwrapApiResponse(
    locationsResponse,
    "Failed to fetch Locations",
  );

  return (
    <PageLayout>
      <LocationWorkspaceFilter />
      <LocationWorkspaceListFrame
        data={locations.items}
        totalCount={locations.totalCount}
      />
    </PageLayout>
  );
};

export type { LocationWorkspaceSearchParams };
export default LocationWorkspace;
