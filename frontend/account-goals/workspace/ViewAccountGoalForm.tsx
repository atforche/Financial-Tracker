"use client";

import type { AccountGoal, AccountGoalProgress } from "@/account-goals/types";
import type { AccountBalanceEvent } from "@/accounts/types";
import AccountGoalBalanceEventsFrame from "@/account-goals/workspace/AccountGoalBalanceEventsFrame";
import AccountGoalContextFrame from "@/account-goals/workspace/AccountGoalContextFrame";
import AccountGoalEndingBalanceRange from "@/account-goals/workspace/AccountGoalEndingBalanceRange";
import type { BalanceTrendDateSummary } from "@/framework/charts/balanceTrendHelpers";
import ConstrainedContent from "@/framework/view/ConstrainedContent";
import Frame from "@/framework/view/Frame";
import type { JSX } from "react";
import PageLayout from "@/framework/view/PageLayout";
import RecentBalanceActivity from "@/balance-events/RecentBalanceActivity";
import type { Route } from "next";
import UrlTabs from "@/framework/view/UrlTabs";

interface ViewAccountGoalFormProps {
  readonly accountGoal: AccountGoal;
  readonly progress: AccountGoalProgress;
  readonly redirectUrl: string;
  readonly isReadOnly: boolean;
  readonly recentBalanceEvents: AccountBalanceEvent[];
  readonly recentBalanceEventCount: number;
  readonly recentActivityBalances: readonly BalanceTrendDateSummary[];
  readonly periodOpeningBalance: number;
  readonly trendsHref: Route;
  readonly addTransactionHref: string;
  readonly accountingPeriodId: string;
  readonly accountId: string;
}

/** Displays account goal details, progress, activity, and balance events. */
const ViewAccountGoalForm = function (
  props: ViewAccountGoalFormProps,
): JSX.Element {
  return (
    <ConstrainedContent maxWidth={1200}>
      <PageLayout>
        <AccountGoalContextFrame
          accountGoal={props.accountGoal}
          redirectUrl={props.redirectUrl}
          isReadOnly={props.isReadOnly}
        />
        <Frame title="Progress">
          <AccountGoalEndingBalanceRange
            endingBalance={props.progress.endingBalance}
          />
        </Frame>
        <UrlTabs
          label="Account Goal workspace views"
          paramName="tab"
          tabs={[
            {
              value: "transactions",
              label: "Transactions",
              content: (
                <AccountGoalBalanceEventsFrame
                  data={props.recentBalanceEvents}
                  totalCount={props.recentBalanceEventCount}
                  addTransactionHref={props.addTransactionHref}
                  accountingPeriodId={props.accountingPeriodId}
                  accountId={props.accountId}
                />
              ),
            },
            {
              value: "activity",
              label: "Activity",
              content: (
                <RecentBalanceActivity
                  summaryFirst
                  data={[] as AccountBalanceEvent[]}
                  dailyBalances={props.recentActivityBalances}
                  periodOpeningBalance={props.periodOpeningBalance}
                  trendsHref={props.trendsHref}
                  getPreviousBalance={(event) =>
                    event.previousBalance.postedBalance
                  }
                  getNewBalance={(event) => event.newBalance.postedBalance}
                  title="Accounting Period Activity"
                  balanceLabel="Account Balance"
                />
              ),
            },
          ]}
        />
      </PageLayout>
    </ConstrainedContent>
  );
};

export default ViewAccountGoalForm;
