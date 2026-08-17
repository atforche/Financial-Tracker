"use client";

import type { JSX } from "react";
import type { LocationWorkspaceSearchParams } from "@/locations/LocationWorkspace";
import PageFilterFrame from "@/framework/view/PageFilterFrame";
import SearchBar from "@/framework/listframe/SearchBar";
import propertyName from "@/framework/data/propertyName";

/** Renders the Location workspace search filter. */
const LocationWorkspaceFilter = function (): JSX.Element {
  return (
    <PageFilterFrame title="Locations Workspace">
      <SearchBar
        searchParamName={propertyName<LocationWorkspaceSearchParams>("search")}
      />
    </PageFilterFrame>
  );
};

export default LocationWorkspaceFilter;
