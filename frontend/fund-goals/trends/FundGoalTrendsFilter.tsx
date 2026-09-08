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
import { getDefaultTrendAccountingPeriodRange } from "@/framework/routes/trendRange";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";

/**
 * Props for the FundGoalTrendsFilter component.
 */
interface FundGoalTrendsFilterProps {
  readonly accountingPeriods: readonly AccountingPeriod[];
  readonly availableFundNames: readonly string[];
  readonly transactionWorkspaceHref: Route | null;
}

/**
 * Renders the trends filter card for the Fund Goals view.
 */
const FundGoalTrendsFilter = function ({
  accountingPeriods,
  availableFundNames,
  transactionWorkspaceHref,
}: FundGoalTrendsFilterProps): JSX.Element {
  const defaultRange = getDefaultTrendAccountingPeriodRange(accountingPeriods);
  const defaultStartAccountingPeriodId = defaultRange?.start ?? null;
  const defaultEndAccountingPeriodId = defaultRange?.end ?? null;
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
    defaultStartAccountingPeriodId;
  const currentEndAccountingPeriodId =
    searchParams.get(endAccountingPeriodIdParamName) ??
    defaultEndAccountingPeriodId;

  const updateParams = useSearchParamUpdater([
    pageParamName,
    balanceEventPageParamName,
  ]);

  const hasActiveView =
    shouldPersistFundNames(currentFundNames) ||
    currentStartAccountingPeriodId !== defaultStartAccountingPeriodId ||
    currentEndAccountingPeriodId !== defaultEndAccountingPeriodId;

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
        defaultStartAccountingPeriodId ?? "",
      );
      params.set(
        endAccountingPeriodIdParamName,
        defaultEndAccountingPeriodId ?? "",
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
      />
      <FundTrendsFundNameFilter
        availableFundNames={availableFundNames}
        value={currentFundNames}
        onChange={handleFundNameChange}
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
