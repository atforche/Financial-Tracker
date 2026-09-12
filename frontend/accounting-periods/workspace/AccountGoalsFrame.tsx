"use client";

import {
  AccountGoalSort,
  type AccountGoalWithProgress,
} from "@/account-goals/types";
import { useRouter, useSearchParams } from "next/navigation";
import type { AccountingPeriodWorkspaceSearchParams } from "@/accounting-periods/workspace/AccountingPeriodWorkspace";
import ArrowForwardOutlined from "@mui/icons-material/ArrowForwardOutlined";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import type { JSX } from "react";
import ListFrame from "@/framework/listframe/ListFrame";
import ListFrameActionButton from "@/framework/listframe/ListFrameActionButton";
import accountGoalRoutes from "@/account-goals/routes";
import createColumnSortProps from "@/framework/listframe/createColumnSortProps";
import { formatCurrency } from "@/framework/currencyHelpers";
import parseEnumValue from "@/framework/data/parseEnumValue";
import propertyName from "@/framework/data/propertyName";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";

interface AccountGoalsFrameProps {
  readonly goals: readonly AccountGoalWithProgress[];
  readonly totalCount: number;
  readonly accountingPeriodId: string;
  readonly returnUrl: string;
}

/** Displays account goals for the accounting period. */
const AccountGoalsFrame = function ({
  goals,
  totalCount,
  accountingPeriodId,
  returnUrl,
}: AccountGoalsFrameProps): JSX.Element {
  const router = useRouter();
  const searchParams = useSearchParams();
  const sortParamName =
    propertyName<AccountingPeriodWorkspaceSearchParams>("accountGoalSort");
  const pageParamName =
    propertyName<AccountingPeriodWorkspaceSearchParams>("accountGoalPage");
  const updateParams = useSearchParamUpdater([pageParamName]);
  const currentSort = parseEnumValue(
    AccountGoalSort,
    searchParams.get(sortParamName) ?? "",
  );
  const setSort = function (sort: AccountGoalSort | null): void {
    updateParams((params) => {
      if (sort === null) {
        params.delete(sortParamName);
      } else {
        params.set(sortParamName, sort);
      }
    });
  };
  const getSortProps = createColumnSortProps(currentSort, setSort);
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
      ...getSortProps(
        AccountGoalSort.Account,
        AccountGoalSort.AccountDescending,
      ),
    },
    {
      name: "minimumEndingBalance",
      headerContent: "Minimum Balance",
      getBodyContent: (goal) => formatCurrency(goal.minimumEndingBalance),
      ...getSortProps(
        AccountGoalSort.MinimumEndingBalance,
        AccountGoalSort.MinimumEndingBalanceDescending,
      ),
      alignment: "right",
    },
    {
      name: "maximumEndingBalance",
      headerContent: "Maximum Balance",
      getBodyContent: (goal) =>
        typeof goal.maximumEndingBalance === "number"
          ? formatCurrency(goal.maximumEndingBalance)
          : "Not set",
      ...getSortProps(
        AccountGoalSort.MaximumEndingBalance,
        AccountGoalSort.MaximumEndingBalanceDescending,
      ),
      alignment: "right",
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
      pageParamName={pageParamName}
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
