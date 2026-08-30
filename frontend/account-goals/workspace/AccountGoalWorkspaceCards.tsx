"use client";

import type { AccountGoalWithProgress } from "@/account-goals/types";
import AccountGoalWorkspaceCard from "@/account-goals/workspace/AccountGoalWorkspaceCard";
import type { AccountGoalWorkspaceSearchParams } from "@/account-goals/workspace/types";
import type { AccountingPeriod } from "@/accounting-periods/types";
import CardResponsiveGrid from "@/framework/view/CardResponsiveGrid";
import type { JSX } from "react";
import { Typography } from "@mui/material";
import propertyName from "@/framework/data/propertyName";
import routes from "@/account-goals/routes";
import { useSearchParams } from "next/navigation";

interface AccountGoalWorkspaceCardsProps {
  readonly accountingPeriod: AccountingPeriod | null;
  readonly accountGoals: readonly AccountGoalWithProgress[];
}

/**
 * Displays Account Goal progress as navigable workspace cards.
 */
const AccountGoalWorkspaceCards = function ({
  accountingPeriod,
  accountGoals,
}: AccountGoalWorkspaceCardsProps): JSX.Element {
  const searchParams = useSearchParams();
  if (accountGoals.length === 0) {
    return (
      <Typography color="text.secondary">
        No Account Goals match the selected accounting period and account
        filters.
      </Typography>
    );
  }
  const accountIdsParamName =
    propertyName<AccountGoalWorkspaceSearchParams>("accountIds");
  const selectedAccountIds = searchParams.getAll(accountIdsParamName);
  return (
    <CardResponsiveGrid minimumColumnWidth={340} spacing={2}>
      {accountGoals.map((accountGoal) => (
        <AccountGoalWorkspaceCard
          key={accountGoal.id}
          accountingPeriod={accountingPeriod}
          accountGoal={accountGoal}
          detailHref={routes.workspaceDetail(accountGoal.account.id, {
            ...(accountingPeriod
              ? { accountingPeriodId: accountingPeriod.id }
              : {}),
            ...(selectedAccountIds.length
              ? { accountIds: selectedAccountIds }
              : {}),
          })}
        />
      ))}
    </CardResponsiveGrid>
  );
};

export default AccountGoalWorkspaceCards;
