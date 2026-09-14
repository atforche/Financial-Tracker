"use client";

import type {
  AccountingPeriod,
  AccountingPeriodRange,
} from "@/accounting-periods/types";
import { Button, MenuItem, TextField } from "@mui/material";
import AccountingPeriodRangeFilter from "@/accounting-periods/AccountingPeriodRangeFilter";
import type { JSX } from "react";
import PageFilterFrame from "@/framework/view/PageFilterFrame";
import { accountGoalTrendsParamNames } from "@/account-goals/trends/helpers";
import { getDefaultTrendAccountingPeriodRange } from "@/framework/routes/trendRange";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";
import { useSearchParams } from "next/navigation";

interface AccountGoalTrendsFilterProps {
  readonly accountingPeriods: readonly AccountingPeriod[];
  readonly accounts: readonly { readonly id: string; readonly name: string }[];
}

/**
 * Renders the Account Goal trends filters.
 */
const AccountGoalTrendsFilter = function ({
  accountingPeriods,
  accounts,
}: AccountGoalTrendsFilterProps): JSX.Element {
  const defaultRange = getDefaultTrendAccountingPeriodRange(accountingPeriods);
  const defaultStartAccountingPeriodId = defaultRange?.start ?? null;
  const defaultEndAccountingPeriodId = defaultRange?.end ?? null;
  const searchParams = useSearchParams();
  const { accountId, startAccountingPeriodId, endAccountingPeriodId } =
    accountGoalTrendsParamNames;
  const selectedAccountId = accounts.some(
    (account) => account.id === searchParams.get(accountId),
  )
    ? (searchParams.get(accountId) ?? "")
    : "all";
  const start =
    searchParams.get(startAccountingPeriodId) ??
    defaultStartAccountingPeriodId ??
    "";
  const end =
    searchParams.get(endAccountingPeriodId) ??
    defaultEndAccountingPeriodId ??
    "";
  const updateParams = useSearchParamUpdater([]);
  const hasActiveView =
    selectedAccountId !== "all" ||
    start !== (defaultStartAccountingPeriodId ?? "") ||
    end !== (defaultEndAccountingPeriodId ?? "");

  return (
    <PageFilterFrame title="Account Goal Trends">
      <AccountingPeriodRangeFilter
        accountingPeriods={accountingPeriods}
        startValue={start}
        endValue={end}
        onChange={(range: AccountingPeriodRange) => {
          updateParams((params) => {
            params.set(startAccountingPeriodId, range.start);
            params.set(endAccountingPeriodId, range.end);
          });
        }}
      />
      <TextField
        select
        label="View"
        size="small"
        value={selectedAccountId}
        onChange={(event) => {
          updateParams((params) => {
            params.delete(accountGoalTrendsParamNames.accountName);
            if (event.target.value === "all") {
              params.delete(accountId);
            } else {
              params.set(accountId, event.target.value);
            }
          });
        }}
        sx={{ minWidth: 220 }}
      >
        <MenuItem value="all">All Accounts</MenuItem>
        {accounts.map((account) => (
          <MenuItem key={account.id} value={account.id}>
            {account.name}
          </MenuItem>
        ))}
      </TextField>
      <Button
        variant="outlined"
        onClick={() => {
          updateParams((params) => {
            params.delete(accountId);
            params.delete(accountGoalTrendsParamNames.accountName);
            params.set(
              startAccountingPeriodId,
              defaultStartAccountingPeriodId ?? "",
            );
            params.set(
              endAccountingPeriodId,
              defaultEndAccountingPeriodId ?? "",
            );
          });
        }}
        disabled={!hasActiveView}
        sx={{ flexShrink: 0 }}
      >
        Reset filters
      </Button>
    </PageFilterFrame>
  );
};

export default AccountGoalTrendsFilter;
