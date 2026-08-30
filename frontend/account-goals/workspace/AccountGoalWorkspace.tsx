import AccountGoalWorkspaceCards from "@/account-goals/workspace/AccountGoalWorkspaceCards";
import AccountGoalWorkspaceFilter from "@/account-goals/workspace/AccountGoalWorkspaceFilter";
import type { AccountGoalWorkspaceSearchParams } from "@/account-goals/workspace/types";
import type { JSX } from "react";
import PageLayout from "@/framework/view/PageLayout";
import createApiClient from "@/framework/data/createApiClient";
import { isNotNullOrUndefined } from "@/framework/nullHelpers";
import { toRepeatedSearchParams } from "@/framework/routes/helpers";
import unwrapApiResponse from "@/framework/data/unwrapApiResponse";

interface AccountGoalWorkspaceProps {
  readonly searchParams: Promise<AccountGoalWorkspaceSearchParams>;
}

/**
 * Displays Account Goal progress for one accounting period.
 */
const AccountGoalWorkspace = async function ({
  searchParams,
}: AccountGoalWorkspaceProps): Promise<JSX.Element> {
  const { accountingPeriodId, accountIds } = await searchParams;
  const apiClient = await createApiClient();
  const periods = unwrapApiResponse(
    await apiClient.GET("/accounting-periods", {
      params: { query: { Limit: 500 } },
    }),
    "Failed to fetch Account Goal workspace filters",
  );
  const selectedAccountingPeriodId = accountingPeriodId ?? periods.items[0]?.id;
  const selectedAccountIds = toRepeatedSearchParams(accountIds);
  const accountGoals = unwrapApiResponse(
    await apiClient.GET("/account-goals", {
      params: {
        query: {
          ...(isNotNullOrUndefined(selectedAccountingPeriodId)
            ? { "Filter.AccountingPeriodIds": [selectedAccountingPeriodId] }
            : {}),
          Limit: 500,
        },
      },
    }),
    "Failed to fetch Account Goals",
  );
  const progressResults =
    typeof selectedAccountingPeriodId === "string"
      ? unwrapApiResponse(
          await apiClient.GET("/account-goals/progress/{accountingPeriodId}", {
            params: {
              path: { accountingPeriodId: selectedAccountingPeriodId },
            },
          }),
          "Failed to fetch Account Goal progress",
        )
      : [];
  const progressByAccountGoalId = new Map(
    progressResults.map((result) => [result.accountGoalId, result.progress]),
  );
  const goalsWithProgress = accountGoals.items.flatMap((accountGoal) => {
    const progress = progressByAccountGoalId.get(accountGoal.id);
    return (selectedAccountIds.length === 0 ||
      selectedAccountIds.includes(accountGoal.account.id)) &&
      typeof progress !== "undefined"
      ? [{ ...accountGoal, progress }]
      : [];
  });
  return (
    <PageLayout>
      <AccountGoalWorkspaceFilter
        accountingPeriods={periods.items}
        accounts={accountGoals.items.map((goal) => goal.account)}
        selectedAccountingPeriodId={selectedAccountingPeriodId ?? null}
      />
      <AccountGoalWorkspaceCards
        accountingPeriod={
          periods.items.find(
            (period) => period.id === selectedAccountingPeriodId,
          ) ?? null
        }
        accountGoals={goalsWithProgress}
      />
    </PageLayout>
  );
};

export default AccountGoalWorkspace;
