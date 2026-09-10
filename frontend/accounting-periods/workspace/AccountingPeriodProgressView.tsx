import AccountBalanceSummaryCards from "@/accounts/AccountBalanceSummaryCards";
import AccountingPeriodProgressFlow from "@/accounting-periods/workspace/AccountingPeriodProgressFlow";
import type { AccountingPeriodWithBalance } from "@/accounting-periods/types";
import FundBalanceSummaryCards from "@/funds/FundBalanceSummaryCards";
import type { JSX } from "react";
import createApiClient from "@/framework/data/createApiClient";
import { getAccountTrendsSnapshot } from "@/accounts/trends/helpers";
import { getFundTrendsSnapshot } from "@/funds/trends/helpers";
import unwrapApiResponse from "@/framework/data/unwrapApiResponse";

interface AccountingPeriodProgressViewProps {
  readonly accountingPeriod: AccountingPeriodWithBalance;
}

/** Displays actual progress and balance movement for an accounting period. */
const AccountingPeriodProgressView = async function ({
  accountingPeriod,
}: AccountingPeriodProgressViewProps): Promise<JSX.Element> {
  const apiClient = await createApiClient();
  const accountingPeriodId = accountingPeriod.id;
  const [transactionsResponse, accountBalancesResponse, fundBalancesResponse] =
    await Promise.all([
      apiClient.GET("/accounting-periods/{accountingPeriodId}/transactions", {
        params: {
          path: { accountingPeriodId },
          query: { Limit: 1 },
        },
      }),
      apiClient.GET("/accounts/accounting-period-range", {
        params: {
          query: {
            "Range.Start": accountingPeriodId,
            "Range.End": accountingPeriodId,
            Limit: 1,
            Offset: 0,
          },
        },
      }),
      apiClient.GET("/funds/accounting-period-range", {
        params: {
          query: {
            "Range.Start": accountingPeriodId,
            "Range.End": accountingPeriodId,
            Limit: 1,
            Offset: 0,
          },
        },
      }),
    ]);
  const transactionSnapshot = unwrapApiResponse(
    transactionsResponse,
    "Failed to fetch accounting period transactions",
  );
  const accountBalances = unwrapApiResponse(
    accountBalancesResponse,
    "Failed to fetch accounting period account balances",
  );
  const fundBalances = unwrapApiResponse(
    fundBalancesResponse,
    "Failed to fetch accounting period fund balances",
  );
  const accountBalanceSnapshot = getAccountTrendsSnapshot(
    "AccountingPeriod",
    accountBalances.accountingPeriods,
    [],
  );
  const fundBalanceSnapshot = getFundTrendsSnapshot(
    "AccountingPeriod",
    fundBalances.accountingPeriods,
    [],
  );
  return (
    <>
      <AccountBalanceSummaryCards
        startingLabel={accountBalanceSnapshot.startLabel}
        endingLabel={accountBalanceSnapshot.endLabel}
        startingBalance={accountBalanceSnapshot.startingBalance}
        endingBalance={accountBalanceSnapshot.endingBalance}
        showLabels={false}
        titlePrefix="Account"
      />
      <FundBalanceSummaryCards
        startingLabel={fundBalanceSnapshot.startLabel}
        endingLabel={fundBalanceSnapshot.endLabel}
        startingBalance={fundBalanceSnapshot.startingBalance}
        endingBalance={fundBalanceSnapshot.endingBalance}
        showLabels={false}
        titlePrefix="Fund"
      />
      <AccountingPeriodProgressFlow
        expectedIncome={accountingPeriod.expectedIncome}
        actualIncome={transactionSnapshot.totalIncome}
        totalSpending={transactionSnapshot.totalSpending}
        expectedFundGoalContributions={
          accountingPeriod.expectedGoalContributions
        }
        actualFundGoalContributions={accountingPeriod.actualGoalContributions}
        actualExtraFundGoalContributions={
          accountingPeriod.actualExtraGoalContributions
        }
      />
    </>
  );
};

export default AccountingPeriodProgressView;
