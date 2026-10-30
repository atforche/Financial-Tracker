import type {
  AccountingPeriodPlanTab,
  AccountingPeriodSearchParams,
} from "@/accounting-periods/workspace/AccountingPeriodWorkspaceDetailPage";
import {
  getPageOffset,
  getRowsPerPage,
  normalizePageValue,
} from "@/framework/listframe/page";
import AccountGoalsFrame from "@/accounting-periods/workspace/AccountGoalsFrame";
import type { AccountingPeriodWithBalance } from "@/accounting-periods/types";
import FundGoalsFrame from "@/accounting-periods/workspace/FundGoalsFrame";
import type { JSX } from "react";
import UrlTabs from "@/framework/view/UrlTabs";
import createApiClient from "@/framework/data/createApiClient";
import unwrapApiResponse from "@/framework/data/unwrapApiResponse";

/**
 * Props for the AccountingPeriodPlanTabs component.
 */
interface AccountingPeriodPlanTabsProps {
  readonly accountingPeriod: AccountingPeriodWithBalance;
  readonly searchParams: AccountingPeriodSearchParams;
  readonly currentUrl: string;
}

/**
 * Displays an accounting period's Fund Goals and Account Goals as tabs, loading only the selected tab's list.
 */
const AccountingPeriodPlanTabs = async function ({
  accountingPeriod,
  searchParams,
  currentUrl,
}: AccountingPeriodPlanTabsProps): Promise<JSX.Element> {
  const apiClient = await createApiClient();
  const rowsPerPage = getRowsPerPage(searchParams.pageSize);
  const accountingPeriodId = accountingPeriod.id;
  // The tab comes from the URL, so anything unrecognized falls back to the first tab
  const activeTab: AccountingPeriodPlanTab =
    searchParams.planTab === "account-goals" ? "account-goals" : "fund-goals";
  const fundGoals =
    activeTab === "fund-goals"
      ? unwrapApiResponse(
          await apiClient.GET("/fund-goals", {
            params: {
              query: {
                "Filter.AccountingPeriodIds": [accountingPeriodId],
                ...(typeof searchParams.fundGoalSort === "undefined"
                  ? {}
                  : { Sort: searchParams.fundGoalSort }),
                Limit: rowsPerPage,
                Offset: getPageOffset(
                  normalizePageValue(searchParams.fundGoalPage),
                  rowsPerPage,
                ),
              },
            },
          }),
          "Failed to fetch fund goals",
        )
      : null;
  const accountGoals =
    activeTab === "account-goals"
      ? unwrapApiResponse(
          await apiClient.GET("/account-goals", {
            params: {
              query: {
                "Filter.AccountingPeriodIds": [accountingPeriodId],
                ...(typeof searchParams.accountGoalSort === "undefined"
                  ? {}
                  : { Sort: searchParams.accountGoalSort }),
                Limit: rowsPerPage,
                Offset: getPageOffset(
                  normalizePageValue(searchParams.accountGoalPage),
                  rowsPerPage,
                ),
              },
            },
          }),
          "Failed to fetch account goals",
        )
      : null;

  return (
    <UrlTabs
      label="Accounting period plan"
      paramName="planTab"
      tabs={[
        {
          value: "fund-goals",
          label: "Fund Goals",
          content:
            fundGoals === null ? null : (
              <FundGoalsFrame
                goals={fundGoals.items}
                totalCount={fundGoals.totalCount}
                accountingPeriodId={accountingPeriodId}
                returnUrl={currentUrl}
              />
            ),
        },
        {
          value: "account-goals",
          label: "Account Goals",
          content:
            accountGoals === null ? null : (
              <AccountGoalsFrame
                goals={accountGoals.items}
                totalCount={accountGoals.totalCount}
                accountingPeriodId={accountingPeriodId}
                returnUrl={currentUrl}
              />
            ),
        },
      ]}
    />
  );
};

export default AccountingPeriodPlanTabs;
