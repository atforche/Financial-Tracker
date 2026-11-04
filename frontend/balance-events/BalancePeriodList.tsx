"use client";

import {
  formatCurrency,
  getCurrencyDifference,
} from "@/framework/currencyHelpers";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import type { JSX } from "react";
import Link from "next/link";
import ListFrame from "@/framework/listframe/ListFrame";
import SignedChange from "@/framework/view/SignedChange";
import accountingPeriodRoutes from "@/accounting-periods/routes";

/**
 * One accounting period's balances for an account or fund.
 */
interface BalancePeriod {
  readonly id: string;
  readonly name: string;
  readonly openingBalance: number;
  readonly closingBalance: number;
}

const columns: ColumnDefinition<BalancePeriod>[] = [
  {
    name: "period",
    headerContent: "Period",
    mobilePrimary: true,
    getBodyContent: (period) => (
      <Link href={accountingPeriodRoutes.workspaceDetail(period.id, {})}>
        {period.name}
      </Link>
    ),
  },
  {
    name: "opening",
    headerContent: "Opening",
    alignment: "right",
    getBodyContent: (period) => formatCurrency(period.openingBalance),
  },
  {
    name: "closing",
    headerContent: "Closing",
    alignment: "right",
    getBodyContent: (period) => formatCurrency(period.closingBalance),
  },
  {
    name: "change",
    headerContent: "Change",
    alignment: "right",
    getBodyContent: (period) => (
      <SignedChange
        amount={getCurrencyDifference(
          period.closingBalance,
          period.openingBalance,
        )}
      />
    ),
  },
];

/**
 * Lists how an account or fund's balance changed in each accounting period of a range.
 */
const BalancePeriodList = function ({
  periods,
}: {
  readonly periods: readonly BalancePeriod[];
}): JSX.Element {
  return (
    <ListFrame<BalancePeriod>
      title="Periods"
      columns={columns}
      getId={(period) => period.id}
      data={periods}
      totalCount={periods.length}
      pageParamName="historyPeriodPage"
    />
  );
};

export default BalancePeriodList;
