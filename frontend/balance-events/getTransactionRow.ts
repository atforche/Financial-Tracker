import type { AccountType } from "@/accounts/types";
import { BalanceEventType } from "@/balance-events/types";
import type { Route } from "next";
import { formatBalanceEventCounterparty } from "@/balance-events/helpers";
import { formatShortDate } from "@/framework/dateHelpers";
import { formatSignedCurrency } from "@/framework/currencyHelpers";
import { isDebtAccountType } from "@/accounts/helpers";
import routes from "@/transactions/routes";

/**
 * Balance event fields needed to list a transaction's effect on an account or fund.
 */
interface EntityEvent {
  readonly transactionId: string;
  readonly transactionDate: string;
  readonly eventDate?: string | null;
  readonly description: string;
  readonly type: BalanceEventType;
  readonly amount: number;
  readonly isPosted: boolean;
  readonly source:
    | { readonly displayName: string }
    | readonly { readonly displayName: string }[];
  readonly destinations: readonly { readonly displayName: string }[];
  readonly account?: { readonly type: AccountType };
}

/**
 * Builds the list row for a balance event. The value shows how the balance moved, and green marks a credit. Identical events can share every field, so the row's index keeps its key unique.
 */
const getTransactionRow = function (
  event: EntityEvent,
  index: number,
  returnUrl: string,
): {
  readonly key: string;
  readonly href: Route;
  readonly leading: string;
  readonly title: string;
  readonly detail: string;
  readonly value: string;
  readonly valueColor?: string;
} {
  const isCredit = event.type === BalanceEventType.Credit;
  // A credit is good for every account, but it lowers the balance of a debt account, so the sign follows the balance
  const raisesBalance =
    event.account !== undefined && isDebtAccountType(event.account.type)
      ? !isCredit
      : isCredit;
  return {
    key: `${event.transactionId}-${index.toString()}`,
    href: routes.workspaceDetail(event.transactionId, { returnUrl }),
    leading: formatShortDate(
      new Date(`${event.eventDate ?? event.transactionDate}T00:00:00`),
    ),
    title: event.description,
    detail: formatBalanceEventCounterparty(event),
    value: formatSignedCurrency(raisesBalance ? event.amount : -event.amount),
    ...(isCredit ? { valueColor: "success.main" } : {}),
  };
};

export type { EntityEvent };
export default getTransactionRow;
