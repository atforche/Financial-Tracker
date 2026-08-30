import {
  isNotNullOrUndefined,
  isNullOrUndefined,
} from "@/framework/nullHelpers";
import AccountGoalContextFrame from "@/account-goals/workspace/AccountGoalContextFrame";
import AccountGoalWorkspacePageHeader from "@/account-goals/workspace/AccountGoalWorkspacePageHeader";
import type { AccountGoalWorkspaceSearchParams } from "@/account-goals/workspace/types";
import type { JSX } from "react";
import PageLayout from "@/framework/view/PageLayout";
import createApiClient from "@/framework/data/createApiClient";
import { redirect } from "next/navigation";
import routes from "@/account-goals/routes";
import { toRepeatedSearchParams } from "@/framework/routes/helpers";
import unwrapApiResponse from "@/framework/data/unwrapApiResponse";

interface AccountGoalWorkspaceDetailPageProps {
  readonly params: Promise<{ accountId: string }>;
  readonly searchParams: Promise<AccountGoalWorkspaceSearchParams>;
}

/**
 * Displays Account Goal details and progress for one account and period.
 */
const AccountGoalWorkspaceDetailPage = async function ({
  params,
  searchParams,
}: AccountGoalWorkspaceDetailPageProps): Promise<JSX.Element> {
  const { accountId } = await params;
  const { accountingPeriodId, accountIds, returnUrl } = await searchParams;
  const selectedAccountIds = toRepeatedSearchParams(accountIds);
  const apiClient = await createApiClient();
  const periods = unwrapApiResponse(
    await apiClient.GET("/accounting-periods", {
      params: { query: { Limit: 500 } },
    }),
    "Failed to fetch Account Goal accounting periods",
  );
  const periodId = accountingPeriodId ?? periods.items[0]?.id;
  const workspaceUrl = routes.workspace({
    ...(isNotNullOrUndefined(periodId) ? { accountingPeriodId: periodId } : {}),
    ...(selectedAccountIds.length ? { accountIds: selectedAccountIds } : {}),
    ...(isNotNullOrUndefined(returnUrl) ? { returnUrl } : {}),
  });
  if (isNullOrUndefined(periodId)) {
    redirect(workspaceUrl);
  }
  const goalResponse = await apiClient.GET(
    "/account-goals/account/{accountId}",
    {
      params: { path: { accountId }, query: { accountingPeriodId: periodId } },
    },
  );
  if (goalResponse.response.status === 404) {
    redirect(workspaceUrl);
  }
  const accountGoal = unwrapApiResponse(
    goalResponse,
    "Failed to fetch the Account Goal",
  );
  if (isNullOrUndefined(accountGoal.accountingPeriod)) {
    redirect(workspaceUrl);
  }
  const progress = unwrapApiResponse(
    await apiClient.GET(
      "/account-goals/{accountGoalId}/progress/{accountingPeriodId}",
      {
        params: {
          path: { accountGoalId: accountGoal.id, accountingPeriodId: periodId },
        },
      },
    ),
    "Failed to fetch Account Goal progress",
  );
  const currentUrl = routes.workspaceDetail(accountId, {
    accountingPeriodId: periodId,
    ...(selectedAccountIds.length ? { accountIds: selectedAccountIds } : {}),
  });
  return (
    <PageLayout>
      <AccountGoalWorkspacePageHeader
        backHref={returnUrl ?? workspaceUrl}
        title="Account Goal Details"
      />
      <AccountGoalContextFrame
        accountGoal={accountGoal}
        progress={progress}
        redirectUrl={currentUrl}
        isReadOnly={!accountGoal.accountingPeriod.isOpen}
      />
    </PageLayout>
  );
};

export default AccountGoalWorkspaceDetailPage;
