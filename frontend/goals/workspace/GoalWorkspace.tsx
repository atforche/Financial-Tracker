import GoalWorkspaceCards from "@/goals/workspace/GoalWorkspaceCards";
import GoalWorkspaceFilter from "@/goals/workspace/GoalWorkspaceFilter";
import type { JSX } from "react";
import PageLayout from "@/framework/view/PageLayout";
import createApiClient from "@/framework/data/createApiClient";
import { isNotNullOrUndefined } from "@/framework/nullHelpers";
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
  const periods = unwrapApiResponse(
    await apiClient.GET("/accounting-periods", {
      params: { query: { Limit: 500 } },
    }),
    "Failed to fetch goal workspace filters",
  );
  const selectedAccountingPeriodId = accountingPeriodId ?? periods.items[0]?.id;
  const selectedFundIds = toRepeatedSearchParams(fundIds);
  const plans = unwrapApiResponse(
    await apiClient.GET("/fund-plans", {
      params: {
        query: {
          ...(isNotNullOrUndefined(selectedAccountingPeriodId)
            ? { "Filter.AccountingPeriodIds": [selectedAccountingPeriodId] }
            : {}),
          ...(selectedFundIds.length
            ? { "Filter.FundIds": selectedFundIds }
            : {}),
          Limit: 500,
        },
      },
    }),
    "Failed to fetch fund plans",
  );
  const plansWithProgress =
    typeof selectedAccountingPeriodId === "string"
      ? await Promise.all(
          plans.items.map(async (fundPlan) => ({
            ...fundPlan,
            progress: unwrapApiResponse(
              await apiClient.GET(
                "/fund-plans/{fundPlanId}/progress/{accountingPeriodId}",
                {
                  params: {
                    path: {
                      fundPlanId: fundPlan.id,
                      accountingPeriodId: selectedAccountingPeriodId,
                    },
                  },
                },
              ),
              "Failed to fetch goal progress",
            ),
          })),
        )
      : [];
  return (
    <PageLayout>
      <GoalWorkspaceFilter
        accountingPeriods={periods.items}
        selectedAccountingPeriodId={selectedAccountingPeriodId ?? null}
      />
      <GoalWorkspaceCards
        accountingPeriod={
          periods.items.find(
            (period) => period.id === selectedAccountingPeriodId,
          ) ?? null
        }
        fundPlans={plansWithProgress}
      />
    </PageLayout>
  );
};
export type { GoalWorkspaceSearchParams };
export default GoalWorkspace;
