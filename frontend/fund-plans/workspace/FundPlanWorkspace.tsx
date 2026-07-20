import FundPlanWorkspaceCards from "@/fund-plans/workspace/FundPlanWorkspaceCards";
import FundPlanWorkspaceFilter from "@/fund-plans/workspace/FundPlanWorkspaceFilter";
import type { JSX } from "react";
import PageLayout from "@/framework/view/PageLayout";
import createApiClient from "@/framework/data/createApiClient";
import { isNotNullOrUndefined } from "@/framework/nullHelpers";
import { toRepeatedSearchParams } from "@/framework/routes/helpers";
import unwrapApiResponse from "@/framework/data/unwrapApiResponse";

/**
 * Search parameters for the FundPlanWorkspace component.
 */
interface FundPlanWorkspaceSearchParams {
  accountingPeriodId?: string;
  fundIds?: string | string[];
  search?: string;
  balanceEventPage?: string;
}

/**
 * Props for the FundPlanWorkspace component.
 */
interface FundPlanWorkspaceProps {
  readonly searchParams: Promise<FundPlanWorkspaceSearchParams>;
}

/**
 * Displays Funding Plan progress for one accounting period.
 */
const FundPlanWorkspace = async function ({
  searchParams,
}: FundPlanWorkspaceProps): Promise<JSX.Element> {
  const { accountingPeriodId, fundIds } = await searchParams;
  const apiClient = createApiClient();
  const periods = unwrapApiResponse(
    await apiClient.GET("/accounting-periods", {
      params: { query: { Limit: 500 } },
    }),
    "Failed to fetch Funding Plan workspace filters",
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
              "Failed to fetch Funding Plan progress",
            ),
          })),
        )
      : [];
  return (
    <PageLayout>
      <FundPlanWorkspaceFilter
        accountingPeriods={periods.items}
        selectedAccountingPeriodId={selectedAccountingPeriodId ?? null}
      />
      <FundPlanWorkspaceCards
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
export type { FundPlanWorkspaceSearchParams };
export default FundPlanWorkspace;
