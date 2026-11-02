import AccountingPeriodGoalCards from "@/accounting-periods/workspace/AccountingPeriodGoalCards";
import AccountingPeriodHeader from "@/accounting-periods/workspace/AccountingPeriodHeader";
import AccountingPeriodPlanCard from "@/accounting-periods/workspace/AccountingPeriodPlanCard";
import AccountingPeriodProgressCard from "@/accounting-periods/workspace/AccountingPeriodProgressCard";
import BalanceStatCards from "@/accounts/BalanceStatCards";
import { Box } from "@mui/material";
import ExpectedIncomeSourcesFrame from "@/accounting-periods/workspace/ExpectedIncomeSourcesFrame";
import type { JSX } from "react";
import PageLayout from "@/framework/view/PageLayout";
import SignedChange from "@/framework/view/SignedChange";
import createApiClient from "@/framework/data/createApiClient";
import { expectedIncomeSourcesId } from "@/accounting-periods/workspace/helpers";
import fillHeightSx from "@/framework/view/fillHeightSx";
import fundGoalRoutes from "@/fund-goals/routes";
import { getCurrencyDifference } from "@/framework/currencyHelpers";
import { redirect } from "next/navigation";
import routes from "@/accounting-periods/routes";
import unwrapApiResponse from "@/framework/data/unwrapApiResponse";

/**
 * Search parameters supported by an accounting period's page.
 */
interface AccountingPeriodSearchParams {
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
 * Displays an accounting period's balances, progress against its plan, and the income sources it plans, with links
 * to its goals.
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
  if (detailResponse.response.status === 404) {
    redirect(routes.workspace({}));
  }
  const detail = unwrapApiResponse(
    detailResponse,
    "Failed to fetch accounting period",
  );
  const period = detail.accountingPeriod;
  const currentUrl = routes.workspaceDetail(period.id, resolvedSearchParams);

  return (
    <PageLayout>
      <AccountingPeriodHeader
        detail={detail}
        searchParams={resolvedSearchParams}
        currentUrl={currentUrl}
      />
      <BalanceStatCards
        openingBalance={detail.accountOpeningBalance}
        closingBalance={detail.accountClosingBalance}
        unassignedBalance={detail.fundClosingBalance.totalUnassignedBalance}
        unassignedCaption={
          <>
            <SignedChange
              amount={getCurrencyDifference(
                detail.fundClosingBalance.totalUnassignedBalance,
                detail.fundOpeningBalance.totalUnassignedBalance,
              )}
            />{" "}
            this period
          </>
        }
        changeDescription="this period"
      />
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
            fundGoalsHref={fundGoalRoutes.workspace({
              accountingPeriodId: period.id,
              returnUrl: currentUrl,
            })}
          />
        </Box>
      </Box>
      <Box id={expectedIncomeSourcesId}>
        <ExpectedIncomeSourcesFrame
          accountingPeriod={period}
          redirectUrl={currentUrl}
        />
      </Box>
      <AccountingPeriodGoalCards
        accountingPeriodId={period.id}
        summary={detail.goalSummary}
        returnUrl={currentUrl}
      />
    </PageLayout>
  );
};

export type { AccountingPeriodSearchParams };
export default AccountingPeriodWorkspaceDetailPage;
