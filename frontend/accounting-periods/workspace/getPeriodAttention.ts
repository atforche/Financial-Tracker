import type { AttentionItem } from "@/framework/view/AttentionList";
import PeriodEntityStatus from "@/accounting-periods/workspace/periodEntityStatus";
import type { Route } from "next";
import type { components } from "@/framework/data/api";
import { formatCurrency } from "@/framework/currencyHelpers";
import routes from "@/accounting-periods/routes";

const maximumListedItems = 3;

/**
 * Builds one attention line for a class of problem, listing its largest items.
 */
const describe = function (
  title: string,
  suffix: string,
  entries: readonly { readonly name: string; readonly amount: number }[],
  href: Route,
): AttentionItem[] {
  if (entries.length === 0) {
    return [];
  }
  const sorted = [...entries].sort((a, b) => b.amount - a.amount);
  return [
    {
      title: `${entries.length.toString()} ${entries.length === 1 ? "goal" : "goals"} ${title}`,
      amount: `${formatCurrency(sorted.reduce((sum, entry) => sum + entry.amount, 0))} ${suffix}`,
      items: sorted.slice(0, maximumListedItems).map((entry) => ({
        text: entry.name,
        amount: formatCurrency(entry.amount),
      })),
      remainingItemCount: Math.max(sorted.length - maximumListedItems, 0),
      facts: [],
      href,
    },
  ];
};

/**
 * Lists what needs attention in an accounting period, with at most one line for each class of problem.
 */
const getPeriodAttention = function (
  detail: components["schemas"]["AccountingPeriodDetailModel"],
): AttentionItem[] {
  const periodId = detail.accountingPeriod.id;
  const goals = [
    ...detail.funds.map((item) => ({
      name: item.fund.name,
      tab: "funds",
      ending: item.goalProgress?.endingBalance,
    })),
    ...detail.accounts.map((item) => ({
      name: item.account.name,
      tab: "accounts",
      ending: item.goalProgress?.endingBalance,
    })),
  ];
  const describeStatus = function (
    status: PeriodEntityStatus,
    title: string,
    suffix: string,
    getAmount: (
      ending: NonNullable<(typeof goals)[number]["ending"]>,
    ) => number,
  ): AttentionItem[] {
    const matches = goals.flatMap(({ name, tab, ending }) =>
      ending?.status.toString() === status
        ? [{ name, tab, amount: getAmount(ending) }]
        : [],
    );
    return describe(
      title,
      suffix,
      matches,
      routes.workspaceDetail(periodId, {
        tab: matches[0]?.tab ?? "funds",
        status,
      }),
    );
  };
  return [
    ...describeStatus(
      PeriodEntityStatus.BelowMinimum,
      "below minimum",
      "short",
      (ending) => ending.amountBelowMinimum,
    ),
    ...describeStatus(
      PeriodEntityStatus.AboveMaximum,
      "above maximum",
      "over",
      (ending) => ending.amountAboveMaximum,
    ),
    ...describe(
      "not fully funded",
      "remaining",
      detail.funds.flatMap((item) => {
        const contribution = item.goalProgress?.contribution;
        return contribution === null ||
          contribution === undefined ||
          contribution.isSatisfied
          ? []
          : [{ name: item.fund.name, amount: contribution.remainingAmount }];
      }),
      routes.workspaceDetail(periodId, { tab: "funds" }),
    ),
  ];
};

export default getPeriodAttention;
