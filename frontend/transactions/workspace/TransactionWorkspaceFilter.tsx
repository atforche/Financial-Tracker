"use client";

import type { Account, AccountType } from "@/accounts/types";
import {
  normalizeAccountTypes,
  shouldPersistAccountTypes,
} from "@/accounts/accountTypeFilterHelpers";
import {
  normalizeStringSearchParams,
  selectAvailableSearchParamValues,
} from "@/framework/routes/helpers";
import {
  normalizeTransactionTypes,
  shouldPersistTransactionTypes,
  transactionTypeValues,
} from "@/transactions/transactionTypeFilter";
import AccountTypeFilter from "@/accounts/AccountTypeFilter";
import type { AccountingPeriod } from "@/accounting-periods/types";
import { Button } from "@mui/material";
import DateRangeFilter from "@/framework/forms/DateRangeFilter";
import type { Fund } from "@/funds/types";
import type { JSX } from "react";
import type { Location } from "@/locations/types";
import MultiSelectAutocompleteFilter from "@/framework/forms/MultiSelectAutocompleteFilter";
import PageFilterFrame from "@/framework/view/PageFilterFrame";
import TransactionFilterControl from "@/transactions/workspace/TransactionFilterControl";
import type { TransactionType } from "@/transactions/types";
import type { TransactionWorkspaceSearchParams } from "@/transactions/workspace/TransactionWorkspace";
import propertyName from "@/framework/data/propertyName";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";
import { useSearchParams } from "next/navigation";

/**
 * Props for the TransactionWorkspaceFilter component.
 */
interface TransactionWorkspaceFilterProps {
  readonly accountingPeriods: readonly AccountingPeriod[];
  readonly accounts: readonly Account[];
  readonly funds: readonly Fund[];
  readonly locations: readonly Location[];
  readonly selectedAccountIds: readonly string[];
  readonly selectedFundIds: readonly string[];
}

/**
 * Renders the filter card for the Transaction workspace with accounting period, account, and fund filters.
 */
const TransactionWorkspaceFilter = function ({
  accountingPeriods,
  accounts,
  funds,
  selectedAccountIds,
  selectedFundIds,
  locations,
}: TransactionWorkspaceFilterProps): JSX.Element {
  const searchParams = useSearchParams();

  const pageParamName = propertyName<TransactionWorkspaceSearchParams>("page");
  const accountingPeriodParamName =
    propertyName<TransactionWorkspaceSearchParams>("accountingPeriodIds");
  const accountParamName =
    propertyName<TransactionWorkspaceSearchParams>("accountIds");
  const fundParamName =
    propertyName<TransactionWorkspaceSearchParams>("fundIds");
  const fundNameParamName =
    propertyName<TransactionWorkspaceSearchParams>("fundNames");
  const locationParamName =
    propertyName<TransactionWorkspaceSearchParams>("locationIds");
  const accountTypeParamName =
    propertyName<TransactionWorkspaceSearchParams>("accountTypes");
  const accountNameParamName =
    propertyName<TransactionWorkspaceSearchParams>("accountNames");
  const transactionTypeParamName =
    propertyName<TransactionWorkspaceSearchParams>("transactionTypes");
  const startDateParamName =
    propertyName<TransactionWorkspaceSearchParams>("startDate");
  const endDateParamName =
    propertyName<TransactionWorkspaceSearchParams>("endDate");
  const startAccountingPeriodIdParamName =
    propertyName<TransactionWorkspaceSearchParams>("startAccountingPeriodId");
  const endAccountingPeriodIdParamName =
    propertyName<TransactionWorkspaceSearchParams>("endAccountingPeriodId");

  const currentAccountingPeriods = selectAvailableSearchParamValues(
    normalizeStringSearchParams(searchParams.getAll(accountingPeriodParamName)),
    accountingPeriods,
    (value) => value,
    (value) => value.id,
  );
  const currentAccounts = selectAvailableSearchParamValues(
    selectedAccountIds,
    accounts,
    (value) => value,
    (value) => value.id,
  );
  const currentFunds = selectAvailableSearchParamValues(
    selectedFundIds,
    funds,
    (value) => value,
    (value) => value.id,
  );
  const currentLocations = selectAvailableSearchParamValues(
    normalizeStringSearchParams(searchParams.getAll(locationParamName)),
    locations,
    (value) => value,
    (value) => value.id,
  );
  const currentAccountTypes = normalizeAccountTypes(
    searchParams.getAll(accountTypeParamName),
  );
  const currentTransactionTypes = normalizeTransactionTypes(
    searchParams.getAll(transactionTypeParamName),
  );
  const currentStartDate = searchParams.get(startDateParamName) ?? "";
  const currentEndDate = searchParams.get(endDateParamName) ?? "";

  const updateParams = useSearchParamUpdater([pageParamName]);

  const hasActiveView =
    currentAccountingPeriods.length > 0 ||
    currentAccounts.length > 0 ||
    currentFunds.length > 0 ||
    currentLocations.length > 0 ||
    shouldPersistAccountTypes(currentAccountTypes) ||
    shouldPersistTransactionTypes(currentTransactionTypes) ||
    currentStartDate !== "" ||
    currentEndDate !== "" ||
    searchParams.has(startAccountingPeriodIdParamName) ||
    searchParams.has(endAccountingPeriodIdParamName);

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
      params.delete(accountNameParamName);
      params.delete(accountTypeParamName);
      nextAccounts.forEach((account) => {
        params.append(accountParamName, account.id);
      });
    });
  };

  const handleFundChange = function (nextFunds: readonly Fund[]): void {
    updateParams((params) => {
      params.delete(fundParamName);
      params.delete(fundNameParamName);
      nextFunds.forEach((fund) => {
        params.append(fundParamName, fund.id);
      });
    });
  };

  const handleLocationChange = function (
    nextLocations: readonly Location[],
  ): void {
    updateParams((params) => {
      params.delete(locationParamName);
      nextLocations.forEach((location) => {
        params.append(locationParamName, location.id);
      });
    });
  };

  const handleAccountTypeChange = function (
    nextAccountTypes: readonly AccountType[],
  ): void {
    updateParams((params) => {
      params.delete(accountTypeParamName);
      if (shouldPersistAccountTypes(nextAccountTypes)) {
        nextAccountTypes.forEach((accountType) => {
          params.append(accountTypeParamName, accountType);
        });
      }
    });
  };

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

  const handleDateRangeChange = function ({
    start,
    end,
  }: {
    start: string;
    end: string;
  }): void {
    updateParams((params) => {
      params.delete(startAccountingPeriodIdParamName);
      params.delete(endAccountingPeriodIdParamName);
      if (start === "") {
        params.delete(startDateParamName);
      } else {
        params.set(startDateParamName, start);
      }
      if (end === "") {
        params.delete(endDateParamName);
      } else {
        params.set(endDateParamName, end);
      }
    });
  };

  const clearView = function (): void {
    updateParams((params) => {
      params.delete(accountingPeriodParamName);
      params.delete(accountParamName);
      params.delete(fundParamName);
      params.delete(locationParamName);
      params.delete(fundNameParamName);
      params.delete(accountTypeParamName);
      params.delete(accountNameParamName);
      params.delete(transactionTypeParamName);
      params.delete(startDateParamName);
      params.delete(endDateParamName);
      params.delete(startAccountingPeriodIdParamName);
      params.delete(endAccountingPeriodIdParamName);
    });
  };

  return (
    <PageFilterFrame title="Transactions">
      <TransactionFilterControl>
        <DateRangeFilter
          value={{ start: currentStartDate, end: currentEndDate }}
          onChange={handleDateRangeChange}
        />
      </TransactionFilterControl>
      <TransactionFilterControl>
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
          onChange={handleAccountingPeriodChange}
          getOptionLabel={(accountingPeriod) => accountingPeriod.name}
        />
      </TransactionFilterControl>
      <TransactionFilterControl>
        <MultiSelectAutocompleteFilter
          label="Transaction types"
          options={transactionTypeValues}
          value={currentTransactionTypes}
          placeholder="All transaction types"
          noOptionsText="No transaction types found"
          onChange={handleTransactionTypeChange}
        />
      </TransactionFilterControl>
      <TransactionFilterControl>
        <AccountTypeFilter
          value={currentAccountTypes}
          onChange={handleAccountTypeChange}
        />
      </TransactionFilterControl>
      <TransactionFilterControl>
        <MultiSelectAutocompleteFilter
          label="Accounts"
          options={accounts}
          value={currentAccounts}
          disabled={accounts.length === 0}
          placeholder="All accounts"
          noOptionsText={
            accounts.length === 0
              ? "No accounts available"
              : "No accounts found"
          }
          isOptionEqualToValue={(option, value) => option.id === value.id}
          onChange={handleAccountChange}
          getOptionLabel={(account) => account.name}
        />
      </TransactionFilterControl>
      <TransactionFilterControl>
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
          onChange={handleFundChange}
          getOptionLabel={(fund) => fund.name}
        />
      </TransactionFilterControl>
      <TransactionFilterControl>
        <MultiSelectAutocompleteFilter
          label="Locations"
          options={locations}
          value={currentLocations}
          disabled={locations.length === 0}
          placeholder="All locations"
          noOptionsText={
            locations.length === 0
              ? "No locations available"
              : "No locations found"
          }
          isOptionEqualToValue={(option, value) => option.id === value.id}
          onChange={handleLocationChange}
          getOptionLabel={(location) => location.name}
        />
      </TransactionFilterControl>
      <TransactionFilterControl expand={false}>
        <Button
          variant="outlined"
          onClick={clearView}
          disabled={!hasActiveView}
        >
          Reset filters
        </Button>
      </TransactionFilterControl>
    </PageFilterFrame>
  );
};

export default TransactionWorkspaceFilter;
