"use client";

import {
  accountTrendsParamNames,
  clearAccountTrendsFilters,
} from "@/accounts/trends/helpers";
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
import type { AccountingPeriod } from "@/accounting-periods/types";
import type { JSX } from "react";
import TrendFilterHeader from "@/framework/view/TrendFilterHeader";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";
import { useSearchParams } from "next/navigation";

/**
 * Props for the AccountTrendsFilter component.
 */
interface AccountTrendsFilterProps {
  readonly accountingPeriods: readonly AccountingPeriod[];
  readonly availableAccountNames: readonly string[];
  readonly returnUrl: string | undefined;
}

/**
 * Renders the header and filters for the account trends page.
 */
const AccountTrendsFilter = function ({
  accountingPeriods,
  availableAccountNames,
  returnUrl,
}: AccountTrendsFilterProps): JSX.Element {
  const searchParams = useSearchParams();
  const { page, accountType, accountName } = accountTrendsParamNames;
  const currentAccountTypes = normalizeAccountTypes(
    searchParams.getAll(accountType),
  );
  const currentAccountNames = normalizeAccountNames(
    searchParams.getAll(accountName),
    availableAccountNames,
  );
  const updateParams = useSearchParamUpdater([page]);

  const handleAccountTypeChange = function (
    nextAccountTypes: readonly AccountType[],
  ): void {
    updateParams((params) => {
      params.delete(accountType);
      params.delete(accountName);
      if (shouldPersistAccountTypes(nextAccountTypes)) {
        nextAccountTypes.forEach((type) => {
          params.append(accountType, type);
        });
      }
    });
  };

  const handleAccountNameChange = function (
    nextAccountNames: readonly string[],
  ): void {
    updateParams((params) => {
      params.delete(accountName);
      if (shouldPersistAccountNames(nextAccountNames)) {
        nextAccountNames.forEach((name) => {
          params.append(accountName, name);
        });
      }
    });
  };

  return (
    <TrendFilterHeader
      title="Account Trends"
      returnUrl={returnUrl}
      accountingPeriods={accountingPeriods}
      hasActiveFilters={
        shouldPersistAccountTypes(currentAccountTypes) ||
        shouldPersistAccountNames(currentAccountNames)
      }
      clearFilters={clearAccountTrendsFilters}
    >
      <AccountTypeFilter
        value={currentAccountTypes}
        onChange={handleAccountTypeChange}
      />
      <AccountNameFilter
        availableAccountNames={availableAccountNames}
        value={currentAccountNames}
        onChange={handleAccountNameChange}
      />
    </TrendFilterHeader>
  );
};

export default AccountTrendsFilter;
