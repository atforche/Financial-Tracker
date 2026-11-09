"use client";

import {
  clearFundTrendsFilters,
  fundTrendsParamNames,
} from "@/funds/trends/helpers";
import {
  normalizeFundNames,
  shouldPersistFundNames,
} from "@/funds/trends/fundNameFilter";
import type { AccountingPeriod } from "@/accounting-periods/types";
import FundTrendsFundNameFilter from "@/funds/trends/FundTrendsFundNameFilter";
import type { JSX } from "react";
import TrendFilterHeader from "@/framework/view/TrendFilterHeader";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";
import { useSearchParams } from "next/navigation";

/**
 * Props for the FundTrendsFilter component.
 */
interface FundTrendsFilterProps {
  readonly accountingPeriods: readonly AccountingPeriod[];
  readonly availableFundNames: readonly string[];
  readonly returnUrl: string | undefined;
}

/**
 * Renders the header and filters for the fund trends page.
 */
const FundTrendsFilter = function ({
  accountingPeriods,
  availableFundNames,
  returnUrl,
}: FundTrendsFilterProps): JSX.Element {
  const searchParams = useSearchParams();
  const { page, fundName } = fundTrendsParamNames;
  const currentFundNames = normalizeFundNames(
    searchParams.getAll(fundName),
    availableFundNames,
  );
  const updateParams = useSearchParamUpdater([page]);

  return (
    <TrendFilterHeader
      title="Fund Trends"
      returnUrl={returnUrl}
      accountingPeriods={accountingPeriods}
      hasActiveFilters={shouldPersistFundNames(currentFundNames)}
      clearFilters={clearFundTrendsFilters}
    >
      <FundTrendsFundNameFilter
        availableFundNames={availableFundNames}
        value={currentFundNames}
        onChange={(nextFundNames) => {
          updateParams((params) => {
            params.delete(fundName);
            if (shouldPersistFundNames(nextFundNames)) {
              nextFundNames.forEach((name) => {
                params.append(fundName, name);
              });
            }
          });
        }}
      />
    </TrendFilterHeader>
  );
};

export default FundTrendsFilter;
