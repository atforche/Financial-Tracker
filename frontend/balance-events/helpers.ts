import { BalanceEventType } from "@/balance-events/types";

interface BalanceEventPartyListItem {
  readonly destinations: readonly { readonly displayName: string }[];
}

interface BalanceEventPartyEvent extends BalanceEventPartyListItem {
  readonly source: { readonly displayName: string };
  readonly type: BalanceEventType;
}

/**
 * Formats the destinations associated with an Account balance event.
 */
const formatBalanceEventDestinations = function (
  event: BalanceEventPartyListItem,
): string {
  const destinations = event.destinations.map(
    (destination) => destination.displayName,
  );
  return destinations.length === 0 ? "—" : destinations.join(", ");
};

/**
 * Formats the party on the other side of an Account balance event.
 */
const formatBalanceEventCounterparty = function (
  event: BalanceEventPartyEvent,
): string {
  return event.type === BalanceEventType.Debit
    ? formatBalanceEventDestinations(event)
    : event.source.displayName;
};

export { formatBalanceEventCounterparty, formatBalanceEventDestinations };
