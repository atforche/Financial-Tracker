import {
  OverviewAttentionKindModel,
  OverviewHistoryRangeModel,
  type components,
} from "@/framework/data/api";
import {
  formatCurrency,
  formatSignedCurrency,
} from "@/framework/currencyHelpers";
import type { AccountingPeriodPlanTab } from "@/accounting-periods/workspace/AccountingPeriodWorkspaceDetailPage";
import type { Route } from "next";
import type { ToggleButtonSelectorOption } from "@/framework/forms/ToggleButtonSelector";
import accountingPeriodRoutes from "@/accounting-periods/routes";
import { expectedIncomeSourcesId } from "@/accounting-periods/workspace/helpers";
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
 * Balance history length shown when the search params don't choose one.
 */
const defaultOverviewHistory = OverviewHistoryRangeModel.ThreeMonths;

/**
 * Gets the overview URL showing the provided balance history length. The default length is left out, so the URL
 * matches a direct visit to the overview.
 */
const getOverviewUrl = function (history: OverviewHistoryRangeModel): Route {
  return history === defaultOverviewHistory ? "/" : `/?history=${history}`;
};

/**
 * Reads the balance history length from a search param, falling back to the default.
 */
const parseOverviewHistoryRange = function (
  value: string | undefined,
): OverviewHistoryRangeModel {
  return (
    (typeof value === "string"
      ? parseEnumValue(OverviewHistoryRangeModel, value)
      : null) ?? defaultOverviewHistory
  );
};

/**
 * Formats a count with its noun, adding an "s" when there is not exactly one.
 */
const pluralize = function (count: number, noun: string): string {
  return `${count} ${noun}${count === 1 ? "" : "s"}`;
};

/**
 * One item listed for a group needing attention, with an optional amount that is highlighted after its name.
 */
interface OverviewAttentionItemDescription {
  readonly text: string;
  readonly amount?: string;
}

/**
 * What the overview says about a group of items needing attention, and where to resolve it. Items are listed together,
 * followed by how many more there are, while each fact is a separate piece of supporting information.
 */
interface OverviewAttentionDescription {
  readonly title: string;
  readonly amount: string;
  readonly items: readonly OverviewAttentionItemDescription[];
  readonly remainingItemCount: number;
  readonly facts: readonly string[];
  readonly href: Route;
}

/**
 * Describes a group of items needing attention, with links that return to the provided URL. Goal groups link to the
 * matching plan tab of the current period, and income groups to its Expected Income Sources.
 */
const describeAttentionGroup = function (
  group: OverviewAttentionGroup,
  currentPeriodId: string | null,
  returnUrl: string,
): OverviewAttentionDescription {
  const getPlanHref = function (planTab: AccountingPeriodPlanTab): Route {
    return currentPeriodId === null
      ? accountingPeriodRoutes.workspace({ returnUrl })
      : accountingPeriodRoutes.workspaceDetail(currentPeriodId, {
          planTab,
          returnUrl,
        });
  };
  const remainingItemCount = group.itemCount - group.topItems.length;
  const oldestDate =
    typeof group.oldestDate === "string"
      ? formatShortDate(new Date(`${group.oldestDate}T00:00:00`))
      : null;

  switch (group.kind) {
    case OverviewAttentionKindModel.GoalsBelowMinimum:
      return {
        title: `${pluralize(group.count, "goal")} below minimum`,
        amount: `${formatCurrency(group.amount)} short`,
        items: group.topItems.map((item) => ({
          text: item.name,
          amount: formatSignedCurrency(-item.amount),
        })),
        remainingItemCount,
        facts: [],
        href: getPlanHref("fund-goals"),
      };
    case OverviewAttentionKindModel.GoalsAboveMaximum:
      return {
        title: `${pluralize(group.count, "goal")} above maximum`,
        amount: `${formatCurrency(group.amount)} over`,
        items: group.topItems.map((item) => ({
          text: item.name,
          amount: formatSignedCurrency(item.amount),
        })),
        remainingItemCount,
        facts: [],
        href: getPlanHref("fund-goals"),
      };
    case OverviewAttentionKindModel.GoalsBehind:
      return {
        title: `${pluralize(group.count, "goal")} not fully funded`,
        amount: `${formatCurrency(group.amount)} remaining`,
        items: group.topItems.map((item) => ({
          text: item.name,
          amount: formatCurrency(item.amount),
        })),
        remainingItemCount,
        facts: [],
        href: getPlanHref("fund-goals"),
      };
    case OverviewAttentionKindModel.UnpostedTransactions:
      return {
        title: `${pluralize(group.count, "transaction")} not posted`,
        amount: `${formatSignedCurrency(group.amount)} pending`,
        items: group.topItems.map((item) => ({
          text: `${item.name} (${item.count ?? 0})`,
        })),
        remainingItemCount,
        facts: oldestDate === null ? [] : [`Oldest ${oldestDate}`],
        href: transactionRoutes.workspace({
          fullyPosted: "false",
          returnUrl,
        }),
      };
    case OverviewAttentionKindModel.IncomeNotReceived:
    default:
      return {
        title: `${pluralize(group.count, "income source")} behind`,
        amount: `${formatCurrency(group.amount)} short`,
        items: group.topItems.map((item) => ({
          text: item.name,
          amount: formatSignedCurrency(-item.amount),
        })),
        remainingItemCount,
        facts: [
          ...(group.income === null || group.income === undefined
            ? []
            : [
                `${formatCurrency(group.income.receivedAmount)} of ${formatCurrency(group.income.expectedAmount)} received`,
              ]),
          ...(oldestDate === null ? [] : [`Behind since ${oldestDate}`]),
        ],
        href:
          currentPeriodId === null
            ? accountingPeriodRoutes.workspace({ returnUrl })
            : `${accountingPeriodRoutes.workspaceDetail(currentPeriodId, { returnUrl })}#${expectedIncomeSourcesId}`,
      };
  }
};

export {
  describeAttentionGroup,
  defaultOverviewHistory,
  getOverviewUrl,
  overviewHistoryOptions,
  parseOverviewHistoryRange,
};
export type { OverviewAttentionGroup, OverviewSearchParams };
