"use client";

import { Button, Stack } from "@mui/material";
import type { AccountingPeriod } from "@/accounting-periods/types";
import AccountingPeriodEntryField from "@/accounting-periods/AccountingPeriodEntryField";
import type { FundGoalWorkspaceSearchParams } from "@/fund-goals/workspace/FundGoalWorkspace";
import type { JSX } from "react";
import PageFilterFrame from "@/framework/view/PageFilterFrame";
import SearchBar from "@/framework/listframe/SearchBar";
import propertyName from "@/framework/data/propertyName";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";
import { useSearchParams } from "next/navigation";

/**
 * Props for the FundGoalWorkspaceFilter component.
 */
interface FundGoalWorkspaceFilterProps {
  readonly accountingPeriods: AccountingPeriod[];
  readonly selectedAccountingPeriodId: string | null;
}

/**
 * Filters the Fund Goal workspace by period and fund.
 */
const FundGoalWorkspaceFilter = function ({
  accountingPeriods,
  selectedAccountingPeriodId,
}: FundGoalWorkspaceFilterProps): JSX.Element {
  const searchParams = useSearchParams();
  const selectedPeriod =
    accountingPeriods.find(
      (period) => period.id === selectedAccountingPeriodId,
    ) ?? null;
  const searchParamName = propertyName<FundGoalWorkspaceSearchParams>("search");
  const balanceEventPageParamName =
    propertyName<FundGoalWorkspaceSearchParams>("balanceEventPage");
  const accountingPeriodParamName =
    propertyName<FundGoalWorkspaceSearchParams>("accountingPeriodId");

  const updateParams = useSearchParamUpdater([balanceEventPageParamName]);

  const replace = updateParams;

  return (
    <PageFilterFrame
      title="Fund Goals"
      actions={
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
          Reset Filters
        </Button>
      }
    >
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
    </PageFilterFrame>
  );
};

export default FundGoalWorkspaceFilter;
