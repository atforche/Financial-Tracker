"use client";

import { IconButton, Stack, Typography } from "@mui/material";
import ChevronLeft from "@mui/icons-material/ChevronLeft";
import ChevronRight from "@mui/icons-material/ChevronRight";
import type { JSX } from "react";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";

/**
 * Props for the AccountingPeriodYearSelector component.
 */
interface AccountingPeriodYearSelectorProps {
  readonly value: number;
  readonly years: readonly number[];
}

/**
 * Steps between the years of accounting periods the workspace shows.
 */
const AccountingPeriodYearSelector = function ({
  value,
  years,
}: AccountingPeriodYearSelectorProps): JSX.Element {
  const updateParams = useSearchParamUpdater([]);
  const setYear = (year: number): void => {
    updateParams((params) => {
      params.set("year", year.toString());
    });
  };
  return (
    <Stack direction="row" spacing={1} sx={{ alignItems: "center" }}>
      <IconButton
        aria-label="Previous year"
        disabled={!years.includes(value - 1)}
        onClick={() => {
          setYear(value - 1);
        }}
      >
        <ChevronLeft />
      </IconButton>
      <Typography variant="h6">{value}</Typography>
      <IconButton
        aria-label="Next year"
        disabled={!years.includes(value + 1)}
        onClick={() => {
          setYear(value + 1);
        }}
      >
        <ChevronRight />
      </IconButton>
    </Stack>
  );
};

export default AccountingPeriodYearSelector;
