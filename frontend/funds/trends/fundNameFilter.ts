/**
 * Normalizes raw fund-name values from the URL into a trimmed, unique list.
 */
const normalizeRequestedFundNames = function (
  values: readonly string[],
): string[] {
  const seenFundNames = new Set<string>();
  const normalizedFundNames: string[] = [];
  values.forEach((value) => {
    const trimmedValue = value.trim();
    const normalizedValue = trimmedValue.toLocaleLowerCase();
    if (trimmedValue === "" || seenFundNames.has(normalizedValue)) {
      return;
    }
    seenFundNames.add(normalizedValue);
    normalizedFundNames.push(trimmedValue);
  });
  return normalizedFundNames;
};

/**
 * Restricts selected fund names to the currently available fund-name options.
 */
const normalizeFundNames = function (
  values: readonly string[],
  availableFundNames: readonly string[],
): readonly string[] {
  const selectedFundNames = new Set(
    normalizeRequestedFundNames(values).map((fundName) =>
      fundName.toLocaleLowerCase(),
    ),
  );
  if (selectedFundNames.size === 0 || availableFundNames.length === 0) {
    return [];
  }
  return availableFundNames.filter((fundName) =>
    selectedFundNames.has(fundName.toLocaleLowerCase()),
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
