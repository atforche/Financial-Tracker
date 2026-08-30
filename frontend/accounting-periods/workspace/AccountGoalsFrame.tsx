"use client";

import type { AccountGoalWithProgress } from "@/account-goals/types";
import ArrowForwardOutlined from "@mui/icons-material/ArrowForwardOutlined";
import Chip from "@mui/material/Chip";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import type { JSX } from "react";
import ListFrame from "@/framework/listframe/ListFrame";
import ListFrameActionButton from "@/framework/listframe/ListFrameActionButton";
import accountGoalRoutes from "@/account-goals/routes";
import { formatCurrency } from "@/framework/currencyHelpers";
import { useRouter } from "next/navigation";

/**
 * Props for the AccountGoalsFrame component.
 */
interface AccountGoalsFrameProps {
  readonly goals: readonly AccountGoalWithProgress[];
  readonly totalCount: number;
  readonly accountingPeriodId: string;
  readonly returnUrl: string;
}

/**
 * Displays account goals for the accounting period.
 */
const AccountGoalsFrame = function ({
  goals,
  totalCount,
  accountingPeriodId,
  returnUrl,
}: AccountGoalsFrameProps): JSX.Element {
  const router = useRouter();
  const openAccountGoal = function (goal: AccountGoalWithProgress): void {
    router.push(
      accountGoalRoutes.workspaceDetail(goal.account.id, {
        accountingPeriodId,
        returnUrl,
      }),
    );
  };
  const columns: ColumnDefinition<AccountGoalWithProgress>[] = [
    {
      name: "account",
      headerContent: "Account",
      getBodyContent: (goal) => goal.account.name,
      mobilePrimary: true,
    },
    {
      name: "endingBalance",
      headerContent: "Ending Balance",
      getBodyContent: (goal) =>
        formatCurrency(goal.progress.positiveBalance.currentBalance),
      alignment: "right",
    },
    {
      name: "status",
      headerContent: "Status",
      getBodyContent: (goal) => (
        <Chip
          label={goal.progress.isSatisfied ? "Achieved" : "Needs attention"}
          color={goal.progress.isSatisfied ? "success" : "warning"}
          size="small"
        />
      ),
    },
    {
      name: "actions",
      headerContent: "",
      getBodyContent: (goal) => (
        <ListFrameActionButton
          ariaLabel={`View ${goal.account.name} Account Goal`}
          onClick={(event) => {
            event.stopPropagation();
            openAccountGoal(goal);
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
      title="Account Goals"
      columns={columns}
      getId={(goal) => goal.id}
      data={goals}
      totalCount={totalCount}
      pageParamName="accountGoalPage"
      onRowClick={openAccountGoal}
      initialEmptyState={{
        title: "No Account Goals",
        description: "There are no account goals configured for this period.",
        action: null,
      }}
    />
  );
};

export default AccountGoalsFrame;
