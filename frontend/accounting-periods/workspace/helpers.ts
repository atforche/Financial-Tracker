import {
  getCurrencyDifference,
  getCurrencyTotal,
} from "@/framework/currencyHelpers";
import type { ExpectedIncomeSourceRequest } from "@/accounting-periods/types";
import type { Route } from "next";
import { periodTab } from "@/balance-events/resolveEntityHistory";

/**
 * Type representing the possible actions that can be performed in the Accounting Period workspace.
 */
type AccountingPeriodWorkspaceAction = "create";

/**
 * ID of an accounting period page's Expected Income Sources card, so links can jump straight to it.
 */
const expectedIncomeSourcesId = "expected-income-sources";

/**
 * Gets the net amount of one expected payment and the part of it that reaches tracked accounts.
 */
const getPaymentAmounts = function (source: ExpectedIncomeSourceRequest): {
  net: number;
  tracked: number;
} {
  const net = getCurrencyDifference(
    getCurrencyTotal(source.incomeLines.map((line) => line.amount)),
    getCurrencyTotal(
      source.incomeDeductions.map((deduction) => deduction.amount),
    ),
  );
  return {
    net,
    tracked: getCurrencyDifference(
      net,
      getCurrencyTotal(
        source.untrackedTransfers.map((transfer) => transfer.amount),
      ),
    ),
  };
};

/**
 * Gets the link from a period's account or fund page to all of its periods. Arriving from that page goes back to it as
 * it was left, so moving between the two doesn't nest return links.
 */
const getAllPeriodsHref = function (
  getEntityHref: (searchParams: { tab?: string; returnUrl?: string }) => Route,
  returnUrl: string | undefined,
  currentUrl: string,
): Route {
  return returnUrl?.split("?")[0] === getEntityHref({})
    ? returnUrl
    : getEntityHref({ tab: periodTab, returnUrl: currentUrl });
};

export type { AccountingPeriodWorkspaceAction };
export { expectedIncomeSourcesId, getAllPeriodsHref, getPaymentAmounts };
