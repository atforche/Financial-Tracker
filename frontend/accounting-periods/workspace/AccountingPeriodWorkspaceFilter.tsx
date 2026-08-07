"use client";

import { Button, Stack } from "@mui/material";
import {
  accountingPeriodMonths,
  formatAccountingPeriodMonth,
} from "@/accounting-periods/helpers";
import {
  normalizeIntegerSearchParams,
  selectAvailableSearchParamValues,
} from "@/framework/routes/helpers";
import type { AccountingPeriod } from "@/accounting-periods/types";
import type { AccountingPeriodWorkspaceAction } from "@/accounting-periods/workspace/helpers";
import type { AccountingPeriodWorkspaceSearchParams } from "@/accounting-periods/workspace/AccountingPeriodWorkspace";
import type { JSX } from "react";
import MultiSelectAutocompleteFilter from "@/framework/forms/MultiSelectAutocompleteFilter";
import PageFilterFrame from "@/framework/view/PageFilterFrame";
import propertyName from "@/framework/data/propertyName";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";
import { useSearchParams } from "next/navigation";
import { useWriteAccess } from "@/framework/auth/ApplicationUserProvider";

/**
 * Props for the AccountingPeriodWorkspaceFilter component.
 */
interface AccountingPeriodWorkspaceFilterProps {
  readonly firstAccountingPeriod: AccountingPeriod | null;
  readonly isInOnboardingMode: boolean;
  readonly selectedAccountingPeriod: AccountingPeriod | null;
}

/**
 * Renders the filter card for the accounting period workspace with year and month filters.
 */
const AccountingPeriodWorkspaceFilter = function ({
  firstAccountingPeriod,
  isInOnboardingMode,
  selectedAccountingPeriod,
}: AccountingPeriodWorkspaceFilterProps): JSX.Element {
  const canWrite = useWriteAccess();
  const searchParams = useSearchParams();

  const pageParamName =
    propertyName<AccountingPeriodWorkspaceSearchParams>("page");
  const yearsParamName =
    propertyName<AccountingPeriodWorkspaceSearchParams>("years");
  const monthsParamName =
    propertyName<AccountingPeriodWorkspaceSearchParams>("months");
  const selectedAccountingPeriodIdParamName =
    propertyName<AccountingPeriodWorkspaceSearchParams>(
      "selectedAccountingPeriodId",
    );
  const actionParamName =
    propertyName<AccountingPeriodWorkspaceSearchParams>("action");

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

  const setAction = function (action: AccountingPeriodWorkspaceAction): void {
    updateParams((params) => {
      params.set(actionParamName, action);
    });
  };

  const selectedAction =
    selectedAccountingPeriod === null
      ? null
      : selectedAccountingPeriod.isOpen
        ? "close"
        : "reopen";

  return (
    <PageFilterFrame
      title="Accounting Periods Workspace"
      actions={
        <Stack direction="row" spacing={1.5} useFlexGap flexWrap="wrap">
          <Button
            variant="outlined"
            onClick={clearView}
            disabled={!hasActiveView}
          >
            Reset Filters
          </Button>
          {!canWrite || selectedAction === null ? null : (
            <Button
              variant="outlined"
              onClick={() => {
                setAction(selectedAction);
              }}
            >
              {selectedAction === "close" ? "Close Period" : "Reopen Period"}
            </Button>
          )}
          {!canWrite || selectedAccountingPeriod === null ? null : (
            <Button
              color="error"
              variant="outlined"
              onClick={() => {
                setAction("delete");
              }}
            >
              Delete Period
            </Button>
          )}
          {!canWrite ? null : (
            <Button
              variant="contained"
              onClick={() => {
                setAction("create");
              }}
            >
              {isInOnboardingMode
                ? "Create First Period"
                : "Create Next Period"}
            </Button>
          )}
        </Stack>
      }
    >
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
    </PageFilterFrame>
  );
};

export default AccountingPeriodWorkspaceFilter;
