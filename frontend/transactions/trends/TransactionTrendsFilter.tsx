"use client";

import type {
  AccountingPeriod,
  AccountingPeriodRange,
} from "@/accounting-periods/types";
import { Button, TextField } from "@mui/material";
import type { ChangeEvent, JSX } from "react";
import {
  normalizeAccountNames,
  shouldPersistAccountNames,
} from "@/accounts/accountNameFilterHelpers";
import {
  normalizeFundNames,
  shouldPersistFundNames,
} from "@/funds/trends/fundNameFilter";
import {
  normalizeTransactionTypes,
  shouldPersistTransactionTypes,
  transactionTypeValues,
} from "@/transactions/trends/transactionTypeFilter";
import AccountNameFilter from "@/accounts/AccountNameFilter";
import AccountingPeriodRangeFilter from "@/accounting-periods/AccountingPeriodRangeFilter";
import MultiSelectAutocompleteFilter from "@/framework/forms/MultiSelectAutocompleteFilter";
import PageFilterFrame from "@/framework/view/PageFilterFrame";
import ToggleButtonSelector from "@/framework/forms/ToggleButtonSelector";
import type { TransactionTrendsSearchParams } from "@/transactions/trends/TransactionTrends";
import type { TransactionType } from "@/transactions/types";
import propertyName from "@/framework/data/propertyName";
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

  const pageParamName = propertyName<TransactionTrendsSearchParams>("page");
  const modeParamName = propertyName<TransactionTrendsSearchParams>("mode");
  const transactionTypeParamName =
    propertyName<TransactionTrendsSearchParams>("transactionType");
  const accountNameParamName =
    propertyName<TransactionTrendsSearchParams>("accountName");
  const fundNameParamName =
    propertyName<TransactionTrendsSearchParams>("fundName");
  const startAccountingPeriodIdParamName =
    propertyName<TransactionTrendsSearchParams>("startAccountingPeriodId");
  const endAccountingPeriodIdParamName =
    propertyName<TransactionTrendsSearchParams>("endAccountingPeriodId");
  const startDateParamName =
    propertyName<TransactionTrendsSearchParams>("startDate");
  const endDateParamName =
    propertyName<TransactionTrendsSearchParams>("endDate");

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

  const handleAccountingPeriodRangeChange = function (
    range: AccountingPeriodRange,
  ): void {
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
      <MultiSelectAutocompleteFilter
        label="Transaction types"
        options={transactionTypeValues}
        value={currentTransactionTypes}
        disabled={disabled}
        placeholder="All transaction types"
        noOptionsText="No transaction types found"
        onChange={(nextTransactionTypes) => {
          handleTransactionTypeChange(nextTransactionTypes);
        }}
      />
      <AccountNameFilter
        availableAccountNames={availableAccountNames}
        value={currentAccountNames}
        onChange={handleAccountNameChange}
        disabled={disabled}
      />
      <MultiSelectAutocompleteFilter
        label="Fund names"
        options={availableFundNames}
        value={currentFundNames}
        disabled={disabled || availableFundNames.length === 0}
        placeholder="All fund names"
        noOptionsText={
          availableFundNames.length === 0
            ? "No fund names available"
            : "No fund names found"
        }
        onChange={(nextFundNames) => {
          handleFundNameChange(nextFundNames);
        }}
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
