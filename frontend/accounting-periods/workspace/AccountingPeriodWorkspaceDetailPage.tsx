import AccountingPeriodEntityList, {
  AccountingPeriodEntityControls,
} from "@/accounting-periods/workspace/AccountingPeriodEntityList";
import { Box, Stack } from "@mui/material";
import {
  compareCurrencyAmounts,
  getCurrencyDifference,
} from "@/framework/currencyHelpers";
import AccountingPeriodHeader from "@/accounting-periods/workspace/AccountingPeriodHeader";
import AccountingPeriodPlanCard from "@/accounting-periods/workspace/AccountingPeriodPlanCard";
import AccountingPeriodProgressCard from "@/accounting-periods/workspace/AccountingPeriodProgressCard";
import AttentionList from "@/framework/view/AttentionList";
import BalanceStatCards from "@/accounts/BalanceStatCards";
import ExpectedIncomeSourcesFrame from "@/accounting-periods/workspace/ExpectedIncomeSourcesFrame";
import type { JSX } from "react";
import PageLayout from "@/framework/view/PageLayout";
import PeriodEntityStatus from "@/accounting-periods/workspace/periodEntityStatus";
import SignedChange from "@/framework/view/SignedChange";
import UrlTabs from "@/framework/view/UrlTabs";
import createApiClient from "@/framework/data/createApiClient";
import { expectedIncomeSourcesId } from "@/accounting-periods/workspace/helpers";
import fillHeightSx from "@/framework/view/fillHeightSx";
import getPeriodAttention from "@/accounting-periods/workspace/getPeriodAttention";
import { isDebtAccountType } from "@/accounts/helpers";
import parseEnumValue from "@/framework/data/parseEnumValue";
import { redirect } from "next/navigation";
import routes from "@/accounting-periods/routes";
import unwrapApiResponse from "@/framework/data/unwrapApiResponse";

/**
 * Search parameters supported by an accounting period's page.
 */
interface AccountingPeriodSearchParams {
  returnUrl?: string;
  tab?: string;
}

/**
 * Converts a goal's ending balance status to the status shown in the period's tables.
 */
const getEntityStatus = function (status?: string): PeriodEntityStatus {
  return (
    parseEnumValue(PeriodEntityStatus, status ?? "") ??
    PeriodEntityStatus.NoGoal
  );
};

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
      <UrlTabs
        label="Accounting period views"
        paramName="tab"
        tabs={[
          {
            value: "summary",
            label: "Summary",
            content: (
              <Stack spacing={2}>
                <BalanceStatCards
                  openingBalance={detail.accountOpeningBalance}
                  closingBalance={detail.accountClosingBalance}
                  unassignedBalance={
                    detail.fundClosingBalance.totalUnassignedBalance
                  }
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
                <AttentionList descriptions={getPeriodAttention(detail)} />
                <Box
                  sx={{
                    display: "grid",
                    gap: 2,
                    gridTemplateColumns: {
                      xs: "minmax(0, 1fr)",
                      lg: "2fr 1fr",
                    },
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
                      unusedContributions={detail.funds.flatMap((item) => {
                        const amount =
                          item.goalProgress?.contribution
                            ?.amountReducedByMaximumEndingBalance ?? 0;
                        return compareCurrencyAmounts(amount, 0) > 0
                          ? [
                              {
                                id: item.fund.id,
                                name: item.fund.name,
                                amount,
                                href: routes.fundDetail(
                                  period.id,
                                  item.fund.id,
                                  {},
                                ),
                              },
                            ]
                          : [];
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
              </Stack>
            ),
          },
          {
            value: "accounts",
            label: "Accounts",
            controls: <AccountingPeriodEntityControls />,
            content: (
              <AccountingPeriodEntityList
                entities={detail.accounts.map((item) => ({
                  id: item.account.id,
                  name: item.account.name,
                  href: routes.accountDetail(period.id, item.account.id, {
                    periodTab: "accounts",
                  }),
                  openingBalance: item.openingBalance,
                  closingBalance: item.closingBalance,
                  minimum:
                    item.goalProgress?.endingBalance.minimumBalance ?? null,
                  maximum:
                    item.goalProgress?.endingBalance.maximumBalance ?? null,
                  status: getEntityStatus(
                    item.goalProgress?.endingBalance.status,
                  ),
                  isIncreaseGood: !isDebtAccountType(item.account.type),
                  contribution: null,
                }))}
              />
            ),
          },
          {
            value: "funds",
            label: "Funds",
            controls: <AccountingPeriodEntityControls />,
            content: (
              <AccountingPeriodEntityList
                entities={detail.funds.map((item) => ({
                  id: item.fund.id,
                  name: item.fund.name,
                  href: routes.fundDetail(period.id, item.fund.id, {
                    periodTab: "funds",
                  }),
                  openingBalance: item.openingBalance,
                  closingBalance: item.closingBalance,
                  minimum:
                    item.goalProgress?.endingBalance.minimumBalance ?? null,
                  maximum:
                    item.goalProgress?.endingBalance.maximumBalance ?? null,
                  status: getEntityStatus(
                    item.goalProgress?.endingBalance.status,
                  ),
                  isIncreaseGood: true,
                  contribution:
                    item.goalProgress?.contribution === null ||
                    item.goalProgress?.contribution === undefined
                      ? null
                      : {
                          assigned:
                            item.goalProgress.contribution.assignedAmount,
                          expected:
                            item.goalProgress.contribution.expectedAmount,
                        },
                }))}
              />
            ),
          },
        ]}
      />
    </PageLayout>
  );
};

export type { AccountingPeriodSearchParams };
export default AccountingPeriodWorkspaceDetailPage;
