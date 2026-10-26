import type { components } from "@/framework/data/api";

/**
 * Type representing a Location.
 */
type Location = components["schemas"]["LocationModel"];

/**
 * Input for creating or selecting a Location.
 */
type LocationInput = components["schemas"]["LocationInputModel"];

/**
 * A draft of a Location being entered in a form.
 */
interface LocationDraft {
  readonly id: string | null;
  readonly name: string;
}

/**
 * Converts a persisted Location into an editable selection.
 */
const toLocationDraft = function (
  location: Location | null,
): LocationDraft | null {
  return location === null ? null : { id: location.id, name: location.name };
};

/**
 * Converts a Location selection into the transaction request model.
 */
const toLocationInput = function (
  location: LocationDraft | null,
): LocationInput | null {
  return location === null
    ? null
    : location.id === null
      ? { newLocationName: location.name.trim() }
      : { locationId: location.id };
};

export { type Location, type LocationDraft, toLocationDraft, toLocationInput };
