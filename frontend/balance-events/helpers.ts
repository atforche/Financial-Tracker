import { BalanceEventType } from "@/balance-events/types";

interface BalanceEventParty {
  readonly displayName: string;
}

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
