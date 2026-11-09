import { formatShortDate } from "@/framework/dateHelpers";

/**
 * Starting and ending balances of a trends range, and how to describe where it starts.
 */
interface TrendSnapshot<TBalance> {
  readonly startLabel: string;
  readonly startingBalance: TBalance;
  readonly endingBalance: TBalance;
}

/**
 * Gets the balances at each end of a trends range. A range holds either periods or dates.
 */
const getTrendSnapshot = function <TBalance>(
  accountingPeriods: readonly {
    readonly accountingPeriod: { readonly name: string };
    readonly openingBalance: TBalance;
    readonly closingBalance: TBalance;
  }[],
  dates: readonly (TBalance & { readonly date: string })[],
  openingBalance: TBalance | null,
  startDate: string | null,
  emptyBalance: TBalance,
): TrendSnapshot<TBalance> {
  const firstPeriod = accountingPeriods.at(0);
  const lastPeriod = accountingPeriods.at(-1);
  if (firstPeriod !== undefined && lastPeriod !== undefined) {
    return {
      startLabel: firstPeriod.accountingPeriod.name,
      startingBalance: firstPeriod.openingBalance,
      endingBalance: lastPeriod.closingBalance,
    };
  }
  const firstDate = dates.at(0);
  const startLabelDate = startDate ?? firstDate?.date;
  return {
    startLabel:
      startLabelDate === undefined
        ? "the start"
        : formatShortDate(new Date(`${startLabelDate}T00:00:00`)),
    startingBalance: openingBalance ?? firstDate ?? emptyBalance,
    endingBalance: dates.at(-1) ?? emptyBalance,
  };
};

export default getTrendSnapshot;
