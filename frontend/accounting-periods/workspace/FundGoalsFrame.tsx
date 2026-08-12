"use client";

import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import type { FundGoalWithProgress } from "@/fund-goals/types";
import type { JSX } from "react";
import ListFrame from "@/framework/listframe/ListFrame";
import { formatCurrency } from "@/framework/currencyHelpers";

/**
 * Props for the FundGoalsFrame component.
 */
interface FundGoalsFrameProps {
  readonly goals: readonly FundGoalWithProgress[];
}

/** 
 * Displays fund goals for the accounting period. 
 */
const FundGoalsFrame = function ({ goals }: FundGoalsFrameProps): JSX.Element {
  const columns: ColumnDefinition<FundGoalWithProgress>[] = [
    {
      name: "fund",
      headerContent: "Fund",
      getBodyContent: (goal) => goal.fund.name,
      mobilePrimary: true,
    },
    {
      name: "regularContribution",
      headerContent: "Regular Contribution",
      getBodyContent: (goal) => formatCurrency(goal.regularContribution ?? 0),
      alignment: "right",
    },
    {
      name: "targetEndingBalance",
      headerContent: "Target Ending Balance",
      getBodyContent: (goal) =>
        goal.targetEndingBalance === null ||
        typeof goal.targetEndingBalance === "undefined"
          ? "—"
          : formatCurrency(goal.targetEndingBalance),
      alignment: "right",
    },
    {
      name: "status",
      headerContent: "Available",
      getBodyContent: (goal) =>
        goal.progress.availableBalance.isSatisfied
          ? "On track"
          : "Needs attention",
    },
  ];
  return (
    <ListFrame
      title="Fund Goals for This Period"
      columns={columns}
      getId={(goal) => goal.id}
      data={goals}
      totalCount={goals.length}
      pageParamName="fundGoalPage"
      initialEmptyState={{
        title: "No fund goals",
        description: "There are no fund goals configured for this period.",
        action: null,
      }}
    />
  );
};

export default FundGoalsFrame;
