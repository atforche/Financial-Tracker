/**
 * Normalizes raw account-name values from the URL into a trimmed, unique list.
 */
const normalizeRequestedAccountNames = function (
  values: readonly string[],
): readonly string[] {
  const seenAccountNames = new Set<string>();
  const normalizedAccountNames: string[] = [];
  values.forEach((value) => {
    const trimmedValue = value.trim();
    const normalizedValue = trimmedValue.toLocaleLowerCase();
    if (trimmedValue === "" || seenAccountNames.has(normalizedValue)) {
      return;
    }
    seenAccountNames.add(normalizedValue);
    normalizedAccountNames.push(trimmedValue);
  });
  return normalizedAccountNames;
};

/**
 * Restricts selected account names to the currently available account-name options.
 */
const normalizeAccountNames = function (
  values: readonly string[],
  availableAccountNames: readonly string[],
): readonly string[] {
  const selectedAccountNames = new Set(
    normalizeRequestedAccountNames(values).map((accountName) =>
      accountName.toLocaleLowerCase(),
    ),
  );
  if (selectedAccountNames.size === 0 || availableAccountNames.length === 0) {
    return [];
  }
  return availableAccountNames.filter((accountName) =>
    selectedAccountNames.has(accountName.toLocaleLowerCase()),
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
