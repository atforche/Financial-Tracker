"use client";

import type { AccountingPeriod } from "@/accounting-periods/types";
import AccountingPeriodRangeFilter from "@/accounting-periods/AccountingPeriodRangeFilter";
import type { JSX } from "react";
import { Stack } from "@mui/material";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";

/**
 * Chooses the accounting periods shown by a goal tab, independently of the history range.
 */
const GoalPeriodRangeFilter = function ({
  accountingPeriods,
  start,
  end,
}: {
  readonly accountingPeriods: readonly AccountingPeriod[];
  readonly start: string;
  readonly end: string;
}): JSX.Element {
  const updateParams = useSearchParamUpdater(["goalHistoryPage"]);
  return (
    <Stack direction="row" useFlexGap spacing={2} sx={{ flexWrap: "wrap" }}>
      <AccountingPeriodRangeFilter
        accountingPeriods={accountingPeriods}
        startValue={start}
        endValue={end}
        onChange={(range) => {
          updateParams((params) => {
            params.set("goalStartPeriodId", range.start);
            params.set("goalEndPeriodId", range.end);
          });
        }}
      />
    </Stack>
  );
};

export default GoalPeriodRangeFilter;
