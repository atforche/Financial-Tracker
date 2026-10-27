import {
  OverviewAttentionKindModel,
  OverviewHistoryRangeModel,
  type components,
} from "@/framework/data/api";
import {
  formatCurrency,
  formatSignedCurrency,
} from "@/framework/currencyHelpers";
import type { Route } from "next";
import type { ToggleButtonSelectorOption } from "@/framework/forms/ToggleButtonSelector";
import accountingPeriodRoutes from "@/accounting-periods/routes";
import dayjs from "dayjs";
import { formatShortDate } from "@/framework/dateHelpers";
import parseEnumValue from "@/framework/data/parseEnumValue";
import transactionRoutes from "@/transactions/routes";

/**
 * A group of related items needing attention.
 */
type OverviewAttentionGroup =
  components["schemas"]["OverviewAttentionGroupModel"];

/**
 * Search params for the overview page.
 */
interface OverviewSearchParams {
  history?: string;
}

/**
 * Options for choosing the length of the balance history.
 */
const overviewHistoryOptions: readonly ToggleButtonSelectorOption<OverviewHistoryRangeModel>[] =
  [
    { value: OverviewHistoryRangeModel.ThreeMonths, label: "3 months" },
    { value: OverviewHistoryRangeModel.OneYear, label: "1 year" },
    { value: OverviewHistoryRangeModel.FiveYears, label: "5 years" },
  ];

/**
 * Reads the balance history length from a search param, defaulting to three months.
 */
const parseOverviewHistoryRange = function (
  value: string | undefined,
): OverviewHistoryRangeModel {
  return (
    (typeof value === "string"
      ? parseEnumValue(OverviewHistoryRangeModel, value)
      : null) ?? OverviewHistoryRangeModel.ThreeMonths
  );
};

/**
 * Gets the first date of the balance history ending on the as-of date, matching the backend.
 */
const getOverviewHistoryStart = function (
  asOf: string,
  history: OverviewHistoryRangeModel,
): string {
  const end = dayjs(asOf);
  const start =
    history === OverviewHistoryRangeModel.ThreeMonths
      ? end.subtract(3, "month")
      : end.subtract(
          history === OverviewHistoryRangeModel.OneYear ? 1 : 5,
          "year",
        );
  return start.format("YYYY-MM-DD");
};

/**
 * Formats a count with its noun, adding an "s" when there is not exactly one.
 */
const pluralize = function (count: number, noun: string): string {
  return `${count} ${noun}${count === 1 ? "" : "s"}`;
};

/**
 * What the overview says about a group of items needing attention, and where to resolve it.
 */
interface OverviewAttentionDescription {
  readonly title: string;
  readonly amount: string;
  readonly detail: string;
  readonly actionLabel: string;
  readonly href: Route;
}

/**
 * Describes a group of items needing attention. Goal and income groups link to the current period's plan.
 */
const describeAttentionGroup = function (
  group: OverviewAttentionGroup,
  currentPeriodId: string | null,
): OverviewAttentionDescription {
  const getPlanHref = function (
    planTab: "fund-goals" | "income-sources",
  ): Route {
    return currentPeriodId === null
      ? accountingPeriodRoutes.workspace({})
      : accountingPeriodRoutes.workspacePlan(currentPeriodId, { planTab });
  };
  const listItems = function (
    formatItem: (item: OverviewAttentionGroup["topItems"][number]) => string,
    ...extra: string[]
  ): string {
    const remaining = group.itemCount - group.topItems.length;
    return [
      ...group.topItems.map(formatItem),
      ...(remaining > 0 ? [`+${remaining} more`] : []),
      ...extra,
    ].join(" · ");
  };
  const oldestDate =
    typeof group.oldestDate === "string"
      ? formatShortDate(new Date(`${group.oldestDate}T00:00:00`))
      : null;

  switch (group.kind) {
    case OverviewAttentionKindModel.GoalsBelowMinimum:
      return {
        title: `${pluralize(group.count, "goal")} below minimum`,
        amount: `${formatCurrency(group.amount)} short`,
        detail: listItems(
          (item) => `${item.name} ${formatCurrency(item.amount)}`,
        ),
        actionLabel: "Review",
        href: getPlanHref("fund-goals"),
      };
    case OverviewAttentionKindModel.GoalsBehind:
      return {
        title: `${pluralize(group.count, "goal")} behind`,
        amount: `${formatCurrency(group.amount)} to go`,
        detail: listItems(
          (item) => `${item.name} ${formatCurrency(item.amount)}`,
        ),
        actionLabel: "Review",
        href: getPlanHref("fund-goals"),
      };
    case OverviewAttentionKindModel.UnpostedTransactions:
      return {
        title: `${pluralize(group.count, "transaction")} not posted`,
        amount: formatSignedCurrency(group.amount),
        detail: listItems(
          (item) => `${item.name} (${item.count ?? 0})`,
          ...(oldestDate === null ? [] : [`oldest ${oldestDate}`]),
        ),
        actionLabel: "Post",
        href: transactionRoutes.workspace({}),
      };
    case OverviewAttentionKindModel.IncomeNotReceived:
    default:
      return {
        title: "Expected income not received",
        amount: `${formatCurrency(group.amount)} short`,
        detail: listItems(
          (item) => `${item.name} ${formatCurrency(item.amount)}`,
          ...(oldestDate === null ? [] : [`due since ${oldestDate}`]),
        ),
        actionLabel: "Review",
        href: getPlanHref("income-sources"),
      };
  }
};

export {
  describeAttentionGroup,
  getOverviewHistoryStart,
  overviewHistoryOptions,
  parseOverviewHistoryRange,
};
export type { OverviewAttentionGroup, OverviewSearchParams };
