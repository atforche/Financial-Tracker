"use client";

import { Button, Stack } from "@mui/material";
import type { Account } from "@/accounts/types";
import type { AccountGoalWorkspaceSearchParams } from "@/account-goals/workspace/types";
import type { AccountingPeriod } from "@/accounting-periods/types";
import AccountingPeriodEntryField from "@/accounting-periods/AccountingPeriodEntryField";
import type { JSX } from "react";
import MultiSelectAutocompleteFilter from "@/framework/forms/MultiSelectAutocompleteFilter";
import PageFilterFrame from "@/framework/view/PageFilterFrame";
import propertyName from "@/framework/data/propertyName";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";
import { useSearchParams } from "next/navigation";

interface AccountGoalWorkspaceFilterProps {
  readonly accountingPeriods: readonly AccountingPeriod[];
  readonly accounts: readonly Account[];
  readonly selectedAccountingPeriodId: string | null;
}

/**
 * Filters the Account Goal workspace by accounting period and account.
 */
const AccountGoalWorkspaceFilter = function ({
  accountingPeriods,
  accounts,
  selectedAccountingPeriodId,
}: AccountGoalWorkspaceFilterProps): JSX.Element {
  const searchParams = useSearchParams();
  const updateParams = useSearchParamUpdater([]);
  const periodParamName =
    propertyName<AccountGoalWorkspaceSearchParams>("accountingPeriodId");
  const accountIdsParamName =
    propertyName<AccountGoalWorkspaceSearchParams>("accountIds");
  const selectedPeriod =
    accountingPeriods.find(
      (period) => period.id === selectedAccountingPeriodId,
    ) ?? null;
  const selectedAccountIds = searchParams.getAll(accountIdsParamName);
  const selectedAccounts = accounts.filter((account) =>
    selectedAccountIds.includes(account.id),
  );

  return (
    <PageFilterFrame
      title="Account Goals Workspace"
      actions={
        <Button
          variant="outlined"
          onClick={() => {
            updateParams((params) => {
              params.delete(periodParamName);
              params.delete(accountIdsParamName);
            });
          }}
          disabled={
            !searchParams.has(periodParamName) &&
            selectedAccountIds.length === 0
          }
        >
          Reset Filters
        </Button>
      }
    >
      <Stack sx={{ minWidth: { xs: "100%", sm: 260 }, flex: 1 }}>
        <AccountingPeriodEntryField
          label="Accounting period"
          size="small"
          options={[...accountingPeriods]}
          value={selectedPeriod}
          setValue={(period) => {
            updateParams((params) => {
              if (period === null) {
                params.delete(periodParamName);
              } else {
                params.set(periodParamName, period.id);
              }
            });
          }}
        />
      </Stack>
      <MultiSelectAutocompleteFilter
        label="Accounts"
        options={accounts}
        value={selectedAccounts}
        placeholder="All accounts"
        noOptionsText="No accounts found"
        getOptionLabel={(account) => account.name}
        isOptionEqualToValue={(option, value) => option.id === value.id}
        onChange={(nextAccounts) => {
          updateParams((params) => {
            params.delete(accountIdsParamName);
            nextAccounts.forEach((account) => {
              params.append(accountIdsParamName, account.id);
            });
          });
        }}
      />
    </PageFilterFrame>
  );
};

export default AccountGoalWorkspaceFilter;
