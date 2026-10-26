import { BalanceEventType } from "@/balance-events/types";

/**
 * A named source or destination of a balance event.
 */
interface BalanceEventParty {
  readonly displayName: string;
}

/**
 * Balance event fields needed to describe its counterparty.
 */
interface BalanceEventPartyEvent {
  readonly destinations: readonly BalanceEventParty[];
  readonly source: BalanceEventParty | readonly BalanceEventParty[];
  readonly type: BalanceEventType;
}

/**
 * Formats the party on the other side of an Account balance event.
 */
const formatBalanceEventCounterparty = function (
  event: BalanceEventPartyEvent,
): string {
  if (event.type === BalanceEventType.Debit) {
    const destinations = event.destinations.map(
      (destination) => destination.displayName,
    );
    return destinations.length === 0 ? "—" : destinations.join(", ");
  }
  const sources: readonly BalanceEventParty[] = Array.isArray(event.source)
    ? event.source
    : [event.source];
  return sources.map((source) => source.displayName).join(", ");
};

export { formatBalanceEventCounterparty };
