"use client";

import ArrowForwardOutlined from "@mui/icons-material/ArrowForwardOutlined";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import type { FundGoalWithProgress } from "@/fund-goals/types";
import type { JSX } from "react";
import ListFrame from "@/framework/listframe/ListFrame";
import ListFrameActionButton from "@/framework/listframe/ListFrameActionButton";
import { formatCurrency } from "@/framework/currencyHelpers";
import fundGoalRoutes from "@/fund-goals/routes";
import { useRouter } from "next/navigation";

/**
 * Props for the FundGoalsFrame component.
 */
interface FundGoalsFrameProps {
  readonly goals: readonly FundGoalWithProgress[];
  readonly totalCount: number;
  readonly accountingPeriodId: string;
  readonly returnUrl: string;
}

/**
 * Displays fund goals for the accounting period.
 */
const FundGoalsFrame = function ({
  goals,
  totalCount,
  accountingPeriodId,
  returnUrl,
}: FundGoalsFrameProps): JSX.Element {
  const router = useRouter();
  const openFundGoal = function (goal: FundGoalWithProgress): void {
    router.push(
      fundGoalRoutes.workspaceDetail(goal.fund.id, {
        accountingPeriodId,
        returnUrl,
      }),
    );
  };
  const columns: ColumnDefinition<FundGoalWithProgress>[] = [
    {
      name: "fund",
      headerContent: "Fund",
      getBodyContent: (goal) => goal.fund.name,
      mobilePrimary: true,
    },
    {
      name: "expectedContributions",
      headerContent: "Expected Contributions",
      getBodyContent: (goal) =>
        formatCurrency(goal.progress.contribution?.targetAmount ?? 0),
      alignment: "right",
    },
    {
      name: "actions",
      headerContent: "",
      getBodyContent: (goal) => (
        <ListFrameActionButton
          ariaLabel={`View ${goal.fund.name} goal`}
          onClick={(event) => {
            event.stopPropagation();
            openFundGoal(goal);
          }}
        >
          <ArrowForwardOutlined fontSize="small" color="action" />
        </ListFrameActionButton>
      ),
      alignment: "right",
      minWidth: 52,
      maxWidth: 52,
    },
  ];
  return (
    <ListFrame
      title="Fund Goals"
      columns={columns}
      getId={(goal) => goal.id}
      data={goals}
      totalCount={totalCount}
      pageParamName="fundGoalPage"
      onRowClick={openFundGoal}
      initialEmptyState={{
        title: "No fund goals",
        description: "There are no fund goals configured for this period.",
        action: null,
      }}
    />
  );
};

export default FundGoalsFrame;
