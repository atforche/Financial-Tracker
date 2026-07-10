"use client";

import {
  Autocomplete,
  Button,
  Checkbox,
  Paper,
  Stack,
  TextField,
  Typography,
} from "@mui/material";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import type { AccountingPeriod } from "@/accounting-periods/types";
import type { JSX } from "react";
import { buildUrl } from "@/framework/routes/helpers";

/**
 * Props for the AccountingPeriodWorkspaceFilter component.
 */
interface AccountingPeriodWorkspaceFilterProps {
  readonly firstAccountingPeriod: AccountingPeriod | null;
}

/**
 * Renders the filter card for the Accounting Period workspace with year and month filters.
 */
const AccountingPeriodWorkspaceFilter = function ({
  firstAccountingPeriod,
}: AccountingPeriodWorkspaceFilterProps): JSX.Element {
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const router = useRouter();

  const pageParamName = "page";
  const yearsParamName = "years";
  const monthsParamName = "months";

  const currentYear = new Date().getFullYear();
  const firstAccountingPeriodYear = firstAccountingPeriod?.year ?? currentYear;
  const availableYears = Array.from(
    { length: currentYear - firstAccountingPeriodYear + 1 },
    (_, index) => firstAccountingPeriodYear + index,
  );
  const availableMonths = Array.from({ length: 12 }, (_, index) => index + 1);
  const monthNames = Array.from({ length: 12 }, (_, index) =>
    new Date(2024, index, 1).toLocaleString("en", { month: "long" }),
  );

  const normalizeRequestedNumberValues = function (
    values: readonly string[],
    minimumValue: number,
    maximumValue: number,
  ): readonly number[] {
    const seenValues = new Set<number>();
    const normalizedValues: number[] = [];

    values.forEach((value) => {
      const parsedValue = Number.parseInt(value.trim(), 10);
      if (
        !Number.isFinite(parsedValue) ||
        parsedValue < minimumValue ||
        parsedValue > maximumValue ||
        seenValues.has(parsedValue)
      ) {
        return;
      }

      seenValues.add(parsedValue);
      normalizedValues.push(parsedValue);
    });

    return normalizedValues;
  };

  const normalizeSelectedNumberValues = function (
    values: readonly number[],
    availableValues: readonly number[],
  ): readonly number[] {
    const selectedValues = new Set(values);
    if (selectedValues.size === 0 || availableValues.length === 0) {
      return [];
    }

    return availableValues.filter((value) => selectedValues.has(value));
  };

  const currentYears = normalizeSelectedNumberValues(
    normalizeRequestedNumberValues(
      searchParams.getAll(yearsParamName),
      firstAccountingPeriodYear,
      currentYear,
    ),
    availableYears,
  );
  const currentMonths = normalizeSelectedNumberValues(
    normalizeRequestedNumberValues(searchParams.getAll(monthsParamName), 1, 12),
    availableMonths,
  );

  const updateParams = function (
    updater: (params: URLSearchParams) => void,
  ): void {
    const params = new URLSearchParams(searchParams.toString());
    updater(params);
    params.delete(pageParamName);
    router.replace(buildUrl(pathname, params), {
      scroll: false,
    });
  };

  const hasActiveView = currentYears.length > 0 || currentMonths.length > 0;

  const handleYearChange = function (nextYears: readonly number[]): void {
    updateParams((params) => {
      params.delete(yearsParamName);
      nextYears.forEach((year) => {
        params.append(yearsParamName, year.toString());
      });
    });
  };

  const handleMonthChange = function (nextMonths: readonly number[]): void {
    updateParams((params) => {
      params.delete(monthsParamName);
      nextMonths.forEach((month) => {
        params.append(monthsParamName, month.toString());
      });
    });
  };

  const clearView = function (): void {
    updateParams((params) => {
      params.delete(yearsParamName);
      params.delete(monthsParamName);
    });
  };

  return (
    <Paper
      sx={{
        position: "sticky",
        top: 10,
        zIndex: (theme) => theme.zIndex.appBar - 1,
        border: "1px solid",
        borderColor: "divider",
        borderRadius: 3,
        bgcolor: "background.paper",
        p: { xs: 2, md: 2.5 },
      }}
    >
      <Stack spacing={2}>
        <Stack spacing={0.5}>
          <Typography variant="h5">Accounting Periods Workspace</Typography>
        </Stack>
        <Stack
          direction="row"
          spacing={1.5}
          useFlexGap
          flexWrap="wrap"
          alignItems={{ xs: "stretch", md: "center" }}
        >
          <Autocomplete
            multiple
            disableCloseOnSelect
            size="small"
            options={[...availableYears]}
            value={[...currentYears]}
            disabled={
              firstAccountingPeriod === null || availableYears.length === 0
            }
            limitTags={1}
            sx={{ minWidth: { xs: "100%", sm: 280 }, flex: { md: 1 } }}
            noOptionsText={
              availableYears.length === 0
                ? "No years available"
                : "No years found"
            }
            slotProps={{
              paper: {
                sx: {
                  "& .MuiAutocomplete-listbox": {
                    maxHeight: 320,
                  },
                },
              },
            }}
            onChange={(_, nextYears) => {
              handleYearChange(nextYears);
            }}
            getOptionLabel={(year) => year.toString()}
            renderOption={(props, option, { selected }) => (
              <li {...props}>
                <Checkbox size="small" checked={selected} sx={{ mr: 1 }} />
                {option}
              </li>
            )}
            renderInput={(params) => (
              <TextField
                {...params}
                label="Years"
                {...(currentYears.length === 0
                  ? { placeholder: "All years" }
                  : {})}
              />
            )}
          />
          <Autocomplete
            multiple
            disableCloseOnSelect
            size="small"
            options={[...availableMonths]}
            value={[...currentMonths]}
            disabled={
              firstAccountingPeriod === null || availableMonths.length === 0
            }
            limitTags={1}
            sx={{ minWidth: { xs: "100%", sm: 280 }, flex: { md: 1 } }}
            noOptionsText={
              availableMonths.length === 0
                ? "No months available"
                : "No months found"
            }
            slotProps={{
              paper: {
                sx: {
                  "& .MuiAutocomplete-listbox": {
                    maxHeight: 320,
                  },
                },
              },
            }}
            onChange={(_, nextMonths) => {
              handleMonthChange(nextMonths);
            }}
            getOptionLabel={(month) =>
              monthNames[month - 1] ?? month.toString()
            }
            renderOption={(props, option, { selected }) => (
              <li {...props}>
                <Checkbox size="small" checked={selected} sx={{ mr: 1 }} />
                {monthNames[option - 1] ?? option.toString()}
              </li>
            )}
            renderInput={(params) => (
              <TextField
                {...params}
                label="Months"
                {...(currentMonths.length === 0
                  ? { placeholder: "All months" }
                  : {})}
              />
            )}
          />
          <Button
            variant="outlined"
            onClick={clearView}
            disabled={!hasActiveView}
            sx={{ flexShrink: 0 }}
          >
            Reset filters
          </Button>
        </Stack>
      </Stack>
    </Paper>
  );
};

export default AccountingPeriodWorkspaceFilter;
