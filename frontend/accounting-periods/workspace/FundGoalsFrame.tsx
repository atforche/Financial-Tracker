"use client";

import { FundGoalSort, type FundGoalWithProgress } from "@/fund-goals/types";
import { useRouter, useSearchParams } from "next/navigation";
import type { AccountingPeriodWorkspaceSearchParams } from "@/accounting-periods/workspace/AccountingPeriodWorkspace";
import ArrowForwardOutlined from "@mui/icons-material/ArrowForwardOutlined";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import type { JSX } from "react";
import ListFrame from "@/framework/listframe/ListFrame";
import ListFrameActionButton from "@/framework/listframe/ListFrameActionButton";
import createColumnSortProps from "@/framework/listframe/createColumnSortProps";
import { formatCurrency } from "@/framework/currencyHelpers";
import fundGoalRoutes from "@/fund-goals/routes";
import parseEnumValue from "@/framework/data/parseEnumValue";
import propertyName from "@/framework/data/propertyName";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";

interface FundGoalsFrameProps {
  readonly goals: readonly FundGoalWithProgress[];
  readonly totalCount: number;
  readonly accountingPeriodId: string;
  readonly returnUrl: string;
}

/** Displays fund goals for the accounting period. */
const FundGoalsFrame = function ({
  goals,
  totalCount,
  accountingPeriodId,
  returnUrl,
}: FundGoalsFrameProps): JSX.Element {
  const router = useRouter();
  const searchParams = useSearchParams();
  const sortParamName =
    propertyName<AccountingPeriodWorkspaceSearchParams>("fundGoalSort");
  const pageParamName =
    propertyName<AccountingPeriodWorkspaceSearchParams>("fundGoalPage");
  const updateParams = useSearchParamUpdater([pageParamName]);
  const currentSort = parseEnumValue(
    FundGoalSort,
    searchParams.get(sortParamName) ?? "",
  );
  const setSort = function (sort: FundGoalSort | null): void {
    updateParams((params) => {
      if (sort === null) {
        params.delete(sortParamName);
      } else {
        params.set(sortParamName, sort);
      }
    });
  };
  const getSortProps = createColumnSortProps(currentSort, setSort);
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
      ...getSortProps(FundGoalSort.Fund, FundGoalSort.FundDescending),
    },
    {
      name: "plannedMonthlyContribution",
      headerContent: "Monthly Contribution",
      getBodyContent: (goal) =>
        typeof goal.plannedMonthlyContribution === "number"
          ? formatCurrency(goal.plannedMonthlyContribution)
          : "Not set",
      ...getSortProps(
        FundGoalSort.PlannedMonthlyContribution,
        FundGoalSort.PlannedMonthlyContributionDescending,
      ),
      alignment: "right",
      minWidth: 200,
      maxWidth: 200,
    },
    {
      name: "minimumEndingBalance",
      headerContent: "Minimum Balance",
      getBodyContent: (goal) => formatCurrency(goal.minimumEndingBalance),
      ...getSortProps(
        FundGoalSort.MinimumEndingBalance,
        FundGoalSort.MinimumEndingBalanceDescending,
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
        FundGoalSort.MaximumEndingBalance,
        FundGoalSort.MaximumEndingBalanceDescending,
      ),
      alignment: "right",
    },
    {
      name: "actions",
      headerContent: "",
      getBodyContent: (goal) => (
        <ListFrameActionButton
          ariaLabel={`View ${goal.fund.name} Fund Goal`}
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
      pageParamName={pageParamName}
      onRowClick={openFundGoal}
      initialEmptyState={{
        title: "No Fund Goals",
        description: "There are no fund goals configured for this period.",
        action: null,
      }}
    />
  );
};

export default FundGoalsFrame;
