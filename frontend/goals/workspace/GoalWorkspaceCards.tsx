"use client";

import type { AssignmentGoal, SpendingGoal } from "@/goals/types";
import { Stack, Typography } from "@mui/material";
import type { AccountingPeriod } from "@/accounting-periods/types";
import GoalProgress from "@/goals/workspace/GoalProgress";
import type { GoalWorkspaceSearchParams } from "@/goals/workspace/GoalWorkspace";
import type { JSX } from "react";
import ResponsiveGrid from "@/framework/view/ResponsiveGrid";
import WorkspaceCard from "@/framework/view/WorkspaceCard";
import nameof from "@/framework/data/nameof";
import routes from "@/goals/routes";
import { useSearchParams } from "next/navigation";

/**
 * Props for the GoalWorkspaceCards component.
 */
interface GoalWorkspaceCardsProps {
  readonly accountingPeriod: AccountingPeriod | null;
  readonly assignmentGoals: AssignmentGoal[];
  readonly spendingGoals: SpendingGoal[];
}

/**
 * Displays paired goal progress as navigable workspace cards.
 */
const GoalWorkspaceCards = function ({
  accountingPeriod,
  assignmentGoals,
  spendingGoals,
}: GoalWorkspaceCardsProps): JSX.Element {
  const searchParams = useSearchParams();
  const search = (
    searchParams.get(nameof<GoalWorkspaceSearchParams>("search")) ?? ""
  )
    .trim()
    .toLowerCase();
  const funds = Array.from(
    new Map(
      [...assignmentGoals, ...spendingGoals].map((goal) => [
        goal.fund.id,
        goal.fund,
      ]),
    ).values(),
  ).filter((fund) => fund.name.toLowerCase().includes(search));

  if (funds.length === 0) {
    return (
      <Typography color="text.secondary">
        No goals match the selected accounting period and search filters.
      </Typography>
    );
  }

  return (
    <ResponsiveGrid minimumColumnWidth={340} spacing={2}>
      {funds.map((fund) => {
        const assignmentGoal =
          assignmentGoals.find((goal) => goal.fund.id === fund.id) ?? null;
        const spendingGoal =
          spendingGoals.find((goal) => goal.fund.id === fund.id) ?? null;
        const detailSearchParams: GoalWorkspaceSearchParams = {
          ...(accountingPeriod === null
            ? {}
            : { accountingPeriodId: accountingPeriod.id }),
          ...(search === "" ? {} : { search }),
        };
        const fundIds = searchParams.getAll(
          nameof<GoalWorkspaceSearchParams>("fundIds"),
        );
        if (fundIds.length > 0) {
          detailSearchParams.fundIds = fundIds;
        }

        let goalsMet = 0;
        if (assignmentGoal?.isGoalMet === true) {
          goalsMet += 1;
        }
        if (spendingGoal?.isGoalMet === true) {
          goalsMet += 1;
        }
        return (
          <WorkspaceCard
            key={fund.id}
            title={fund.name}
            href={routes.workspaceDetail(fund.id, detailSearchParams)}
            color={
              goalsMet === 2 ? "success" : goalsMet === 1 ? "warning" : "error"
            }
          >
            <Stack spacing={2.25}>
              <Typography variant="body2" color="text.secondary">
                {accountingPeriod?.name ?? "No accounting period"}
              </Typography>
              <GoalProgress
                label="Remaining to assign"
                progress={assignmentGoal}
              />
              <GoalProgress
                label="Remaining to spend"
                progress={spendingGoal}
              />
            </Stack>
          </WorkspaceCard>
        );
      })}
    </ResponsiveGrid>
  );
};

export default GoalWorkspaceCards;
