import { BalanceEventType } from "@/balance-events/types";
import type { components } from "@/framework/data/api";

type AccountBalanceEventPartyListItem = Pick<
  components["schemas"]["AccountBalanceEventModel"],
  "destinations"
>;

type AccountBalanceEventPartyEvent = Pick<
  components["schemas"]["AccountBalanceEventModel"],
  "destinations" | "source" | "type"
>;

/**
 * Formats the destinations associated with an Account balance event.
 */
const formatBalanceEventDestinations = function (
  event: AccountBalanceEventPartyListItem,
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
  event: AccountBalanceEventPartyEvent,
): string {
  return event.type === BalanceEventType.Debit
    ? formatBalanceEventDestinations(event)
    : event.source.displayName;
};

export { formatBalanceEventCounterparty, formatBalanceEventDestinations };
