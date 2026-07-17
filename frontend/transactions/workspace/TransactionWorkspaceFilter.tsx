"use client";

import { Autocomplete, Button, Checkbox, TextField } from "@mui/material";
import {
  normalizeStringSearchParams,
  selectAvailableSearchParamValues,
} from "@/framework/routes/helpers";
import type { Account } from "@/accounts/types";
import type { AccountingPeriod } from "@/accounting-periods/types";
import type { Fund } from "@/funds/types";
import type { JSX } from "react";
import PageFilterFrame from "@/framework/view/PageFilterFrame";
import type { TransactionWorkspaceSearchParams } from "@/transactions/workspace/TransactionWorkspace";
import nameof from "@/framework/data/nameof";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";
import { useSearchParams } from "next/navigation";

/**
 * Props for the TransactionWorkspaceFilter component.
 */
interface TransactionWorkspaceFilterProps {
  readonly accountingPeriods: readonly AccountingPeriod[];
  readonly accounts: readonly Account[];
  readonly funds: readonly Fund[];
}

/**
 * Renders the filter card for the Transaction workspace with accounting period, account, and fund filters.
 */
const TransactionWorkspaceFilter = function ({
  accountingPeriods,
  accounts,
  funds,
}: TransactionWorkspaceFilterProps): JSX.Element {
  const searchParams = useSearchParams();

  const pageParamName = nameof<TransactionWorkspaceSearchParams>("page");
  const accountingPeriodParamName = nameof<TransactionWorkspaceSearchParams>(
    "accountingPeriodIds",
  );
  const accountParamName =
    nameof<TransactionWorkspaceSearchParams>("accountIds");
  const fundParamName = nameof<TransactionWorkspaceSearchParams>("fundIds");

  const currentAccountingPeriods = selectAvailableSearchParamValues(
    normalizeStringSearchParams(searchParams.getAll(accountingPeriodParamName)),
    accountingPeriods,
    (value) => value,
    (value) => value.id,
  );
  const currentAccounts = selectAvailableSearchParamValues(
    normalizeStringSearchParams(searchParams.getAll(accountParamName)),
    accounts,
    (value) => value,
    (value) => value.id,
  );
  const currentFunds = selectAvailableSearchParamValues(
    normalizeStringSearchParams(searchParams.getAll(fundParamName)),
    funds,
    (value) => value,
    (value) => value.id,
  );

  const updateParams = useSearchParamUpdater([pageParamName]);

  const hasActiveView =
    currentAccountingPeriods.length > 0 ||
    currentAccounts.length > 0 ||
    currentFunds.length > 0;

  const handleAccountingPeriodChange = function (
    nextAccountingPeriods: readonly AccountingPeriod[],
  ): void {
    updateParams((params) => {
      params.delete(accountingPeriodParamName);
      nextAccountingPeriods.forEach((accountingPeriod) => {
        params.append(accountingPeriodParamName, accountingPeriod.id);
      });
    });
  };

  const handleAccountChange = function (
    nextAccounts: readonly Account[],
  ): void {
    updateParams((params) => {
      params.delete(accountParamName);
      nextAccounts.forEach((account) => {
        params.append(accountParamName, account.id);
      });
    });
  };

  const handleFundChange = function (nextFunds: readonly Fund[]): void {
    updateParams((params) => {
      params.delete(fundParamName);
      nextFunds.forEach((fund) => {
        params.append(fundParamName, fund.id);
      });
    });
  };

  const clearView = function (): void {
    updateParams((params) => {
      params.delete(accountingPeriodParamName);
      params.delete(accountParamName);
      params.delete(fundParamName);
    });
  };

  const sharedAutocompleteSx = {
    minWidth: { xs: "100%", sm: 280 },
    flex: { md: 1 },
  };

  return (
    <PageFilterFrame title="Transactions Workspace">
      <Autocomplete
        multiple
        disableCloseOnSelect
        size="small"
        options={[...accountingPeriods]}
        value={[...currentAccountingPeriods]}
        disabled={accountingPeriods.length === 0}
        limitTags={1}
        sx={sharedAutocompleteSx}
        noOptionsText={
          accountingPeriods.length === 0
            ? "No accounting periods available"
            : "No accounting periods found"
        }
        isOptionEqualToValue={(option, value) => option.id === value.id}
        slotProps={{
          paper: {
            sx: {
              "& .MuiAutocomplete-listbox": {
                maxHeight: 320,
              },
            },
          },
        }}
        onChange={(_, nextAccountingPeriods) => {
          handleAccountingPeriodChange(nextAccountingPeriods);
        }}
        getOptionLabel={(accountingPeriod) => accountingPeriod.name}
        renderOption={(props, option, { selected }) => (
          <li {...props}>
            <Checkbox size="small" checked={selected} sx={{ mr: 1 }} />
            {option.name}
          </li>
        )}
        renderInput={(params) => (
          <TextField
            {...params}
            label="Accounting periods"
            {...(currentAccountingPeriods.length === 0
              ? { placeholder: "All accounting periods" }
              : {})}
          />
        )}
      />
      <Autocomplete
        multiple
        disableCloseOnSelect
        size="small"
        options={[...accounts]}
        value={[...currentAccounts]}
        disabled={accounts.length === 0}
        limitTags={1}
        sx={sharedAutocompleteSx}
        noOptionsText={
          accounts.length === 0 ? "No accounts available" : "No accounts found"
        }
        isOptionEqualToValue={(option, value) => option.id === value.id}
        slotProps={{
          paper: {
            sx: {
              "& .MuiAutocomplete-listbox": {
                maxHeight: 320,
              },
            },
          },
        }}
        onChange={(_, nextAccounts) => {
          handleAccountChange(nextAccounts);
        }}
        getOptionLabel={(account) => account.name}
        renderOption={(props, option, { selected }) => (
          <li {...props}>
            <Checkbox size="small" checked={selected} sx={{ mr: 1 }} />
            {option.name}
          </li>
        )}
        renderInput={(params) => (
          <TextField
            {...params}
            label="Accounts"
            {...(currentAccounts.length === 0
              ? { placeholder: "All accounts" }
              : {})}
          />
        )}
      />
      <Autocomplete
        multiple
        disableCloseOnSelect
        size="small"
        options={[...funds]}
        value={[...currentFunds]}
        disabled={funds.length === 0}
        limitTags={1}
        sx={sharedAutocompleteSx}
        noOptionsText={
          funds.length === 0 ? "No funds available" : "No funds found"
        }
        isOptionEqualToValue={(option, value) => option.id === value.id}
        slotProps={{
          paper: {
            sx: {
              "& .MuiAutocomplete-listbox": {
                maxHeight: 320,
              },
            },
          },
        }}
        onChange={(_, nextFunds) => {
          handleFundChange(nextFunds);
        }}
        getOptionLabel={(fund) => fund.name}
        renderOption={(props, option, { selected }) => (
          <li {...props}>
            <Checkbox size="small" checked={selected} sx={{ mr: 1 }} />
            {option.name}
          </li>
        )}
        renderInput={(params) => (
          <TextField
            {...params}
            label="Funds"
            {...(currentFunds.length === 0 ? { placeholder: "All funds" } : {})}
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

export default TransactionWorkspaceFilter;
