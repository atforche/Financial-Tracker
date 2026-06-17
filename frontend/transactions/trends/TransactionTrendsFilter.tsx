"use client";

import {
  Autocomplete,
  Button,
  Checkbox,
  Paper,
  Stack,
  TextField,
  ToggleButton,
  ToggleButtonGroup,
  Typography,
} from "@mui/material";
import type { ChangeEvent, JSX, MouseEvent } from "react";
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
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import AccountTrendsAccountingPeriodFilter from "@/accounts/trends/AccountTrendsAccountingPeriodFilter";
import type { AccountingPeriod } from "@/accounting-periods/types";
import type { TransactionType } from "@/transactions/types";

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
  const pathname = usePathname();
  const router = useRouter();

  const pageParamName = "page";
  const modeParamName = "mode";
  const transactionTypeParamName = "transactionType";
  const accountNameParamName = "accountName";
  const fundNameParamName = "fundName";
  const startAccountingPeriodIdParamName = "startAccountingPeriodId";
  const endAccountingPeriodIdParamName = "endAccountingPeriodId";
  const startDateParamName = "startDate";
  const endDateParamName = "endDate";

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

  const accountingPeriodIndexes = new Map(
    accountingPeriods.map((period, index) => [period.id, index]),
  );

  const updateParams = function (
    updater: (params: URLSearchParams) => void,
  ): void {
    const params = new URLSearchParams(searchParams.toString());
    updater(params);
    params.delete(pageParamName);
    const nextQuery = params.toString();
    router.replace(nextQuery === "" ? pathname : `${pathname}?${nextQuery}`, {
      scroll: false,
    });
  };

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
    _: MouseEvent<HTMLElement>,
    nextMode: TransactionTrendsFilterMode | null,
  ): void {
    if (nextMode === null) {
      return;
    }

    updateParams((params) => {
      params.set(modeParamName, nextMode);
      if (nextMode === "date") {
        params.delete(startAccountingPeriodIdParamName);
        params.delete(endAccountingPeriodIdParamName);
        params.set(
          startDateParamName,
          params.get(startDateParamName) ?? defaultStartDate,
        );
        params.set(
          endDateParamName,
          params.get(endDateParamName) ?? defaultEndDate,
        );
        return;
      }

      params.delete(startDateParamName);
      params.delete(endDateParamName);
      if (defaultAccountingPeriodId !== null) {
        params.set(
          startAccountingPeriodIdParamName,
          params.get(startAccountingPeriodIdParamName) ??
            defaultAccountingPeriodId,
        );
        params.set(
          endAccountingPeriodIdParamName,
          params.get(endAccountingPeriodIdParamName) ??
            defaultAccountingPeriodId,
        );
      }
    });
  };

  const handleStartAccountingPeriodChange = function (
    nextStartAccountingPeriodId: string,
  ): void {
    const nextStartIndex =
      accountingPeriodIndexes.get(nextStartAccountingPeriodId) ?? 0;
    const currentEndIndex =
      accountingPeriodIndexes.get(currentEndAccountingPeriodId) ??
      nextStartIndex;
    const nextEndAccountingPeriodId =
      nextStartIndex > currentEndIndex
        ? nextStartAccountingPeriodId
        : currentEndAccountingPeriodId;

    updateParams((params) => {
      params.set(startAccountingPeriodIdParamName, nextStartAccountingPeriodId);
      params.set(endAccountingPeriodIdParamName, nextEndAccountingPeriodId);
    });
  };

  const handleEndAccountingPeriodChange = function (
    nextEndAccountingPeriodId: string,
  ): void {
    const nextEndIndex =
      accountingPeriodIndexes.get(nextEndAccountingPeriodId) ?? 0;
    const currentStartIndex =
      accountingPeriodIndexes.get(currentStartAccountingPeriodId) ??
      nextEndIndex;
    const nextStartAccountingPeriodId =
      nextEndIndex < currentStartIndex
        ? nextEndAccountingPeriodId
        : currentStartAccountingPeriodId;

    updateParams((params) => {
      params.set(startAccountingPeriodIdParamName, nextStartAccountingPeriodId);
      params.set(endAccountingPeriodIdParamName, nextEndAccountingPeriodId);
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
          <Typography variant="h5">Transaction Trends</Typography>
        </Stack>
        <Stack
          direction="row"
          spacing={1.5}
          useFlexGap
          flexWrap="wrap"
          alignItems={{ xs: "stretch", md: "center" }}
        >
          <ToggleButtonGroup
            exclusive
            value={currentMode}
            size="small"
            disabled={disabled}
            onChange={handleModeChange}
            sx={{ flexShrink: 0 }}
          >
            <ToggleButton value="date">Dates</ToggleButton>
            <ToggleButton
              value="accounting-period"
              disabled={defaultAccountingPeriodId === null}
            >
              Accounting periods
            </ToggleButton>
          </ToggleButtonGroup>
          {currentMode === "accounting-period" ? (
            <>
              <AccountTrendsAccountingPeriodFilter
                accountingPeriods={accountingPeriods}
                label="Start period"
                value={currentStartAccountingPeriodId}
                onChange={handleStartAccountingPeriodChange}
                disabled={disabled}
              />
              <AccountTrendsAccountingPeriodFilter
                accountingPeriods={accountingPeriods}
                label="End period"
                value={currentEndAccountingPeriodId}
                onChange={handleEndAccountingPeriodChange}
                disabled={disabled}
              />
            </>
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
        </Stack>
      </Stack>
    </Paper>
  );
};

export default TransactionTrendsFilter;
