import { formatAccountType, isTrackedAccountType } from "@/accounts/helpers";
import { AccountType } from "@/accounts/types";
import enumValues from "@/framework/data/enumValues";

/**
 * Collection of account types.
 */
const accountTypeValues = enumValues(AccountType);

/**
 * Set of account type values for quick membership checks.
 */
const accountTypeSet = new Set<string>(accountTypeValues);

/**
 * Account type filter group identifiers.
 */
type AccountTypeFilterGroup = "tracked" | "untracked";

/**
 * Defines the structure of an account type filter group option.
 */
interface AccountTypeFilterGroupOption {
  readonly value: AccountTypeFilterGroup;
  readonly label: string;
  readonly accountTypes: readonly AccountType[];
}

/**
 * Defines the selection state of an account type group.
 */
interface AccountTypeGroupSelectionState {
  readonly allSelected: boolean;
  readonly someSelected: boolean;
}

/**
 * Collection of tracked account types.
 */
const trackedAccountTypeValues = accountTypeValues.filter(isTrackedAccountType);

/**
 * Collection of untracked account types.
 */
const untrackedAccountTypeValues = accountTypeValues.filter(
  (accountType) => !isTrackedAccountType(accountType),
);

/**
 * Defines the available account type filter groups with their corresponding account types.
 */
const accountTypeFilterGroups: readonly AccountTypeFilterGroupOption[] = [
  {
    value: "tracked",
    label: "Tracked",
    accountTypes: trackedAccountTypeValues,
  },
  {
    value: "untracked",
    label: "Untracked",
    accountTypes: untrackedAccountTypeValues,
  },
];

/**
 * Determines if the provided value is an account type.
 */
const isAccountType = function (value: string): value is AccountType {
  return accountTypeSet.has(value);
};

/**
 * Normalizes raw query or select values into a canonical ordered account type list.
 * An empty list represents all account types, so selecting every account type
 * is normalized back to an empty list as well.
 */
const normalizeAccountTypes = function (
  values: readonly string[],
): readonly AccountType[] {
  const selectedValues = new Set(values.filter(isAccountType));
  if (
    selectedValues.size === 0 ||
    selectedValues.size === accountTypeValues.length
  ) {
    return [];
  }
  return accountTypeValues.filter((accountType) =>
    selectedValues.has(accountType),
  );
};

/**
 * Determines whether the current selection needs to be written into the URL.
 */
const shouldPersistAccountTypes = function (
  values: readonly AccountType[],
): boolean {
  return values.length > 0 && values.length < accountTypeValues.length;
};

/**
 * Gets the account type filter group option.
 */
const getAccountTypeFilterGroup = function (
  group: AccountTypeFilterGroup,
): AccountTypeFilterGroupOption {
  const matchingGroup = accountTypeFilterGroups.find(
    (accountTypeFilterGroup) => accountTypeFilterGroup.value === group,
  );
  if (typeof matchingGroup === "undefined") {
    throw new Error(`Unrecognized account type filter group: ${group}`);
  }

  return matchingGroup;
};

/**
 * Returns the aggregate selection state for a tracked or untracked account type group.
 */
const getAccountTypeGroupSelectionState = function (
  selectedAccountTypes: readonly AccountType[],
  groupAccountTypes: readonly AccountType[],
): AccountTypeGroupSelectionState {
  const selectedAccountTypeSet = new Set(selectedAccountTypes);
  const selectedGroupCount = groupAccountTypes.filter((accountType) =>
    selectedAccountTypeSet.has(accountType),
  ).length;

  return {
    allSelected: selectedGroupCount === groupAccountTypes.length,
    someSelected: selectedGroupCount > 0,
  };
};

/**
 * Toggles every account type within the provided group.
 */
const toggleAccountTypeGroup = function (
  selectedAccountTypes: readonly AccountType[],
  group: AccountTypeFilterGroup,
): readonly AccountType[] {
  const groupAccountTypes = getAccountTypeFilterGroup(group).accountTypes;
  const nextSelectedAccountTypes = new Set(selectedAccountTypes);
  const areAllGroupTypesSelected = groupAccountTypes.every((accountType) =>
    nextSelectedAccountTypes.has(accountType),
  );

  groupAccountTypes.forEach((accountType) => {
    if (areAllGroupTypesSelected) {
      nextSelectedAccountTypes.delete(accountType);
      return;
    }

    nextSelectedAccountTypes.add(accountType);
  });

  return normalizeAccountTypes(
    accountTypeValues.filter((accountType) =>
      nextSelectedAccountTypes.has(accountType),
    ),
  );
};

/**
 * Formats the current selection for the collapsed multi-select input.
 */
const formatSelectedAccountTypes = function (
  selectedAccountTypes: readonly AccountType[],
): string {
  if (
    selectedAccountTypes.length === 0 ||
    selectedAccountTypes.length === accountTypeValues.length
  ) {
    return "All account types";
  }

  const selectedAccountTypeSet = new Set(selectedAccountTypes);
  const labels: string[] = [];

  accountTypeFilterGroups.forEach((group) => {
    const selectedGroupAccountTypes = group.accountTypes.filter((accountType) =>
      selectedAccountTypeSet.has(accountType),
    );

    if (selectedGroupAccountTypes.length === 0) {
      return;
    }

    if (selectedGroupAccountTypes.length === group.accountTypes.length) {
      labels.push(group.label);
      return;
    }

    labels.push(...selectedGroupAccountTypes.map(formatAccountType));
  });

  return labels.join(", ");
};

/**
 * Group value prefix used to identify account type filter group toggles in the select input.
 */
const groupValuePrefix = "__group__";

/**
 * Determines if the provided value is an account type filter group value.
 */
const isAccountTypeFilterGroupValue = function (
  value: string,
): value is `${typeof groupValuePrefix}${AccountTypeFilterGroup}` {
  return value.startsWith(groupValuePrefix);
};

/**
 * Gets the account type filter group from the provided value.
 */
const getAccountTypeFilterGroupFromValue = function (
  value: `${typeof groupValuePrefix}${AccountTypeFilterGroup}`,
): AccountTypeFilterGroup {
  const groupValue = value.slice(groupValuePrefix.length);
  if (groupValue === "tracked" || groupValue === "untracked") {
    return groupValue;
  }

  throw new Error(`Unrecognized account type filter group value: ${value}`);
};

export {
  type AccountTypeFilterGroup,
  accountTypeFilterGroups,
  accountTypeValues,
  formatSelectedAccountTypes,
  getAccountTypeFilterGroupFromValue,
  getAccountTypeGroupSelectionState,
  groupValuePrefix,
  isAccountTypeFilterGroupValue,
  normalizeAccountTypes,
  shouldPersistAccountTypes,
  toggleAccountTypeGroup,
};
