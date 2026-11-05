import {
  getDefaultTrendAccountingPeriodRange,
  resolveTrendRange,
} from "@/framework/routes/trendRange";
import type { AccountingPeriod } from "@/accounting-periods/types";
import type { DetailViewParams } from "@/balance-events/getDetailViewParams";

const periodTab = "period";
const defaultPeriodCount = 6;

/**
 * The selected view of an account or fund page, with the range of each view. The period range is null before any
 * accounting period exists.
 */
interface EntityHistory {
  readonly view: "date" | "period";
  readonly dates: { readonly start: string; readonly end: string };
  readonly periods: {
    readonly start: string;
    readonly end: string;
    readonly ids: readonly string[];
  } | null;
}

/**
 * Resolves the By Date dates, replacing invalid values with the default range.
 */
const resolveDates = function (
  params: DetailViewParams,
  accountingPeriods: readonly AccountingPeriod[],
): EntityHistory["dates"] {
  const { startDate, endDate } = params;
  const range = resolveTrendRange(
    {
      mode: "date",
      ...(startDate === undefined ? {} : { startDate }),
      ...(endDate === undefined ? {} : { endDate }),
    },
    accountingPeriods,
  );
  return range.mode === "date"
    ? { start: range.startDate, end: range.endDate }
    : { start: "", end: "" };
};

/**
 * Resolves the By Period range from accounting periods sorted newest first, replacing unknown periods with the default
 * and ordering the range.
 */
const resolvePeriods = function (
  params: DetailViewParams,
  accountingPeriods: readonly AccountingPeriod[],
): EntityHistory["periods"] {
  const fallback = getDefaultTrendAccountingPeriodRange(
    accountingPeriods,
    defaultPeriodCount,
  );
  if (fallback === null) {
    return null;
  }
  const getIndex = function (
    id: string | undefined,
    fallbackId: string,
  ): number {
    const index = accountingPeriods.findIndex((period) => period.id === id);
    return index === -1
      ? accountingPeriods.findIndex((period) => period.id === fallbackId)
      : index;
  };
  const first = getIndex(params.startAccountingPeriodId, fallback.start);
  const second = getIndex(params.endAccountingPeriodId, fallback.end);
  const periods = accountingPeriods.slice(
    Math.min(first, second),
    Math.max(first, second) + 1,
  );
  return {
    start: periods.at(-1)?.id ?? fallback.start,
    end: periods.at(0)?.id ?? fallback.end,
    ids: periods.map(({ id }) => id),
  };
};

/**
 * Resolves the view and range shown by an account or fund page. Accounting periods are sorted newest first.
 */
const resolveEntityHistory = function (
  params: DetailViewParams,
  accountingPeriods: readonly AccountingPeriod[],
): EntityHistory {
  return {
    view: params.tab === periodTab ? "period" : "date",
    dates: resolveDates(params, accountingPeriods),
    periods: resolvePeriods(params, accountingPeriods),
  };
};

export { periodTab };
export default resolveEntityHistory;
