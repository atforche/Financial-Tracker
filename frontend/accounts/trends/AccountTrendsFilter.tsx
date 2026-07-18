"use client";

import type {
  AccountingPeriod,
  AccountingPeriodRange,
} from "@/accounting-periods/types";
import { Button, TextField } from "@mui/material";
import {
  type TrendRangeMode,
  setTrendRangeMode,
} from "@/framework/routes/trendRange";
import {
  normalizeAccountNames,
  shouldPersistAccountNames,
} from "@/accounts/accountNameFilterHelpers";
import {
  normalizeAccountTypes,
  shouldPersistAccountTypes,
} from "@/accounts/accountTypeFilterHelpers";
import AccountNameFilter from "@/accounts/AccountNameFilter";
import type { AccountType } from "@/accounts/types";
import AccountTypeFilter from "@/accounts/AccountTypeFilter";
import AccountingPeriodRangeFilter from "@/accounting-periods/AccountingPeriodRangeFilter";
import type { JSX } from "react";
import PageFilterFrame from "@/framework/view/PageFilterFrame";
import ToggleButtonSelector from "@/framework/forms/ToggleButtonSelector";
import { accountTrendsParamNames } from "@/accounts/trends/helpers";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";
import { useSearchParams } from "next/navigation";

/**
 * Props for the AccountTrendsFilter component.
 */
interface AccountTrendsFilterProps {
  readonly accountingPeriods: readonly AccountingPeriod[];
  readonly availableAccountNames: readonly string[];
  readonly defaultAccountingPeriodId: string | null;
  readonly defaultStartDate: string;
  readonly defaultEndDate: string;
  readonly disabled?: boolean;
}

/**
 * Renders the trends filter card for the Accounts view.
 */
const AccountTrendsFilter = function ({
  accountingPeriods,
  availableAccountNames,
  defaultAccountingPeriodId,
  defaultStartDate,
  defaultEndDate,
  disabled = false,
}: AccountTrendsFilterProps): JSX.Element {
  const searchParams = useSearchParams();

  const {
    page: pageParamName,
    balanceEventPage: balanceEventPageParamName,
    mode: modeParamName,
    accountType: accountTypeParamName,
    accountName: accountNameParamName,
    startAccountingPeriodId: startAccountingPeriodIdParamName,
    endAccountingPeriodId: endAccountingPeriodIdParamName,
    startDate: startDateParamName,
    endDate: endDateParamName,
  } = accountTrendsParamNames;

  const currentMode: TrendRangeMode =
    searchParams.get(modeParamName) === "accounting-period"
      ? "accounting-period"
      : "date";
  const currentAccountTypes = normalizeAccountTypes(
    searchParams.getAll(accountTypeParamName),
  );
  const currentAccountNames = normalizeAccountNames(
    searchParams.getAll(accountNameParamName),
    availableAccountNames,
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

  const updateParams = useSearchParamUpdater([
    pageParamName,
    balanceEventPageParamName,
  ]);

  const hasActiveView =
    currentMode !== "date" ||
    shouldPersistAccountTypes(currentAccountTypes) ||
    shouldPersistAccountNames(currentAccountNames) ||
    currentStartAccountingPeriodId !== (defaultAccountingPeriodId ?? "") ||
    currentEndAccountingPeriodId !== (defaultAccountingPeriodId ?? "") ||
    currentStartDate !== defaultStartDate ||
    currentEndDate !== defaultEndDate;

  const handleAccountTypeChange = function (
    nextAccountTypes: readonly AccountType[],
  ): void {
    updateParams((params) => {
      params.delete(accountTypeParamName);
      params.delete(accountNameParamName);
      if (shouldPersistAccountTypes(nextAccountTypes)) {
        nextAccountTypes.forEach((accountType) => {
          params.append(accountTypeParamName, accountType);
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

  const handleModeChange = function (nextMode: TrendRangeMode): void {
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
    event: React.ChangeEvent<HTMLInputElement>,
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
    event: React.ChangeEvent<HTMLInputElement>,
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
      params.delete(accountTypeParamName);
      params.delete(accountNameParamName);
      params.set(modeParamName, "date");
      params.set(startDateParamName, defaultStartDate);
      params.set(endDateParamName, defaultEndDate);
      params.delete(startAccountingPeriodIdParamName);
      params.delete(endAccountingPeriodIdParamName);
    });
  };

  const sharedFieldSx = {
    minWidth: { xs: "100%", sm: 180 },
  };

  return (
    <PageFilterFrame title="Account Trends">
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
      <AccountTypeFilter
        value={currentAccountTypes}
        onChange={handleAccountTypeChange}
        disabled={disabled}
      />
      <AccountNameFilter
        availableAccountNames={availableAccountNames}
        value={currentAccountNames}
        onChange={handleAccountNameChange}
        disabled={disabled}
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

export default AccountTrendsFilter;
