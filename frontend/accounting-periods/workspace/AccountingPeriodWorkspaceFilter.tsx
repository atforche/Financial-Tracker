"use client";

import { Autocomplete, Button, Checkbox, TextField } from "@mui/material";
import {
  accountingPeriodMonths,
  formatAccountingPeriodMonth,
} from "@/accounting-periods/helpers";
import {
  normalizeIntegerSearchParams,
  selectAvailableSearchParamValues,
} from "@/framework/routes/helpers";
import type { AccountingPeriod } from "@/accounting-periods/types";
import type { AccountingPeriodWorkspaceSearchParams } from "@/accounting-periods/workspace/AccountingPeriodWorkspace";
import type { JSX } from "react";
import PageFilterFrame from "@/framework/view/PageFilterFrame";
import nameof from "@/framework/data/nameof";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";
import { useSearchParams } from "next/navigation";

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

  const pageParamName = nameof<AccountingPeriodWorkspaceSearchParams>("page");
  const yearsParamName = nameof<AccountingPeriodWorkspaceSearchParams>("years");
  const monthsParamName =
    nameof<AccountingPeriodWorkspaceSearchParams>("months");

  const currentYear = new Date().getFullYear();
  const firstAccountingPeriodYear = firstAccountingPeriod?.year ?? currentYear;
  const availableYears = Array.from(
    { length: currentYear - firstAccountingPeriodYear + 1 },
    (_, index) => firstAccountingPeriodYear + index,
  );

  const currentYears = selectAvailableSearchParamValues(
    normalizeIntegerSearchParams(
      searchParams.getAll(yearsParamName),
      firstAccountingPeriodYear,
      currentYear,
    ),
    availableYears,
    (value) => value,
    (value) => value,
  );
  const currentMonths = selectAvailableSearchParamValues(
    normalizeIntegerSearchParams(searchParams.getAll(monthsParamName), 1, 12),
    accountingPeriodMonths,
    (value) => value,
    (value) => value,
  );

  const updateParams = useSearchParamUpdater([pageParamName]);

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
    <PageFilterFrame title="Accounting Periods Workspace">
      <Autocomplete
        multiple
        disableCloseOnSelect
        size="small"
        options={[...availableYears]}
        value={[...currentYears]}
        disabled={firstAccountingPeriod === null || availableYears.length === 0}
        limitTags={1}
        sx={{ minWidth: { xs: "100%", sm: 280 }, flex: { md: 1 } }}
        noOptionsText={
          availableYears.length === 0 ? "No years available" : "No years found"
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
            {...(currentYears.length === 0 ? { placeholder: "All years" } : {})}
          />
        )}
      />
      <Autocomplete
        multiple
        disableCloseOnSelect
        size="small"
        options={accountingPeriodMonths}
        value={[...currentMonths]}
        disabled={
          firstAccountingPeriod === null || accountingPeriodMonths.length === 0
        }
        limitTags={1}
        sx={{ minWidth: { xs: "100%", sm: 280 }, flex: { md: 1 } }}
        noOptionsText={
          accountingPeriodMonths.length === 0
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
        getOptionLabel={formatAccountingPeriodMonth}
        renderOption={(props, option, { selected }) => (
          <li {...props}>
            <Checkbox size="small" checked={selected} sx={{ mr: 1 }} />
            {formatAccountingPeriodMonth(option)}
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
    </PageFilterFrame>
  );
};

export default AccountingPeriodWorkspaceFilter;
