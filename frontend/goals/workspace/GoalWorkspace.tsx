import GoalWorkspaceCards from "@/goals/workspace/GoalWorkspaceCards";
import GoalWorkspaceFilter from "@/goals/workspace/GoalWorkspaceFilter";
import type { JSX } from "react";
import PageLayout from "@/framework/view/PageLayout";
import createApiClient from "@/framework/data/createApiClient";
import { toRepeatedSearchParams } from "@/framework/routes/helpers";
import unwrapApiResponse from "@/framework/data/unwrapApiResponse";

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
  const apiClient = createApiClient();
  const accountingPeriodsResponse = await apiClient.GET("/accounting-periods", {
    params: { query: { Limit: 500 } },
  });

  const accountingPeriods = unwrapApiResponse(
    accountingPeriodsResponse,
    "Failed to fetch goal workspace filters",
  );

  const selectedAccountingPeriodId =
    accountingPeriodId ?? accountingPeriods.items[0]?.id;
  const selectedFundIds = toRepeatedSearchParams(fundIds);
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
  const assignmentGoals = unwrapApiResponse(
    assignmentGoalResponse,
    "Failed to fetch assignment goals",
  );
  const spendingGoals = unwrapApiResponse(
    spendingGoalResponse,
    "Failed to fetch spending goals",
  );

  return (
    <PageLayout>
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
        assignmentGoals={assignmentGoals.items}
        spendingGoals={spendingGoals.items}
      />
    </PageLayout>
  );
};

export type { GoalWorkspaceSearchParams };
export default GoalWorkspace;
