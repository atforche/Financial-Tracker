import {
  normalizeStringSearchParams,
  selectAvailableSearchParamValues,
} from "@/framework/routes/helpers";

/**
 * Normalizes raw account-name values from the URL into a trimmed, unique list.
 */
const normalizeRequestedAccountNames = function (
  values: readonly string[],
): readonly string[] {
  return normalizeStringSearchParams(values, (value) =>
    value.toLocaleLowerCase(),
  );
};

/**
 * Restricts selected account names to the currently available account-name options.
 */
const normalizeAccountNames = function (
  values: readonly string[],
  availableAccountNames: readonly string[],
): readonly string[] {
  return selectAvailableSearchParamValues(
    normalizeRequestedAccountNames(values),
    availableAccountNames,
    (accountName) => accountName.toLocaleLowerCase(),
    (accountName) => accountName.toLocaleLowerCase(),
  );
};

/**
 * Determines whether selected account names should be persisted in the URL.
 */
const shouldPersistAccountNames = function (
  values: readonly string[],
): boolean {
  return values.length > 0;
};

export {
  normalizeAccountNames,
  normalizeRequestedAccountNames,
  shouldPersistAccountNames,
};
