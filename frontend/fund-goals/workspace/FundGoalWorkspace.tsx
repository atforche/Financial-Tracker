import type { FundGoalBalanceEventSort } from "@/fund-goals/types";
import FundGoalWorkspaceCards from "@/fund-goals/workspace/FundGoalWorkspaceCards";
import FundGoalWorkspaceFilter from "@/fund-goals/workspace/FundGoalWorkspaceFilter";
import type { JSX } from "react";
import PageLayout from "@/framework/view/PageLayout";
import createApiClient from "@/framework/data/createApiClient";
import { isNotNullOrUndefined } from "@/framework/nullHelpers";
import { toRepeatedSearchParams } from "@/framework/routes/helpers";
import unwrapApiResponse from "@/framework/data/unwrapApiResponse";

/**
 * Search parameters for the FundGoalWorkspace component.
 */
interface FundGoalWorkspaceSearchParams {
  accountingPeriodId?: string;
  fundIds?: string | string[];
  search?: string;
  balanceEventPage?: string;
  pageSize?: number | string | null;
  balanceEventSort?: FundGoalBalanceEventSort;
}

/**
 * Props for the FundGoalWorkspace component.
 */
interface FundGoalWorkspaceProps {
  readonly searchParams: Promise<FundGoalWorkspaceSearchParams>;
}

/**
 * Displays Fund Goal progress for one accounting period.
 */
const FundGoalWorkspace = async function ({
  searchParams,
}: FundGoalWorkspaceProps): Promise<JSX.Element> {
  const { accountingPeriodId, fundIds } = await searchParams;
  const apiClient = await createApiClient();
  const periods = unwrapApiResponse(
    await apiClient.GET("/accounting-periods", {
      params: { query: { Limit: 500 } },
    }),
    "Failed to fetch Fund Goal workspace filters",
  );
  const selectedAccountingPeriodId = accountingPeriodId ?? periods.items[0]?.id;
  const selectedFundIds = toRepeatedSearchParams(fundIds);
  const fundGoals = unwrapApiResponse(
    await apiClient.GET("/fund-goals", {
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
    "Failed to fetch fund goals",
  );
  const progressResults =
    typeof selectedAccountingPeriodId === "string"
      ? unwrapApiResponse(
          await apiClient.GET("/fund-goals/progress/{accountingPeriodId}", {
            params: {
              path: { accountingPeriodId: selectedAccountingPeriodId },
            },
          }),
          "Failed to fetch Fund Goal progress",
        )
      : [];
  const progressByFundGoalId = new Map(
    progressResults.map((result) => [result.fundGoalId, result.progress]),
  );
  const goalsWithProgress = fundGoals.items.flatMap((fundGoal) => {
    const progress = progressByFundGoalId.get(fundGoal.id);
    return typeof progress === "undefined" ? [] : [{ ...fundGoal, progress }];
  });
  return (
    <PageLayout>
      <FundGoalWorkspaceFilter
        accountingPeriods={periods.items}
        selectedAccountingPeriodId={selectedAccountingPeriodId ?? null}
      />
      <FundGoalWorkspaceCards
        accountingPeriod={
          periods.items.find(
            (period) => period.id === selectedAccountingPeriodId,
          ) ?? null
        }
        fundGoals={goalsWithProgress}
      />
    </PageLayout>
  );
};
export type { FundGoalWorkspaceSearchParams };
export default FundGoalWorkspace;
