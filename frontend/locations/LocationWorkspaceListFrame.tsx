"use client";

import { useRouter, useSearchParams } from "next/navigation";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import type { JSX } from "react";
import KeyboardArrowRight from "@mui/icons-material/KeyboardArrowRight";
import ListFrame from "@/framework/listframe/ListFrame";
import ListFrameActionButton from "@/framework/listframe/ListFrameActionButton";
import type { Location } from "@/locations/types";
import { LocationSortModel } from "@/framework/data/api";
import type { LocationWorkspaceSearchParams } from "@/locations/LocationWorkspace";
import createColumnSortProps from "@/framework/listframe/createColumnSortProps";
import parseEnumValue from "@/framework/data/parseEnumValue";
import propertyName from "@/framework/data/propertyName";
import routes from "@/locations/routes";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";

interface LocationWorkspaceListFrameProps {
  readonly data: Location[] | null;
  readonly totalCount: number | null;
}

/** Displays Locations matching the active workspace search. */
const LocationWorkspaceListFrame = function ({
  data,
  totalCount,
}: LocationWorkspaceListFrameProps): JSX.Element {
  const router = useRouter();
  const searchParams = useSearchParams();
  const pageParamName = propertyName<LocationWorkspaceSearchParams>("page");
  const searchParamName = propertyName<LocationWorkspaceSearchParams>("search");
  const sortParamName = propertyName<LocationWorkspaceSearchParams>("sort");
  const updateParams = useSearchParamUpdater([]);
  const currentSort = parseEnumValue(
    LocationSortModel,
    searchParams.get(sortParamName) ?? "",
  );
  const setSort = function (sort: LocationSortModel | null): void {
    updateParams((params) => {
      if (sort === null) {
        params.delete(sortParamName);
      } else {
        params.set(sortParamName, sort);
      }
      params.delete(pageParamName);
    });
  };
  const openLocation = function (location: Location): void {
    const search = searchParams.get(searchParamName);
    router.push(
      routes.workspaceDetail(location.id, {
        ...(search === null ? {} : { search }),
        ...(currentSort === null ? {} : { sort: currentSort }),
      }),
    );
  };
  const getSortProps = createColumnSortProps(currentSort, setSort);
  const columns: ColumnDefinition<Location>[] = [
    {
      name: "name",
      headerContent: "Name",
      getBodyContent: (location) => location.name,
      mobilePrimary: true,
      ...getSortProps(LocationSortModel.Name, LocationSortModel.NameDescending),
    },
    {
      name: "actions",
      headerContent: "",
      getBodyContent: (location) => (
        <ListFrameActionButton
          ariaLabel={`View ${location.name} details`}
          onClick={() => {
            openLocation(location);
          }}
        >
          <KeyboardArrowRight fontSize="small" color="action" />
        </ListFrameActionButton>
      ),
      alignment: "right",
      minWidth: 52,
      maxWidth: 52,
    },
  ];

  return (
    <ListFrame<Location>
      title="Locations"
      columns={columns}
      getId={(location) => location.id}
      data={data}
      totalCount={totalCount}
      pageParamName={pageParamName}
      onRowClick={openLocation}
      initialEmptyState={{
        title: "No Locations Yet",
        description: "Locations are created when you save a transaction.",
        action: null,
      }}
      filteredEmptyState={{
        title: "No Locations Match The Current Search",
        description: "Try a broader search term.",
        action: null,
      }}
    />
  );
};

export default LocationWorkspaceListFrame;
