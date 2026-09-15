"use client";

import type {
  AccountingPeriod,
  AccountingPeriodRange,
} from "@/accounting-periods/types";
import { Button, MenuItem, TextField } from "@mui/material";
import AccountingPeriodRangeFilter from "@/accounting-periods/AccountingPeriodRangeFilter";
import type { JSX } from "react";
import PageFilterFrame from "@/framework/view/PageFilterFrame";
import { fundGoalTrendsParamNames } from "@/fund-goals/trends/helpers";
import { getDefaultTrendAccountingPeriodRange } from "@/framework/routes/trendRange";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";
import { useSearchParams } from "next/navigation";

/**
 * Props for the FundGoalTrendsFilter component.
 */
interface FundGoalTrendsFilterProps {
  readonly accountingPeriods: readonly AccountingPeriod[];
  readonly funds: readonly { id: string; name: string }[];
}

/**
 * Renders the trends filter card for the Fund Goals view.
 */
const FundGoalTrendsFilter = function ({
  accountingPeriods,
  funds,
}: FundGoalTrendsFilterProps): JSX.Element {
  const defaultRange = getDefaultTrendAccountingPeriodRange(accountingPeriods);
  const defaultStartAccountingPeriodId = defaultRange?.start ?? null;
  const defaultEndAccountingPeriodId = defaultRange?.end ?? null;
  const searchParams = useSearchParams();

  const pageParamName = fundGoalTrendsParamNames.page;
  const balanceEventPageParamName = fundGoalTrendsParamNames.balanceEventPage;
  const fundIdParamName = fundGoalTrendsParamNames.fundId;
  const startAccountingPeriodIdParamName =
    fundGoalTrendsParamNames.startAccountingPeriodId;
  const endAccountingPeriodIdParamName =
    fundGoalTrendsParamNames.endAccountingPeriodId;

  const selectedFundId = funds.some(
    (fund) => fund.id === searchParams.get(fundIdParamName),
  )
    ? (searchParams.get(fundIdParamName) ?? "all")
    : (funds.find(
        (fund) =>
          fund.name === searchParams.get(fundGoalTrendsParamNames.fundName),
      )?.id ?? "all");
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
    selectedFundId !== "all" ||
    searchParams.has(fundGoalTrendsParamNames.fundName) ||
    currentStartAccountingPeriodId !== defaultStartAccountingPeriodId ||
    currentEndAccountingPeriodId !== defaultEndAccountingPeriodId;

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
      params.delete(fundIdParamName);
      params.delete(fundGoalTrendsParamNames.fundName);
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
    <PageFilterFrame title="Fund Goal Trends">
      <AccountingPeriodRangeFilter
        accountingPeriods={accountingPeriods}
        startValue={currentStartAccountingPeriodId ?? ""}
        endValue={currentEndAccountingPeriodId ?? ""}
        onChange={handleAccountingPeriodRangeChange}
      />
      <TextField
        select
        label="View"
        size="small"
        value={selectedFundId}
        onChange={(event) => {
          updateParams((params) => {
            params.delete(fundGoalTrendsParamNames.fundName);
            if (event.target.value === "all") {
              params.delete(fundIdParamName);
            } else {
              params.set(fundIdParamName, event.target.value);
            }
          });
        }}
        sx={{ minWidth: 220 }}
      >
        <MenuItem value="all">All Funds</MenuItem>
        {funds.map((fund) => (
          <MenuItem key={fund.id} value={fund.id}>
            {fund.name}
          </MenuItem>
        ))}
      </TextField>
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
