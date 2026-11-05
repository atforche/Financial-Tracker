import {
  formatCurrency,
  formatSignedCurrency,
  getCurrencyTotal,
} from "@/framework/currencyHelpers";
import type { AttentionItem } from "@/framework/view/AttentionList";
import PeriodEntityStatus from "@/accounting-periods/workspace/periodEntityStatus";
import type { Route } from "next";
import type { components } from "@/framework/data/api";
import routes from "@/accounting-periods/routes";

const maximumListedItems = 3;

/**
 * An account or fund and how far it ended from its goal.
 */
interface GoalEntry {
  readonly name: string;
  readonly ending:
    | {
        readonly status: string;
        readonly amountBelowMinimum: number;
        readonly amountAboveMaximum: number;
      }
    | undefined;
}

/**
 * Lists an attention line's largest items first, leaving the rest as a count.
 */
const summarizeItems = function (
  items: readonly { readonly text: string; readonly size: number }[],
  getAmount: (size: number) => string,
): Pick<AttentionItem, "items" | "remainingItemCount"> {
  return {
    items: [...items]
      .sort((first, second) => Math.abs(second.size) - Math.abs(first.size))
      .slice(0, maximumListedItems)
      .map(({ text, size }) => ({ text, amount: getAmount(size) })),
    remainingItemCount: Math.max(items.length - maximumListedItems, 0),
  };
};

/**
 * Describes the goals of one kind that ended outside their range, linking to the table that lists them.
 */
const describeOutOfRange = function (
  kind: "account" | "fund",
  entries: readonly GoalEntry[],
  href: Route,
): AttentionItem[] {
  const misses = entries.flatMap(({ name, ending }) => {
    if (ending?.status === PeriodEntityStatus.BelowMinimum) {
      return [{ text: name, size: -ending.amountBelowMinimum }];
    }
    return ending?.status === PeriodEntityStatus.AboveMaximum
      ? [{ text: name, size: ending.amountAboveMaximum }]
      : [];
  });
  if (misses.length === 0) {
    return [];
  }
  const belowCount = misses.filter(({ size }) => size < 0).length;
  const aboveCount = misses.length - belowCount;
  return [
    {
      title: `${misses.length.toString()} ${kind} ${misses.length === 1 ? "goal" : "goals"} outside range`,
      amount: `${formatCurrency(getCurrencyTotal(misses.map(({ size }) => Math.abs(size))))} off goal`,
      ...summarizeItems(misses, formatSignedCurrency),
      facts: [
        [
          ...(belowCount > 0 ? [`${belowCount.toString()} below minimum`] : []),
          ...(aboveCount > 0 ? [`${aboveCount.toString()} above maximum`] : []),
        ].join(" · "),
      ],
      href,
    },
  ];
};

/**
 * Lists what needs attention in an accounting period, with Account Goals and Fund Goals kept apart so that each line
 * links to the one table holding everything it describes.
 */
const getPeriodAttention = function (
  detail: components["schemas"]["AccountingPeriodDetailModel"],
): AttentionItem[] {
  const periodId = detail.accountingPeriod.id;
  const fundsHref = routes.workspaceDetail(periodId, { tab: "funds" });
  const unfunded = detail.funds.flatMap((item) => {
    const contribution = item.goalProgress?.contribution;
    return contribution === null ||
      contribution === undefined ||
      contribution.isSatisfied
      ? []
      : [{ text: item.fund.name, size: contribution.remainingAmount }];
  });
  return [
    ...describeOutOfRange(
      "fund",
      detail.funds.map((item) => ({
        name: item.fund.name,
        ending: item.goalProgress?.endingBalance,
      })),
      fundsHref,
    ),
    ...describeOutOfRange(
      "account",
      detail.accounts.map((item) => ({
        name: item.account.name,
        ending: item.goalProgress?.endingBalance,
      })),
      routes.workspaceDetail(periodId, { tab: "accounts" }),
    ),
    ...(unfunded.length === 0
      ? []
      : [
          {
            title: `${unfunded.length.toString()} ${unfunded.length === 1 ? "fund goal" : "fund goals"} not fully funded`,
            amount: `${formatCurrency(getCurrencyTotal(unfunded.map(({ size }) => size)))} remaining`,
            ...summarizeItems(unfunded, formatCurrency),
            facts: [],
            href: fundsHref,
          },
        ]),
  ];
};

export default getPeriodAttention;
