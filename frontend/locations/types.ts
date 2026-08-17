import type { components } from "@/framework/data/api";

type Location = components["schemas"]["LocationModel"];
type LocationInput = components["schemas"]["LocationInputModel"];

interface LocationDraft {
  readonly id: string | null;
  readonly name: string;
}

/** Converts a persisted Location into an editable selection. */
const toLocationDraft = (location: Location | null): LocationDraft | null =>
  location === null ? null : { id: location.id, name: location.name };

/** Converts a Location selection into the transaction request model. */
const toLocationInput = (
  location: LocationDraft | null,
): LocationInput | null =>
  location === null
    ? null
    : location.id === null
      ? { newLocationName: location.name.trim() }
      : { locationId: location.id };

export {
  type Location,
  type LocationDraft,
  type LocationInput,
  toLocationDraft,
  toLocationInput,
};
