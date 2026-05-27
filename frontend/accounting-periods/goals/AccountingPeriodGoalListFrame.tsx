"use client";

import type { AccountingPeriod } from "@/accounting-periods/types";
import type { AccountingPeriodViewSearchParams } from "@/accounting-periods/AccountingPeriodView";
import type { Goal } from "@/goals/types";
import GoalListFrame from "@/goals/GoalListFrame";
import type { JSX } from "react";
import nameof from "@/framework/data/nameof";

/**
 * Props for the AccountingPeriodGoalListFrame component.
 */
interface AccountingPeriodGoalListFrameProps {
  readonly accountingPeriod: AccountingPeriod;
  readonly data: Goal[] | null;
  readonly totalCount: number | null;
}

/**
 * Component that displays the list of goals associated with an accounting period.
 */
const AccountingPeriodGoalListFrame = function ({
  accountingPeriod,
  data,
  totalCount,
}: AccountingPeriodGoalListFrameProps): JSX.Element {
  const searchParamName = nameof<AccountingPeriodViewSearchParams>("search");
  const pageParamName = nameof<AccountingPeriodViewSearchParams>("page");

  return (
    <GoalListFrame
      accountingPeriod={accountingPeriod}
      data={data ?? null}
      totalCount={totalCount ?? null}
      searchParamName={searchParamName}
      sortParamName={nameof<AccountingPeriodViewSearchParams>("goalSort")}
      pageParamName={pageParamName}
    />
  );
};

export default AccountingPeriodGoalListFrame;
