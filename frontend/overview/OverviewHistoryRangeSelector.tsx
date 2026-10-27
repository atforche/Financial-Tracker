"use client";

import type { JSX } from "react";
import type { OverviewHistoryRangeModel } from "@/framework/data/api";
import ToggleButtonSelector from "@/framework/forms/ToggleButtonSelector";
import { overviewHistoryOptions } from "@/overview/helpers";
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
          params.set("history", history);
        });
      }}
    />
  );
};

export default OverviewHistoryRangeSelector;
