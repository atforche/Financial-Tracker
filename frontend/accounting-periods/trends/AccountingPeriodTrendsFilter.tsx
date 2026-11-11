"use client";

import type { AccountingPeriod } from "@/accounting-periods/types";
import AccountingPeriodRangeFilter from "@/accounting-periods/AccountingPeriodRangeFilter";
import type { AccountingPeriodTrendsSearchParams } from "@/accounting-periods/trends/AccountingPeriodTrends";
import type { JSX } from "react";
import ResetFiltersButton from "@/framework/view/ResetFiltersButton";
import WorkspaceHeader from "@/framework/view/WorkspaceHeader";
import { getDefaultTrendAccountingPeriodRange } from "@/framework/routes/trendRange";
import propertyName from "@/framework/data/propertyName";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";
import { useSearchParams } from "next/navigation";

/**
 * Props for the AccountingPeriodTrendsFilter component.
 */
interface AccountingPeriodTrendsFilterProps {
  readonly accountingPeriods: readonly AccountingPeriod[];
  readonly disabled?: boolean;
}

/**
 * Renders the trends filter card for the Accounting Periods view.
 */
const AccountingPeriodTrendsFilter = function ({
  accountingPeriods,
  disabled = false,
}: AccountingPeriodTrendsFilterProps): JSX.Element {
  const defaultRange = getDefaultTrendAccountingPeriodRange(accountingPeriods);
  const defaultStartAccountingPeriodId = defaultRange?.start ?? null;
  const defaultEndAccountingPeriodId = defaultRange?.end ?? null;
  const searchParams = useSearchParams();

  const startAccountingPeriodIdParamName =
    propertyName<AccountingPeriodTrendsSearchParams>("startAccountingPeriodId");
  const endAccountingPeriodIdParamName =
    propertyName<AccountingPeriodTrendsSearchParams>("endAccountingPeriodId");

  const currentStartAccountingPeriodId =
    searchParams.get(startAccountingPeriodIdParamName) ??
    defaultStartAccountingPeriodId ??
    "";
  const currentEndAccountingPeriodId =
    searchParams.get(endAccountingPeriodIdParamName) ??
    defaultEndAccountingPeriodId ??
    "";

  const updateParams = useSearchParamUpdater([]);

  const hasActiveView =
    currentStartAccountingPeriodId !== (defaultStartAccountingPeriodId ?? "") ||
    currentEndAccountingPeriodId !== (defaultEndAccountingPeriodId ?? "");

  const handleAccountingPeriodRangeChange = function (range: {
    readonly start: string;
    readonly end: string;
  }): void {
    updateParams((params) => {
      params.set(startAccountingPeriodIdParamName, range.start);
      params.set(endAccountingPeriodIdParamName, range.end);
    });
  };

  const clearView = function (): void {
    updateParams((params) => {
      if (
        defaultStartAccountingPeriodId !== null &&
        defaultEndAccountingPeriodId !== null
      ) {
        params.set(
          startAccountingPeriodIdParamName,
          defaultStartAccountingPeriodId,
        );
        params.set(
          endAccountingPeriodIdParamName,
          defaultEndAccountingPeriodId,
        );
      } else {
        params.delete(startAccountingPeriodIdParamName);
        params.delete(endAccountingPeriodIdParamName);
      }
    });
  };

  return (
    <WorkspaceHeader title="Accounting Period Trends" action={null}>
      <AccountingPeriodRangeFilter
        accountingPeriods={accountingPeriods}
        startValue={currentStartAccountingPeriodId}
        endValue={currentEndAccountingPeriodId}
        onChange={handleAccountingPeriodRangeChange}
        disabled={disabled}
      />
      <ResetFiltersButton active={hasActiveView} onClick={clearView} />
    </WorkspaceHeader>
  );
};

export default AccountingPeriodTrendsFilter;
