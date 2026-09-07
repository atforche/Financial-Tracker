"use client";

import type { AccountingPeriod } from "@/accounting-periods/types";
import AccountingPeriodRangeFilter from "@/accounting-periods/AccountingPeriodRangeFilter";
import type { AccountingPeriodTrendsSearchParams } from "@/accounting-periods/trends/AccountingPeriodTrends";
import { Button } from "@mui/material";
import type { JSX } from "react";
import PageFilterFrame from "@/framework/view/PageFilterFrame";
import propertyName from "@/framework/data/propertyName";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";
import { useSearchParams } from "next/navigation";

/**
 * Props for the AccountingPeriodTrendsFilter component.
 */
interface AccountingPeriodTrendsFilterProps {
  readonly accountingPeriods: readonly AccountingPeriod[];
  readonly defaultStartAccountingPeriodId: string | null;
  readonly defaultEndAccountingPeriodId: string | null;
  readonly disabled?: boolean;
}

/**
 * Renders the trends filter card for the Accounting Periods view.
 */
const AccountingPeriodTrendsFilter = function ({
  accountingPeriods,
  defaultStartAccountingPeriodId,
  defaultEndAccountingPeriodId,
  disabled = false,
}: AccountingPeriodTrendsFilterProps): JSX.Element {
  const searchParams = useSearchParams();

  const pageParamName =
    propertyName<AccountingPeriodTrendsSearchParams>("page");
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

  const updateParams = useSearchParamUpdater([pageParamName]);

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
    <PageFilterFrame
      title="Accounting Period Trends"
      actions={
        <Button
          variant="outlined"
          onClick={clearView}
          disabled={!hasActiveView}
        >
          Reset filters
        </Button>
      }
    >
      <AccountingPeriodRangeFilter
        accountingPeriods={accountingPeriods}
        startValue={currentStartAccountingPeriodId}
        endValue={currentEndAccountingPeriodId}
        onChange={handleAccountingPeriodRangeChange}
        disabled={disabled}
      />
    </PageFilterFrame>
  );
};

export default AccountingPeriodTrendsFilter;
