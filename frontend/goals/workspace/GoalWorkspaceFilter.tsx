"use client";

import { Button, Paper, Stack, Typography } from "@mui/material";
import type { AccountingPeriod } from "@/accounting-periods/types";
import AccountingPeriodEntryField from "@/accounting-periods/AccountingPeriodEntryField";
import type { GoalWorkspaceSearchParams } from "@/goals/workspace/GoalWorkspace";
import type { JSX } from "react";
import SearchBar from "@/framework/listframe/SearchBar";
import nameof from "@/framework/data/nameof";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";
import { useSearchParams } from "next/navigation";

/**
 * Props for the GoalWorkspaceFilter component.
 */
interface GoalWorkspaceFilterProps {
  readonly accountingPeriods: AccountingPeriod[];
  readonly selectedAccountingPeriodId: string | null;
}

/**
 * Filters the unified goal workspace by period and fund.
 */
const GoalWorkspaceFilter = function ({
  accountingPeriods,
  selectedAccountingPeriodId,
}: GoalWorkspaceFilterProps): JSX.Element {
  const searchParams = useSearchParams();
  const selectedPeriod =
    accountingPeriods.find(
      (period) => period.id === selectedAccountingPeriodId,
    ) ?? null;
  const searchParamName = nameof<GoalWorkspaceSearchParams>("search");
  const balanceEventPageParamName =
    nameof<GoalWorkspaceSearchParams>("balanceEventPage");
  const accountingPeriodParamName =
    nameof<GoalWorkspaceSearchParams>("accountingPeriodId");

  const updateParams = useSearchParamUpdater([balanceEventPageParamName]);

  const replace = updateParams;

  return (
    <Paper
      sx={{
        border: "1px solid",
        borderColor: "divider",
        borderRadius: 3,
        p: { xs: 2, md: 2.5 },
        maxWidth: 1440,
      }}
    >
      <Stack spacing={2}>
        <Typography variant="h5">Goals Workspace</Typography>
        <Stack direction="row" spacing={1.5} useFlexGap flexWrap="wrap">
          <Stack sx={{ minWidth: { xs: "100%", sm: 260 }, flex: 1 }}>
            <AccountingPeriodEntryField
              label="Accounting period"
              size="small"
              options={accountingPeriods}
              value={selectedPeriod}
              setValue={(period) => {
                replace((params) => {
                  if (period === null) {
                    params.delete(accountingPeriodParamName);
                  } else {
                    params.set(accountingPeriodParamName, period.id);
                  }
                });
              }}
            />
          </Stack>
          <SearchBar
            searchParamName={searchParamName}
            pageParamName={balanceEventPageParamName}
          />
          <Button
            variant="outlined"
            onClick={() => {
              replace((params) => {
                params.delete(accountingPeriodParamName);
                params.delete(searchParamName);
                params.delete(balanceEventPageParamName);
              });
            }}
            disabled={
              !searchParams.has(accountingPeriodParamName) &&
              (searchParams.get(searchParamName) ?? "").trim() === ""
            }
          >
            Reset filters
          </Button>
        </Stack>
      </Stack>
    </Paper>
  );
};

export default GoalWorkspaceFilter;
