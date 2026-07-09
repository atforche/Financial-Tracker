import type { CurrentGoals } from "@/goals/types";
import GoalWorkspaceCards from "@/goals/workspace/GoalWorkspaceCards";
import GoalWorkspaceFilter from "@/goals/workspace/GoalWorkspaceFilter";
import type { JSX } from "react";
import { Stack } from "@mui/material";
import getApiClient from "@/framework/data/getApiClient";

interface GoalWorkspaceSearchParams {
  accountingPeriodId?: string;
  fundIds?: string | string[];
  balanceEventPage?: string;
}

interface GoalWorkspaceProps {
  readonly searchParams: Promise<GoalWorkspaceSearchParams>;
}

const toRepeatedSearchParam = function (
  value: string | string[] | undefined,
): string[] {
  return Array.isArray(value)
    ? value
    : typeof value === "string"
      ? [value]
      : [];
};

const createEmptyGoals = function (): CurrentGoals {
  return {
    accountingPeriodId: null,
    accountingPeriodName: null,
    availableFundNames: [],
    summary: {
      totalAmountToAssign: 0,
      totalAmountAssigned: 0,
      percentageOfAssignmentGoalsMet: {
        totalCount: 0,
        metCount: 0,
        percentageMet: 0,
      },
      totalAmountToSpend: 0,
      totalAmountSpent: 0,
      percentageOfSpendingGoalsMet: {
        totalCount: 0,
        metCount: 0,
        percentageMet: 0,
      },
    },
    goals: [],
  };
};

/** Displays goal progress for one accounting period in a unified card workspace. */
const GoalWorkspace = async function ({
  searchParams,
}: GoalWorkspaceProps): Promise<JSX.Element> {
  const { accountingPeriodId, fundIds } = await searchParams;
  const apiClient = getApiClient();
  const [{ data: accountingPeriods }, { data: funds }] = await Promise.all([
    apiClient.GET("/accounting-periods", { params: { query: { Limit: 500 } } }),
    apiClient.GET("/funds"),
  ]);

  if (
    typeof accountingPeriods === "undefined" ||
    typeof funds === "undefined"
  ) {
    throw new Error("Failed to fetch goal workspace filters");
  }

  const selectedAccountingPeriodId =
    accountingPeriodId ?? accountingPeriods.items[0]?.id;
  const selectedFundIds = toRepeatedSearchParam(fundIds);
  const current =
    (
      await apiClient.GET("/goals/current", {
        params: {
          query: {
            ...(typeof selectedAccountingPeriodId === "string"
              ? { AccountingPeriodId: selectedAccountingPeriodId }
              : {}),
            ...(selectedFundIds.length > 0 ? { FundIds: selectedFundIds } : {}),
          },
        },
      })
    ).data ?? createEmptyGoals();

  return (
    <Stack spacing={3} sx={{ width: "100%" }}>
      <GoalWorkspaceFilter
        accountingPeriods={accountingPeriods.items}
        funds={funds.items}
        selectedAccountingPeriodId={selectedAccountingPeriodId ?? null}
      />
      <GoalWorkspaceCards current={current} />
    </Stack>
  );
};

export type { GoalWorkspaceSearchParams };
export default GoalWorkspace;
