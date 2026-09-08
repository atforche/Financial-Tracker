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
import { getDefaultTrendAccountingPeriodRange } from "@/framework/routes/trendRange";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";
import { useSearchParams } from "next/navigation";

interface AccountGoalTrendsFilterProps {
  readonly accountingPeriods: readonly AccountingPeriod[];
  readonly availableAccountNames: readonly string[];
}

/**
 * Renders the Account Goal trends filters.
 */
const AccountGoalTrendsFilter = function ({
  accountingPeriods,
  availableAccountNames,
}: AccountGoalTrendsFilterProps): JSX.Element {
  const defaultRange = getDefaultTrendAccountingPeriodRange(accountingPeriods);
  const defaultStartAccountingPeriodId = defaultRange?.start ?? null;
  const defaultEndAccountingPeriodId = defaultRange?.end ?? null;
  const searchParams = useSearchParams();
  const { accountName, startAccountingPeriodId, endAccountingPeriodId } =
    accountGoalTrendsParamNames;
  const currentAccountNames = normalizeAccountNames(
    searchParams.getAll(accountName),
    availableAccountNames,
  );
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
    shouldPersistAccountNames(currentAccountNames) ||
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
      <AccountNameFilter
        availableAccountNames={availableAccountNames}
        value={currentAccountNames}
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
