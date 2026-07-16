"use client";

import { Button, Paper, Stack, Typography } from "@mui/material";
import type { AccountingPeriod } from "@/accounting-periods/types";
import AccountingPeriodFilter from "@/accounting-periods/AccountingPeriodFilter";
import type { JSX } from "react";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";
import { useSearchParams } from "next/navigation";

/**
 * Props for the AccountingPeriodTrendsFilter component.
 */
interface AccountingPeriodTrendsFilterProps {
  readonly accountingPeriods: readonly AccountingPeriod[];
  readonly defaultAccountingPeriodId: string | null;
  readonly disabled?: boolean;
}

/**
 * Renders the trends filter card for the Accounting Periods view.
 */
const AccountingPeriodTrendsFilter = function ({
  accountingPeriods,
  defaultAccountingPeriodId,
  disabled = false,
}: AccountingPeriodTrendsFilterProps): JSX.Element {
  const searchParams = useSearchParams();

  const pageParamName = "page";
  const modeParamName = "mode";
  const startAccountingPeriodIdParamName = "startAccountingPeriodId";
  const endAccountingPeriodIdParamName = "endAccountingPeriodId";

  const currentStartAccountingPeriodId =
    searchParams.get(startAccountingPeriodIdParamName) ??
    defaultAccountingPeriodId ??
    "";
  const currentEndAccountingPeriodId =
    searchParams.get(endAccountingPeriodIdParamName) ??
    defaultAccountingPeriodId ??
    "";

  const accountingPeriodIndexes = new Map(
    accountingPeriods.map((period, index) => [period.id, index]),
  );

  const updateParams = useSearchParamUpdater([pageParamName]);

  const hasActiveView =
    currentStartAccountingPeriodId !== (defaultAccountingPeriodId ?? "") ||
    currentEndAccountingPeriodId !== (defaultAccountingPeriodId ?? "");

  const handleStartAccountingPeriodChange = function (
    nextStartAccountingPeriodId: string,
  ): void {
    const nextStartIndex =
      accountingPeriodIndexes.get(nextStartAccountingPeriodId) ?? 0;
    const currentEndIndex =
      accountingPeriodIndexes.get(currentEndAccountingPeriodId) ??
      nextStartIndex;
    const nextEndAccountingPeriodId =
      nextStartIndex > currentEndIndex
        ? nextStartAccountingPeriodId
        : currentEndAccountingPeriodId;

    updateParams((params) => {
      params.set(modeParamName, "accounting-period");
      params.set(startAccountingPeriodIdParamName, nextStartAccountingPeriodId);
      params.set(endAccountingPeriodIdParamName, nextEndAccountingPeriodId);
    });
  };

  const handleEndAccountingPeriodChange = function (
    nextEndAccountingPeriodId: string,
  ): void {
    const nextEndIndex =
      accountingPeriodIndexes.get(nextEndAccountingPeriodId) ?? 0;
    const currentStartIndex =
      accountingPeriodIndexes.get(currentStartAccountingPeriodId) ??
      nextEndIndex;
    const nextStartAccountingPeriodId =
      nextEndIndex < currentStartIndex
        ? nextEndAccountingPeriodId
        : currentStartAccountingPeriodId;

    updateParams((params) => {
      params.set(modeParamName, "accounting-period");
      params.set(startAccountingPeriodIdParamName, nextStartAccountingPeriodId);
      params.set(endAccountingPeriodIdParamName, nextEndAccountingPeriodId);
    });
  };

  const clearView = function (): void {
    updateParams((params) => {
      params.set(modeParamName, "accounting-period");
      if (defaultAccountingPeriodId !== null) {
        params.set(startAccountingPeriodIdParamName, defaultAccountingPeriodId);
        params.set(endAccountingPeriodIdParamName, defaultAccountingPeriodId);
      } else {
        params.delete(startAccountingPeriodIdParamName);
        params.delete(endAccountingPeriodIdParamName);
      }
    });
  };

  return (
    <Paper
      sx={{
        position: "sticky",
        top: 10,
        zIndex: (theme) => theme.zIndex.appBar - 1,
        border: "1px solid",
        borderColor: "divider",
        borderRadius: 3,
        bgcolor: "background.paper",
        p: { xs: 2, md: 2.5 },
      }}
    >
      <Stack spacing={2}>
        <Stack spacing={0.5}>
          <Typography variant="h5">Accounting Period Trends</Typography>
        </Stack>
        <Stack
          direction="row"
          spacing={1.5}
          useFlexGap
          flexWrap="wrap"
          alignItems={{ xs: "stretch", md: "center" }}
        >
          <AccountingPeriodFilter
            accountingPeriods={accountingPeriods}
            label="Start period"
            value={currentStartAccountingPeriodId}
            onChange={handleStartAccountingPeriodChange}
            disabled={disabled}
          />
          <AccountingPeriodFilter
            accountingPeriods={accountingPeriods}
            label="End period"
            value={currentEndAccountingPeriodId}
            onChange={handleEndAccountingPeriodChange}
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
        </Stack>
      </Stack>
    </Paper>
  );
};

export default AccountingPeriodTrendsFilter;
