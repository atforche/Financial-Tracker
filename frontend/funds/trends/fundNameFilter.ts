import {
  normalizeStringSearchParams,
  selectAvailableSearchParamValues,
} from "@/framework/routes/helpers";

/**
 * Normalizes raw fund-name values from the URL into a trimmed, unique list.
 */
const normalizeRequestedFundNames = function (
  values: readonly string[],
): string[] {
  return normalizeStringSearchParams(values, (value) =>
    value.toLocaleLowerCase(),
  );
};

/**
 * Restricts selected fund names to the currently available fund-name options.
 */
const normalizeFundNames = function (
  values: readonly string[],
  availableFundNames: readonly string[],
): readonly string[] {
  return selectAvailableSearchParamValues(
    normalizeRequestedFundNames(values),
    availableFundNames,
    (fundName) => fundName.toLocaleLowerCase(),
    (fundName) => fundName.toLocaleLowerCase(),
  );
};

/**
 * Determines whether selected fund names should be persisted in the URL.
 */
const shouldPersistFundNames = function (values: readonly string[]): boolean {
  return values.length > 0;
};

export {
  normalizeFundNames,
  normalizeRequestedFundNames,
  shouldPersistFundNames,
};
