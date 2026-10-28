"use client";

import {
  defaultOverviewHistory,
  overviewHistoryOptions,
} from "@/overview/helpers";
import type { JSX } from "react";
import type { OverviewHistoryRangeModel } from "@/framework/data/api";
import ToggleButtonSelector from "@/framework/forms/ToggleButtonSelector";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";

/**
 * Props for the OverviewHistoryRangeSelector component.
 */
interface OverviewHistoryRangeSelectorProps {
  readonly value: OverviewHistoryRangeModel;
}

/**
 * Chooses the length of the overview's balance history.
 */
const OverviewHistoryRangeSelector = function ({
  value,
}: OverviewHistoryRangeSelectorProps): JSX.Element {
  const updateParams = useSearchParamUpdater([]);
  return (
    <ToggleButtonSelector
      value={value}
      options={overviewHistoryOptions}
      onChange={(history) => {
        updateParams((params) => {
          // The default range is left out, matching the URL from getOverviewUrl
          if (history === defaultOverviewHistory) {
            params.delete("history");
          } else {
            params.set("history", history);
          }
        });
      }}
    />
  );
};

export default OverviewHistoryRangeSelector;
