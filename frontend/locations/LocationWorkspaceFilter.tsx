"use client";

import type { AccountingPeriod } from "@/accounting-periods/types";
import type { JSX } from "react";
import SearchBar from "@/framework/view/SearchBar";
import TrendFilterHeader from "@/framework/view/TrendFilterHeader";
import { useSearchParams } from "next/navigation";

/**
 * Shows the location workspace's title with its range controls and name search.
 */
const LocationWorkspaceFilter = function ({
  accountingPeriods,
}: {
  readonly accountingPeriods: readonly AccountingPeriod[];
}): JSX.Element {
  const searchParams = useSearchParams();
  return (
    <TrendFilterHeader
      title="Locations"
      returnUrl={undefined}
      accountingPeriods={accountingPeriods}
      hasActiveFilters={(searchParams.get("search") ?? "").trim() !== ""}
      clearFilters={(params) => {
        params.delete("search");
      }}
    >
      <SearchBar searchParamName="search" pageParamName="page" />
    </TrendFilterHeader>
  );
};

export default LocationWorkspaceFilter;
