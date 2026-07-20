"use client";
import type { FundPlan, FundPlanBalanceEvent } from "@/fund-plans/types";
import ConstrainedContent from "@/framework/view/ConstrainedContent";
import FundPlanBalanceEventsFrame from "@/fund-plans/workspace/FundPlanBalanceEventsFrame";
import FundPlanContextFrame from "@/fund-plans/workspace/FundPlanContextFrame";
import type { JSX } from "react";
import PageLayout from "@/framework/view/PageLayout";

/**
 * Props for the ViewFundPlanForm component.
 */
interface ViewFundPlanFormProps {
  readonly fundPlan: FundPlan;
  readonly redirectUrl: string;
  readonly recentBalanceEvents: FundPlanBalanceEvent[];
  readonly recentBalanceEventCount: number;
  readonly addTransactionHref: string;
  readonly accountingPeriodId: string;
  readonly fundId: string;
}

/**
 * Displays Funding Plan details, recent events, and an edit action.
 */
const ViewFundPlanForm = function (props: ViewFundPlanFormProps): JSX.Element {
  return (
    <ConstrainedContent maxWidth={1200}>
      <PageLayout>
        <FundPlanContextFrame
          fundPlan={props.fundPlan}
          redirectUrl={props.redirectUrl}
        />
        <FundPlanBalanceEventsFrame
          data={props.recentBalanceEvents}
          totalCount={props.recentBalanceEventCount}
          addTransactionHref={props.addTransactionHref}
          accountingPeriodId={props.accountingPeriodId}
          fundId={props.fundId}
        />
      </PageLayout>
    </ConstrainedContent>
  );
};
export default ViewFundPlanForm;
