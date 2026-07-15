import GoalWorkspaceCards from "@/goals/workspace/GoalWorkspaceCards";
import GoalWorkspaceFilter from "@/goals/workspace/GoalWorkspaceFilter";
import type { JSX } from "react";
import { Stack } from "@mui/material";
import getApiClient from "@/framework/data/getApiClient";
import { toRepeatedSearchParam } from "@/framework/routes/helpers";

/**
 * Search parameters for the GoalWorkspace component.
 */
interface GoalWorkspaceSearchParams {
  accountingPeriodId?: string;
  fundIds?: string | string[];
  search?: string;
  balanceEventPage?: string;
}

/**
 * Props for the GoalWorkspace component.
 */
interface GoalWorkspaceProps {
  readonly searchParams: Promise<GoalWorkspaceSearchParams>;
}

/**
 * Displays goal progress for one accounting period in a unified card workspace.
 */
const GoalWorkspace = async function ({
  searchParams,
}: GoalWorkspaceProps): Promise<JSX.Element> {
  const { accountingPeriodId, fundIds } = await searchParams;
  const apiClient = getApiClient();
  const { data: accountingPeriods } = await apiClient.GET(
    "/accounting-periods",
    { params: { query: { Limit: 500 } } },
  );

  if (typeof accountingPeriods === "undefined") {
    throw new Error("Failed to fetch goal workspace filters");
  }

  const selectedAccountingPeriodId =
    accountingPeriodId ?? accountingPeriods.items[0]?.id;
  const selectedFundIds = toRepeatedSearchParam(fundIds);
  const [assignmentGoalResponse, spendingGoalResponse] = await Promise.all([
    apiClient.GET("/goals/assignment", {
        params: {
          query: {
            ...(typeof selectedAccountingPeriodId === "string"
              ? { "Filter.AccountingPeriodIds": [selectedAccountingPeriodId] }
              : {}),
            ...(selectedFundIds.length > 0
              ? { "Filter.FundIds": selectedFundIds }
              : {}),
          },
        },
      }),
    apiClient.GET("/goals/spending", {
      params: {
        query: {
          ...(typeof selectedAccountingPeriodId === "string"
            ? { "Filter.AccountingPeriodIds": [selectedAccountingPeriodId] }
            : {}),
          ...(selectedFundIds.length > 0
            ? { "Filter.FundIds": selectedFundIds }
            : {}),
        },
      },
    }),
  ]);

  return (
    <Stack spacing={3} sx={{ width: "100%" }}>
      <GoalWorkspaceFilter
        accountingPeriods={accountingPeriods.items}
        selectedAccountingPeriodId={selectedAccountingPeriodId ?? null}
      />
      <GoalWorkspaceCards
        accountingPeriod={
          accountingPeriods.items.find(
            (period) => period.id === selectedAccountingPeriodId,
          ) ?? null
        }
        assignmentGoals={assignmentGoalResponse.data?.items ?? []}
        spendingGoals={spendingGoalResponse.data?.items ?? []}
      />
    </Stack>
  );
};

export type { GoalWorkspaceSearchParams };
export default GoalWorkspace;
