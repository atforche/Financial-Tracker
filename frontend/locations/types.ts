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
 * Type representing a location's activity over a range.
 */
type LocationRangeSummary = components["schemas"]["LocationRangeSummaryModel"];

/**
 * Type representing one location's activity over a range, by month, and how it was paid for.
 */
type LocationDetailRange = components["schemas"]["LocationDetailRangeModel"];

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

export {
  type Location,
  type LocationDetailRange,
  type LocationDraft,
  type LocationRangeSummary,
  toLocationDraft,
  toLocationInput,
};
