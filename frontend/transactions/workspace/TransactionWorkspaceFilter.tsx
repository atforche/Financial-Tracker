"use client";

import type { Account, AccountType } from "@/accounts/types";
import { type JSX, useEffect, useState } from "react";
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

  const initialAccountingPeriods = selectAvailableSearchParamValues(
    normalizeStringSearchParams(searchParams.getAll(accountingPeriodParamName)),
    accountingPeriods,
    (value) => value,
    (value) => value.id,
  );
  const initialAccounts = selectAvailableSearchParamValues(
    selectedAccountIds,
    accounts,
    (value) => value,
    (value) => value.id,
  );
  const initialFunds = selectAvailableSearchParamValues(
    selectedFundIds,
    funds,
    (value) => value,
    (value) => value.id,
  );
  const initialLocations = selectAvailableSearchParamValues(
    normalizeStringSearchParams(searchParams.getAll(locationParamName)),
    locations,
    (value) => value,
    (value) => value.id,
  );
  const initialAccountTypes = normalizeAccountTypes(
    searchParams.getAll(accountTypeParamName),
  );
  const initialTransactionTypes = normalizeTransactionTypes(
    searchParams.getAll(transactionTypeParamName),
  );
  const initialStartDate = searchParams.get(startDateParamName) ?? "";
  const initialEndDate = searchParams.get(endDateParamName) ?? "";

  const [currentAccountingPeriods, setCurrentAccountingPeriods] = useState<
    readonly AccountingPeriod[]
  >(initialAccountingPeriods);
  const [currentAccounts, setCurrentAccounts] =
    useState<readonly Account[]>(initialAccounts);
  const [currentFunds, setCurrentFunds] =
    useState<readonly Fund[]>(initialFunds);
  const [currentLocations, setCurrentLocations] =
    useState<readonly Location[]>(initialLocations);
  const [currentAccountTypes, setCurrentAccountTypes] =
    useState(initialAccountTypes);
  const [currentTransactionTypes, setCurrentTransactionTypes] = useState(
    initialTransactionTypes,
  );
  const [currentStartDate, setCurrentStartDate] = useState(initialStartDate);
  const [currentEndDate, setCurrentEndDate] = useState(initialEndDate);

  const currentSearchParams = searchParams.toString();
  useEffect(() => {
    const params = new URLSearchParams(currentSearchParams);
    setCurrentAccountingPeriods(
      selectAvailableSearchParamValues(
        normalizeStringSearchParams(params.getAll(accountingPeriodParamName)),
        accountingPeriods,
        (value) => value,
        (value) => value.id,
      ),
    );
    setCurrentAccounts(
      selectAvailableSearchParamValues(
        selectedAccountIds,
        accounts,
        (value) => value,
        (value) => value.id,
      ),
    );
    setCurrentFunds(
      selectAvailableSearchParamValues(
        selectedFundIds,
        funds,
        (value) => value,
        (value) => value.id,
      ),
    );
    setCurrentLocations(
      selectAvailableSearchParamValues(
        normalizeStringSearchParams(params.getAll(locationParamName)),
        locations,
        (value) => value,
        (value) => value.id,
      ),
    );
    setCurrentAccountTypes(
      normalizeAccountTypes(params.getAll(accountTypeParamName)),
    );
    setCurrentTransactionTypes(
      normalizeTransactionTypes(params.getAll(transactionTypeParamName)),
    );
    setCurrentStartDate(params.get(startDateParamName) ?? "");
    setCurrentEndDate(params.get(endDateParamName) ?? "");
  }, [
    accountingPeriodParamName,
    accountingPeriods,
    accountTypeParamName,
    accounts,
    currentSearchParams,
    fundParamName,
    funds,
    locationParamName,
    locations,
    selectedAccountIds,
    selectedFundIds,
    transactionTypeParamName,
    startDateParamName,
    endDateParamName,
  ]);

  const updateParams = useSearchParamUpdater([pageParamName]);

  const submitSearch = function (): void {
    updateParams((params) => {
      [
        accountingPeriodParamName,
        accountParamName,
        fundParamName,
        locationParamName,
        fundNameParamName,
        accountTypeParamName,
        accountNameParamName,
        transactionTypeParamName,
        startDateParamName,
        endDateParamName,
        startAccountingPeriodIdParamName,
        endAccountingPeriodIdParamName,
      ].forEach((paramName) => {
        params.delete(paramName);
      });
      currentAccountingPeriods.forEach((accountingPeriod) => {
        params.append(accountingPeriodParamName, accountingPeriod.id);
      });
      currentAccounts.forEach((account) => {
        params.append(accountParamName, account.id);
      });
      currentFunds.forEach((fund) => {
        params.append(fundParamName, fund.id);
      });
      currentLocations.forEach((location) => {
        params.append(locationParamName, location.id);
      });
      if (shouldPersistAccountTypes(currentAccountTypes)) {
        currentAccountTypes.forEach((accountType) => {
          params.append(accountTypeParamName, accountType);
        });
      }
      if (shouldPersistTransactionTypes(currentTransactionTypes)) {
        currentTransactionTypes.forEach((transactionType) => {
          params.append(transactionTypeParamName, transactionType);
        });
      }
      if (currentStartDate !== "") {
        params.set(startDateParamName, currentStartDate);
      }
      if (currentEndDate !== "") {
        params.set(endDateParamName, currentEndDate);
      }
      params.delete(pageParamName);
    });
  };

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
  const hasAppliedFilters = [
    accountingPeriodParamName,
    accountParamName,
    fundParamName,
    locationParamName,
    fundNameParamName,
    accountTypeParamName,
    accountNameParamName,
    transactionTypeParamName,
    startDateParamName,
    endDateParamName,
    startAccountingPeriodIdParamName,
    endAccountingPeriodIdParamName,
  ].some((paramName) => searchParams.has(paramName));

  const handleAccountingPeriodChange = function (
    nextAccountingPeriods: readonly AccountingPeriod[],
  ): void {
    setCurrentAccountingPeriods(nextAccountingPeriods);
  };

  const handleAccountChange = function (
    nextAccounts: readonly Account[],
  ): void {
    setCurrentAccounts(nextAccounts);
    setCurrentAccountTypes([]);
  };

  const handleFundChange = function (nextFunds: readonly Fund[]): void {
    setCurrentFunds(nextFunds);
  };

  const handleLocationChange = function (
    nextLocations: readonly Location[],
  ): void {
    setCurrentLocations(nextLocations);
  };

  const handleAccountTypeChange = function (
    nextAccountTypes: readonly AccountType[],
  ): void {
    setCurrentAccountTypes(nextAccountTypes);
  };

  const handleTransactionTypeChange = function (
    nextTransactionTypes: readonly TransactionType[],
  ): void {
    setCurrentTransactionTypes(nextTransactionTypes);
  };

  const handleDateRangeChange = function ({
    start,
    end,
  }: {
    start: string;
    end: string;
  }): void {
    setCurrentStartDate(start);
    setCurrentEndDate(end);
  };

  const clearView = function (): void {
    setCurrentAccountingPeriods([]);
    setCurrentAccounts([]);
    setCurrentFunds([]);
    setCurrentLocations([]);
    setCurrentAccountTypes([]);
    setCurrentTransactionTypes([]);
    setCurrentStartDate("");
    setCurrentEndDate("");
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
    <PageFilterFrame title="Transactions" mobileSticky={false}>
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
          variant="contained"
          onClick={submitSearch}
          disabled={!hasActiveView}
        >
          Search
        </Button>
        <Button
          variant="outlined"
          onClick={clearView}
          disabled={!(hasActiveView || hasAppliedFilters)}
        >
          Reset filters
        </Button>
      </TransactionFilterControl>
    </PageFilterFrame>
  );
};

export default TransactionWorkspaceFilter;
