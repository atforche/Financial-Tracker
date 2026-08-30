"use client";

import type {
  AccountingPeriod,
  AccountingPeriodRange,
} from "@/accounting-periods/types";
import {
  normalizeAccountNames,
  shouldPersistAccountNames,
} from "@/accounts/accountNameFilterHelpers";
import AccountNameFilter from "@/accounts/AccountNameFilter";
import AccountingPeriodRangeFilter from "@/accounting-periods/AccountingPeriodRangeFilter";
import { Button } from "@mui/material";
import type { JSX } from "react";
import PageFilterFrame from "@/framework/view/PageFilterFrame";
import { accountGoalTrendsParamNames } from "@/account-goals/trends/helpers";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";
import { useSearchParams } from "next/navigation";

interface AccountGoalTrendsFilterProps {
  readonly accountingPeriods: readonly AccountingPeriod[];
  readonly availableAccountNames: readonly string[];
  readonly defaultAccountingPeriodId: string | null;
  readonly disabled?: boolean;
}

/**
 * Renders the Account Goal trends filters.
 */
const AccountGoalTrendsFilter = function ({
  accountingPeriods,
  availableAccountNames,
  defaultAccountingPeriodId,
  disabled = false,
}: AccountGoalTrendsFilterProps): JSX.Element {
  const searchParams = useSearchParams();
  const { accountName, startAccountingPeriodId, endAccountingPeriodId } =
    accountGoalTrendsParamNames;
  const currentAccountNames = normalizeAccountNames(
    searchParams.getAll(accountName),
    availableAccountNames,
  );
  const start =
    searchParams.get(startAccountingPeriodId) ??
    defaultAccountingPeriodId ??
    "";
  const end =
    searchParams.get(endAccountingPeriodId) ?? defaultAccountingPeriodId ?? "";
  const updateParams = useSearchParamUpdater([]);
  const hasActiveView =
    shouldPersistAccountNames(currentAccountNames) ||
    start !== (defaultAccountingPeriodId ?? "") ||
    end !== (defaultAccountingPeriodId ?? "");

  return (
    <PageFilterFrame title="Account Goal Trends">
      <AccountingPeriodRangeFilter
        accountingPeriods={accountingPeriods}
        startValue={start}
        endValue={end}
        disabled={disabled}
        onChange={(range: AccountingPeriodRange) => {
          updateParams((params) => {
            params.set(startAccountingPeriodId, range.start);
            params.set(endAccountingPeriodId, range.end);
          });
        }}
      />
      <AccountNameFilter
        availableAccountNames={availableAccountNames}
        value={currentAccountNames}
        disabled={disabled}
        onChange={(names) => {
          updateParams((params) => {
            params.delete(accountName);
            if (shouldPersistAccountNames(names)) {
              names.forEach((name) => {
                params.append(accountName, name);
              });
            }
          });
        }}
      />
      <Button
        variant="outlined"
        onClick={() => {
          updateParams((params) => {
            params.delete(accountName);
            params.set(
              startAccountingPeriodId,
              defaultAccountingPeriodId ?? "",
            );
            params.set(endAccountingPeriodId, defaultAccountingPeriodId ?? "");
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
