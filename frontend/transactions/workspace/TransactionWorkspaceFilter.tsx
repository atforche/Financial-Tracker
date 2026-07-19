"use client";

import {
  normalizeStringSearchParams,
  selectAvailableSearchParamValues,
} from "@/framework/routes/helpers";
import type { Account } from "@/accounts/types";
import type { AccountingPeriod } from "@/accounting-periods/types";
import { Button } from "@mui/material";
import type { Fund } from "@/funds/types";
import type { JSX } from "react";
import MultiSelectAutocompleteFilter from "@/framework/forms/MultiSelectAutocompleteFilter";
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

  return (
    <PageFilterFrame title="Transactions Workspace">
      <MultiSelectAutocompleteFilter
        label="Accounting periods"
        options={accountingPeriods}
        value={currentAccountingPeriods}
        disabled={accountingPeriods.length === 0}
        placeholder="All accounting periods"
        noOptionsText={
          accountingPeriods.length === 0
            ? "No accounting periods available"
            : "No accounting periods found"
        }
        isOptionEqualToValue={(option, value) => option.id === value.id}
        onChange={(nextAccountingPeriods) => {
          handleAccountingPeriodChange(nextAccountingPeriods);
        }}
        getOptionLabel={(accountingPeriod) => accountingPeriod.name}
      />
      <MultiSelectAutocompleteFilter
        label="Accounts"
        options={accounts}
        value={currentAccounts}
        disabled={accounts.length === 0}
        placeholder="All accounts"
        noOptionsText={
          accounts.length === 0 ? "No accounts available" : "No accounts found"
        }
        isOptionEqualToValue={(option, value) => option.id === value.id}
        onChange={(nextAccounts) => {
          handleAccountChange(nextAccounts);
        }}
        getOptionLabel={(account) => account.name}
      />
      <MultiSelectAutocompleteFilter
        label="Funds"
        options={funds}
        value={currentFunds}
        disabled={funds.length === 0}
        placeholder="All funds"
        noOptionsText={
          funds.length === 0 ? "No funds available" : "No funds found"
        }
        isOptionEqualToValue={(option, value) => option.id === value.id}
        onChange={(nextFunds) => {
          handleFundChange(nextFunds);
        }}
        getOptionLabel={(fund) => fund.name}
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
