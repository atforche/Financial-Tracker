"use client";

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
import { Button } from "@mui/material";
import type { JSX } from "react";
import MultiSelectAutocompleteFilter from "@/framework/forms/MultiSelectAutocompleteFilter";
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
 * Renders the filter card for the accounting period workspace with year and month filters.
 */
const AccountingPeriodWorkspaceFilter = function ({
  firstAccountingPeriod,
}: AccountingPeriodWorkspaceFilterProps): JSX.Element {
  const searchParams = useSearchParams();

  const pageParamName = nameof<AccountingPeriodWorkspaceSearchParams>("page");
  const yearsParamName = nameof<AccountingPeriodWorkspaceSearchParams>("years");
  const monthsParamName =
    nameof<AccountingPeriodWorkspaceSearchParams>("months");
  const selectedAccountingPeriodIdParamName =
    nameof<AccountingPeriodWorkspaceSearchParams>("selectedAccountingPeriodId");

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

  const updateParams = useSearchParamUpdater([
    pageParamName,
    selectedAccountingPeriodIdParamName,
  ]);

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
      <MultiSelectAutocompleteFilter
        label="Years"
        options={availableYears}
        value={currentYears}
        disabled={firstAccountingPeriod === null || availableYears.length === 0}
        placeholder="All years"
        noOptionsText={
          availableYears.length === 0 ? "No years available" : "No years found"
        }
        onChange={(nextYears) => {
          handleYearChange(nextYears);
        }}
        getOptionLabel={(year) => year.toString()}
      />
      <MultiSelectAutocompleteFilter
        label="Months"
        options={accountingPeriodMonths}
        value={currentMonths}
        disabled={
          firstAccountingPeriod === null || accountingPeriodMonths.length === 0
        }
        placeholder="All months"
        noOptionsText={
          accountingPeriodMonths.length === 0
            ? "No months available"
            : "No months found"
        }
        onChange={(nextMonths) => {
          handleMonthChange(nextMonths);
        }}
        getOptionLabel={formatAccountingPeriodMonth}
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
