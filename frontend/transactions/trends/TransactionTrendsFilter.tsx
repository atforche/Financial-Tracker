"use client";

import { Autocomplete, Button, Checkbox, TextField } from "@mui/material";
import type { ChangeEvent, JSX } from "react";
import {
  normalizeAccountNames,
  shouldPersistAccountNames,
} from "@/accounts/trends/accountNameFilter";
import {
  normalizeFundNames,
  shouldPersistFundNames,
} from "@/funds/trends/fundNameFilter";
import {
  normalizeTransactionTypes,
  shouldPersistTransactionTypes,
  transactionTypeValues,
} from "@/transactions/trends/transactionTypeFilter";
import type { AccountingPeriod } from "@/accounting-periods/types";
import AccountingPeriodRangeFilter from "@/accounting-periods/AccountingPeriodRangeFilter";
import PageFilterFrame from "@/framework/view/PageFilterFrame";
import ToggleButtonSelector from "@/framework/forms/ToggleButtonSelector";
import type { TransactionTrendsSearchParams } from "@/transactions/trends/TransactionTrends";
import type { TransactionType } from "@/transactions/types";
import nameof from "@/framework/data/nameof";
import { setTrendRangeMode } from "@/framework/routes/trendRange";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";
import { useSearchParams } from "next/navigation";

/**
 * Trends filter mode values used in the Transactions view URL.
 */
type TransactionTrendsFilterMode = "accounting-period" | "date";

/**
 * Props for the TransactionTrendsFilter component.
 */
interface TransactionTrendsFilterProps {
  readonly accountingPeriods: readonly AccountingPeriod[];
  readonly availableAccountNames: readonly string[];
  readonly availableFundNames: readonly string[];
  readonly defaultAccountingPeriodId: string | null;
  readonly defaultStartDate: string;
  readonly defaultEndDate: string;
  readonly disabled?: boolean;
}

/**
 * Renders the trends filter card for the Transactions view.
 */
const TransactionTrendsFilter = function ({
  accountingPeriods,
  availableAccountNames,
  availableFundNames,
  defaultAccountingPeriodId,
  defaultStartDate,
  defaultEndDate,
  disabled = false,
}: TransactionTrendsFilterProps): JSX.Element {
  const searchParams = useSearchParams();

  const pageParamName = nameof<TransactionTrendsSearchParams>("page");
  const modeParamName = nameof<TransactionTrendsSearchParams>("mode");
  const transactionTypeParamName =
    nameof<TransactionTrendsSearchParams>("transactionType");
  const accountNameParamName =
    nameof<TransactionTrendsSearchParams>("accountName");
  const fundNameParamName = nameof<TransactionTrendsSearchParams>("fundName");
  const startAccountingPeriodIdParamName =
    nameof<TransactionTrendsSearchParams>("startAccountingPeriodId");
  const endAccountingPeriodIdParamName = nameof<TransactionTrendsSearchParams>(
    "endAccountingPeriodId",
  );
  const startDateParamName = nameof<TransactionTrendsSearchParams>("startDate");
  const endDateParamName = nameof<TransactionTrendsSearchParams>("endDate");

  const currentMode: TransactionTrendsFilterMode =
    searchParams.get(modeParamName) === "accounting-period"
      ? "accounting-period"
      : "date";
  const currentTransactionTypes = normalizeTransactionTypes(
    searchParams.getAll(transactionTypeParamName),
  );
  const currentAccountNames = normalizeAccountNames(
    searchParams.getAll(accountNameParamName),
    availableAccountNames,
  );
  const currentFundNames = normalizeFundNames(
    searchParams.getAll(fundNameParamName),
    availableFundNames,
  );
  const currentStartAccountingPeriodId =
    searchParams.get(startAccountingPeriodIdParamName) ??
    defaultAccountingPeriodId ??
    "";
  const currentEndAccountingPeriodId =
    searchParams.get(endAccountingPeriodIdParamName) ??
    defaultAccountingPeriodId ??
    "";
  const currentStartDate =
    searchParams.get(startDateParamName) ?? defaultStartDate;
  const currentEndDate = searchParams.get(endDateParamName) ?? defaultEndDate;

  const updateParams = useSearchParamUpdater([pageParamName]);

  const hasActiveView =
    currentMode !== "date" ||
    shouldPersistTransactionTypes(currentTransactionTypes) ||
    shouldPersistAccountNames(currentAccountNames) ||
    shouldPersistFundNames(currentFundNames) ||
    currentStartAccountingPeriodId !== (defaultAccountingPeriodId ?? "") ||
    currentEndAccountingPeriodId !== (defaultAccountingPeriodId ?? "") ||
    currentStartDate !== defaultStartDate ||
    currentEndDate !== defaultEndDate;

  const handleTransactionTypeChange = function (
    nextTransactionTypes: readonly TransactionType[],
  ): void {
    updateParams((params) => {
      params.delete(transactionTypeParamName);
      if (shouldPersistTransactionTypes(nextTransactionTypes)) {
        nextTransactionTypes.forEach((transactionType) => {
          params.append(transactionTypeParamName, transactionType);
        });
      }
    });
  };

  const handleAccountNameChange = function (
    nextAccountNames: readonly string[],
  ): void {
    updateParams((params) => {
      params.delete(accountNameParamName);
      if (shouldPersistAccountNames(nextAccountNames)) {
        nextAccountNames.forEach((accountName) => {
          params.append(accountNameParamName, accountName);
        });
      }
    });
  };

  const handleFundNameChange = function (
    nextFundNames: readonly string[],
  ): void {
    updateParams((params) => {
      params.delete(fundNameParamName);
      if (shouldPersistFundNames(nextFundNames)) {
        nextFundNames.forEach((fundName) => {
          params.append(fundNameParamName, fundName);
        });
      }
    });
  };

  const handleModeChange = function (
    nextMode: TransactionTrendsFilterMode,
  ): void {
    updateParams((params) => {
      setTrendRangeMode(params, nextMode, {
        defaultAccountingPeriodId,
        defaultStartDate,
        defaultEndDate,
      });
    });
  };

  const handleAccountingPeriodRangeChange = function (range: {
    readonly start: string;
    readonly end: string;
  }): void {
    updateParams((params) => {
      params.set(startAccountingPeriodIdParamName, range.start);
      params.set(endAccountingPeriodIdParamName, range.end);
    });
  };

  const handleStartDateChange = function (
    event: ChangeEvent<HTMLInputElement>,
  ): void {
    const nextStartDate = event.target.value;
    const nextEndDate =
      nextStartDate > currentEndDate ? nextStartDate : currentEndDate;

    updateParams((params) => {
      params.set(startDateParamName, nextStartDate);
      params.set(endDateParamName, nextEndDate);
    });
  };

  const handleEndDateChange = function (
    event: ChangeEvent<HTMLInputElement>,
  ): void {
    const nextEndDate = event.target.value;
    const nextStartDate =
      nextEndDate < currentStartDate ? nextEndDate : currentStartDate;

    updateParams((params) => {
      params.set(startDateParamName, nextStartDate);
      params.set(endDateParamName, nextEndDate);
    });
  };

  const clearView = function (): void {
    updateParams((params) => {
      params.delete(transactionTypeParamName);
      params.delete(accountNameParamName);
      params.delete(fundNameParamName);
      params.set(modeParamName, "date");
      params.delete(startAccountingPeriodIdParamName);
      params.delete(endAccountingPeriodIdParamName);
      params.set(startDateParamName, defaultStartDate);
      params.set(endDateParamName, defaultEndDate);
    });
  };

  const sharedAutocompleteSx = {
    minWidth: { xs: "100%", sm: 280 },
    flex: { md: 1 },
  };

  const sharedFieldSx = {
    minWidth: { xs: "100%", sm: 180 },
  };

  return (
    <PageFilterFrame title="Transaction Trends">
      <ToggleButtonSelector
        value={currentMode}
        disabled={disabled}
        onChange={handleModeChange}
        options={[
          { value: "date", label: "Dates" },
          {
            value: "accounting-period",
            label: "Accounting periods",
            disabled: defaultAccountingPeriodId === null,
          },
        ]}
      />
      {currentMode === "accounting-period" ? (
        <AccountingPeriodRangeFilter
          accountingPeriods={accountingPeriods}
          startValue={currentStartAccountingPeriodId}
          endValue={currentEndAccountingPeriodId}
          onChange={handleAccountingPeriodRangeChange}
          disabled={disabled}
        />
      ) : (
        <>
          <TextField
            size="small"
            label="Start date"
            type="date"
            value={currentStartDate}
            onChange={handleStartDateChange}
            disabled={disabled}
            sx={sharedFieldSx}
            slotProps={{
              inputLabel: {
                shrink: true,
              },
            }}
          />
          <TextField
            size="small"
            label="End date"
            type="date"
            value={currentEndDate}
            onChange={handleEndDateChange}
            disabled={disabled}
            sx={sharedFieldSx}
            slotProps={{
              inputLabel: {
                shrink: true,
              },
            }}
          />
        </>
      )}
      <Autocomplete
        multiple
        disableCloseOnSelect
        size="small"
        options={[...transactionTypeValues]}
        value={[...currentTransactionTypes]}
        disabled={disabled}
        limitTags={1}
        sx={sharedAutocompleteSx}
        noOptionsText="No transaction types found"
        slotProps={{
          paper: {
            sx: {
              "& .MuiAutocomplete-listbox": {
                maxHeight: 320,
              },
            },
          },
        }}
        onChange={(_, nextTransactionTypes) => {
          handleTransactionTypeChange(nextTransactionTypes);
        }}
        renderOption={(props, option, { selected }) => (
          <li {...props}>
            <Checkbox size="small" checked={selected} sx={{ mr: 1 }} />
            {option}
          </li>
        )}
        renderInput={(params) => (
          <TextField
            {...params}
            label="Transaction types"
            {...(currentTransactionTypes.length === 0
              ? { placeholder: "All transaction types" }
              : {})}
          />
        )}
      />
      <Autocomplete
        multiple
        disableCloseOnSelect
        size="small"
        options={[...availableAccountNames]}
        value={[...currentAccountNames]}
        disabled={disabled || availableAccountNames.length === 0}
        limitTags={1}
        sx={sharedAutocompleteSx}
        noOptionsText={
          availableAccountNames.length === 0
            ? "No account names available"
            : "No account names found"
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
        onChange={(_, nextAccountNames) => {
          handleAccountNameChange(nextAccountNames);
        }}
        renderOption={(props, option, { selected }) => (
          <li {...props}>
            <Checkbox size="small" checked={selected} sx={{ mr: 1 }} />
            {option}
          </li>
        )}
        renderInput={(params) => (
          <TextField
            {...params}
            label="Account names"
            {...(currentAccountNames.length === 0
              ? { placeholder: "All account names" }
              : {})}
          />
        )}
      />
      <Autocomplete
        multiple
        disableCloseOnSelect
        size="small"
        options={[...availableFundNames]}
        value={[...currentFundNames]}
        disabled={disabled || availableFundNames.length === 0}
        limitTags={1}
        sx={sharedAutocompleteSx}
        noOptionsText={
          availableFundNames.length === 0
            ? "No fund names available"
            : "No fund names found"
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
        onChange={(_, nextFundNames) => {
          handleFundNameChange(nextFundNames);
        }}
        renderOption={(props, option, { selected }) => (
          <li {...props}>
            <Checkbox size="small" checked={selected} sx={{ mr: 1 }} />
            {option}
          </li>
        )}
        renderInput={(params) => (
          <TextField
            {...params}
            label="Fund names"
            {...(currentFundNames.length === 0
              ? { placeholder: "All fund names" }
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

export default TransactionTrendsFilter;
