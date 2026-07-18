"use client";

import { Autocomplete, Button, Checkbox, TextField } from "@mui/material";
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
import type { CurrentTransactionsSearchParams } from "@/transactions/current/CurrentTransactions";
import FundTrendsFundNameFilter from "@/funds/trends/FundTrendsFundNameFilter";
import type { JSX } from "react";
import PageFilterFrame from "@/framework/view/PageFilterFrame";
import type { TransactionType } from "@/transactions/types";
import nameof from "@/framework/data/nameof";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";
import { useSearchParams } from "next/navigation";

/**
 * Props for the CurrentTransactionsFilter component.
 */
interface CurrentTransactionsFilterProps {
  readonly availableAccountNames: readonly string[];
  readonly availableFundNames: readonly string[];
  readonly disabled?: boolean;
}

/**
 * Filter for the current transactions page.
 */
const CurrentTransactionsFilter = function ({
  availableAccountNames,
  availableFundNames,
  disabled = false,
}: CurrentTransactionsFilterProps): JSX.Element {
  const searchParams = useSearchParams();

  const unpostedTransactionPageParamName =
    nameof<CurrentTransactionsSearchParams>("unpostedTransactionPage");
  const postedTransactionPageParamName =
    nameof<CurrentTransactionsSearchParams>("postedTransactionPage");
  const transactionTypeParamName =
    nameof<CurrentTransactionsSearchParams>("transactionType");
  const accountNameParamName =
    nameof<CurrentTransactionsSearchParams>("accountName");
  const fundNameParamName = nameof<CurrentTransactionsSearchParams>("fundName");

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

  const updateParams = useSearchParamUpdater([
    unpostedTransactionPageParamName,
    postedTransactionPageParamName,
  ]);

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

  const clearView = function (): void {
    updateParams((params) => {
      params.delete(transactionTypeParamName);
      params.delete(accountNameParamName);
      params.delete(fundNameParamName);
    });
  };

  const hasActiveView =
    shouldPersistTransactionTypes(currentTransactionTypes) ||
    shouldPersistAccountNames(currentAccountNames) ||
    shouldPersistFundNames(currentFundNames);

  return (
    <PageFilterFrame
      title="Current Transactions"
      description="Filter the live transaction snapshot by transaction type, account name, and fund name."
    >
      <Autocomplete
        multiple
        disableCloseOnSelect
        size="small"
        options={[...transactionTypeValues]}
        value={[...currentTransactionTypes]}
        disabled={disabled}
        limitTags={1}
        sx={{ minWidth: { xs: "100%", sm: 280 }, flex: { md: 1 } }}
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
      <AccountNameFilter
        availableAccountNames={availableAccountNames}
        value={currentAccountNames}
        onChange={handleAccountNameChange}
        disabled={disabled}
      />
      <FundTrendsFundNameFilter
        availableFundNames={availableFundNames}
        value={currentFundNames}
        onChange={handleFundNameChange}
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

export default CurrentTransactionsFilter;
