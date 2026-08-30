"use client";

import type {
  AccountingPeriod,
  AccountingPeriodRange,
} from "@/accounting-periods/types";
import {
  normalizeFundNames,
  shouldPersistFundNames,
} from "@/funds/trends/fundNameFilter";
import { useRouter, useSearchParams } from "next/navigation";
import AccountingPeriodRangeFilter from "@/accounting-periods/AccountingPeriodRangeFilter";
import { Button } from "@mui/material";
import FundTrendsFundNameFilter from "@/funds/trends/FundTrendsFundNameFilter";
import type { JSX } from "react";
import PageFilterFrame from "@/framework/view/PageFilterFrame";
import type { Route } from "next";
import { fundGoalTrendsParamNames } from "@/fund-goals/trends/helpers";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";

/**
 * Props for the FundGoalTrendsFilter component.
 */
interface FundGoalTrendsFilterProps {
  readonly accountingPeriods: readonly AccountingPeriod[];
  readonly availableFundNames: readonly string[];
  readonly defaultAccountingPeriodId: string | null;
  readonly transactionWorkspaceHref: Route | null;
  readonly disabled?: boolean;
}

/**
 * Renders the trends filter card for the Fund Goals view.
 */
const FundGoalTrendsFilter = function ({
  accountingPeriods,
  availableFundNames,
  defaultAccountingPeriodId,
  transactionWorkspaceHref,
  disabled = false,
}: FundGoalTrendsFilterProps): JSX.Element {
  const router = useRouter();
  const searchParams = useSearchParams();

  const pageParamName = fundGoalTrendsParamNames.page;
  const balanceEventPageParamName = fundGoalTrendsParamNames.balanceEventPage;
  const fundNameParamName = fundGoalTrendsParamNames.fundName;
  const startAccountingPeriodIdParamName =
    fundGoalTrendsParamNames.startAccountingPeriodId;
  const endAccountingPeriodIdParamName =
    fundGoalTrendsParamNames.endAccountingPeriodId;

  const currentFundNames = normalizeFundNames(
    searchParams.getAll(fundNameParamName),
    availableFundNames,
  );
  const currentStartAccountingPeriodId =
    searchParams.get(startAccountingPeriodIdParamName) ??
    defaultAccountingPeriodId;
  const currentEndAccountingPeriodId =
    searchParams.get(endAccountingPeriodIdParamName) ??
    defaultAccountingPeriodId;

  const updateParams = useSearchParamUpdater([
    pageParamName,
    balanceEventPageParamName,
  ]);

  const hasActiveView =
    shouldPersistFundNames(currentFundNames) ||
    currentStartAccountingPeriodId !== defaultAccountingPeriodId ||
    currentEndAccountingPeriodId !== defaultAccountingPeriodId;

  const handleFundNameChange = function (
    nextFundNames: readonly string[],
  ): void {
    updateParams((params) => {
      params.delete(fundNameParamName);
      if (shouldPersistFundNames(nextFundNames)) {
        nextFundNames.forEach((fundName) => {
          params.append(fundNameParamName, fundName);
        });
      }
    });
  };

  const handleAccountingPeriodRangeChange = function (
    range: AccountingPeriodRange,
  ): void {
    updateParams((params) => {
      params.set(startAccountingPeriodIdParamName, range.start);
      params.set(endAccountingPeriodIdParamName, range.end);
    });
  };

  const clearView = function (): void {
    updateParams((params) => {
      params.delete(fundNameParamName);
      params.set(
        startAccountingPeriodIdParamName,
        defaultAccountingPeriodId ?? "",
      );
      params.set(
        endAccountingPeriodIdParamName,
        defaultAccountingPeriodId ?? "",
      );
    });
  };

  return (
    <PageFilterFrame
      title="Fund Goal Trends"
      actions={
        transactionWorkspaceHref === null ? undefined : (
          <Button
            variant="outlined"
            onClick={() => {
              router.push(transactionWorkspaceHref);
            }}
          >
            View transactions
          </Button>
        )
      }
    >
      <AccountingPeriodRangeFilter
        accountingPeriods={accountingPeriods}
        startValue={currentStartAccountingPeriodId ?? ""}
        endValue={currentEndAccountingPeriodId ?? ""}
        onChange={handleAccountingPeriodRangeChange}
        disabled={disabled}
      />
      <FundTrendsFundNameFilter
        availableFundNames={availableFundNames}
        value={currentFundNames}
        onChange={handleFundNameChange}
        disabled={disabled}
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

export default FundGoalTrendsFilter;
