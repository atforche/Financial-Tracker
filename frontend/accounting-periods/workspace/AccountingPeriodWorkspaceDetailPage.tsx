import {
  formatCurrency,
  getCurrencyDifference,
} from "@/framework/currencyHelpers";
import type { AccountGoalSort } from "@/account-goals/types";
import AccountingPeriodHeader from "@/accounting-periods/workspace/AccountingPeriodHeader";
import AccountingPeriodPlanCard from "@/accounting-periods/workspace/AccountingPeriodPlanCard";
import AccountingPeriodPlanTabs from "@/accounting-periods/workspace/AccountingPeriodPlanTabs";
import AccountingPeriodProgressCard from "@/accounting-periods/workspace/AccountingPeriodProgressCard";
import { Box } from "@mui/material";
import type { ExpectedIncomeSourceSort } from "@/accounting-periods/types";
import type { FundGoalSort } from "@/fund-goals/types";
import type { JSX } from "react";
import PageLayout from "@/framework/view/PageLayout";
import ResponsiveGrid from "@/framework/view/ResponsiveGrid";
import SignedChange from "@/framework/view/SignedChange";
import StatCard from "@/framework/view/StatCard";
import createApiClient from "@/framework/data/createApiClient";
import { getAccountTypeStatDetails } from "@/accounts/balanceSummaryHelpers";
import { redirect } from "next/navigation";
import routes from "@/accounting-periods/routes";
import unwrapApiResponse from "@/framework/data/unwrapApiResponse";

/**
 * The list shown below an accounting period's progress: its Fund Goals, Account Goals, or Expected Income Sources.
 */
type AccountingPeriodPlanTab =
  "fund-goals" | "account-goals" | "income-sources";

/**
 * Search parameters supported by an accounting period's page.
 */
interface AccountingPeriodSearchParams {
  planTab?: AccountingPeriodPlanTab;
  pageSize?: number | string | null;
  fundGoalPage?: number | string | null;
  fundGoalSort?: FundGoalSort;
  accountGoalPage?: number | string | null;
  accountGoalSort?: AccountGoalSort;
  incomeSourcePage?: number | string | null;
  incomeSourceSort?: ExpectedIncomeSourceSort;
  returnUrl?: string;
}

/**
 * Props for the AccountingPeriodWorkspaceDetailPage component.
 */
interface AccountingPeriodWorkspaceDetailPageProps {
  readonly params: Promise<{ accountingPeriodId: string }>;
  readonly searchParams: Promise<AccountingPeriodSearchParams>;
}

/**
 * Stretches a section's surface to the height of its row so side-by-side sections line up.
 */
const fillHeightSx = {
  display: "flex",
  flexDirection: "column",
  "& > .MuiPaper-root": { flex: 1 },
} as const;

/**
 * Displays an accounting period's balances, progress against its plan, and the goals and income sources it plans.
 */
const AccountingPeriodWorkspaceDetailPage = async function ({
  params,
  searchParams,
}: AccountingPeriodWorkspaceDetailPageProps): Promise<JSX.Element> {
  const { accountingPeriodId } = await params;
  const resolvedSearchParams = await searchParams;
  const apiClient = await createApiClient();
  const detailResponse = await apiClient.GET(
    "/accounting-periods/{accountingPeriodId}/detail",
    { params: { path: { accountingPeriodId } } },
  );
  if (detailResponse.error) {
    redirect(routes.workspace({}));
  }
  const detail = unwrapApiResponse(
    detailResponse,
    "Failed to fetch accounting period",
  );
  const period = detail.accountingPeriod;
  const currentUrl = routes.workspaceDetail(period.id, resolvedSearchParams);
  const opening = detail.accountOpeningBalance;
  const closing = detail.accountClosingBalance;
  const describeChange = function (
    current: number,
    previous: number,
  ): JSX.Element {
    return (
      <>
        <SignedChange amount={getCurrencyDifference(current, previous)} /> this
        period
      </>
    );
  };

  return (
    <PageLayout>
      <AccountingPeriodHeader
        detail={detail}
        searchParams={resolvedSearchParams}
        currentUrl={currentUrl}
      />
      <ResponsiveGrid columns={{ xs: 1, sm: 2, lg: 4 }}>
        <StatCard
          label="Net worth"
          value={formatCurrency(closing.totalBalance)}
          caption={describeChange(closing.totalBalance, opening.totalBalance)}
        />
        <StatCard
          label="Tracked money"
          value={formatCurrency(closing.totalTrackedBalance)}
          caption={describeChange(
            closing.totalTrackedBalance,
            opening.totalTrackedBalance,
          )}
          details={getAccountTypeStatDetails(opening, closing, true)}
          detailsLabel="by account type"
        />
        <StatCard
          label="Untracked money"
          value={formatCurrency(closing.totalUntrackedBalance)}
          caption={describeChange(
            closing.totalUntrackedBalance,
            opening.totalUntrackedBalance,
          )}
          details={getAccountTypeStatDetails(opening, closing, false)}
          detailsLabel="by account type"
        />
        <StatCard
          label="Unassigned funds"
          value={formatCurrency(
            detail.fundClosingBalance.totalUnassignedBalance,
          )}
          caption={describeChange(
            detail.fundClosingBalance.totalUnassignedBalance,
            detail.fundOpeningBalance.totalUnassignedBalance,
          )}
        />
      </ResponsiveGrid>
      <Box
        sx={{
          display: "grid",
          gap: 2,
          gridTemplateColumns: { xs: "minmax(0, 1fr)", lg: "2fr 1fr" },
        }}
      >
        <Box sx={fillHeightSx}>
          <AccountingPeriodProgressCard
            accountingPeriod={period}
            totalSpending={detail.totalSpending}
          />
        </Box>
        <Box sx={fillHeightSx}>
          <AccountingPeriodPlanCard
            accountingPeriod={period}
            cappedGoalCount={detail.cappedGoalCount}
            fundGoalsHref={routes.workspaceDetail(period.id, {
              ...resolvedSearchParams,
              planTab: "fund-goals",
            })}
          />
        </Box>
      </Box>
      <AccountingPeriodPlanTabs
        accountingPeriod={period}
        searchParams={resolvedSearchParams}
        currentUrl={currentUrl}
      />
    </PageLayout>
  );
};

export type { AccountingPeriodPlanTab, AccountingPeriodSearchParams };
export default AccountingPeriodWorkspaceDetailPage;
