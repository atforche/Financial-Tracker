import {
  AccountType,
  formatAccountType,
  isTrackedAccountType,
} from "@/accounts/types";

const accountTypeValues = Object.values(AccountType);

const accountTypeSet = new Set<string>(accountTypeValues);

type AccountTypeFilterGroup = "tracked" | "untracked";

interface AccountTypeFilterGroupOption {
  readonly value: AccountTypeFilterGroup;
  readonly label: string;
  readonly accountTypes: readonly AccountType[];
}

interface AccountTypeGroupSelectionState {
  readonly allSelected: boolean;
  readonly someSelected: boolean;
}

const trackedAccountTypeValues = accountTypeValues.filter(isTrackedAccountType);

const untrackedAccountTypeValues = accountTypeValues.filter(
  (accountType) => !isTrackedAccountType(accountType),
);

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

const isAccountType = function (value: string): value is AccountType {
  return accountTypeSet.has(value);
};

/**
 * Normalizes raw query or select values into a canonical ordered account type list.
 */
const normalizeAccountTypes = function (
  values: readonly string[],
): readonly AccountType[] {
  const selectedValues = new Set(values.filter(isAccountType));
  if (selectedValues.size === 0) {
    return accountTypeValues;
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
  if (selectedAccountTypes.length === accountTypeValues.length) {
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

export {
  type AccountTypeFilterGroup,
  accountTypeFilterGroups,
  accountTypeValues,
  formatSelectedAccountTypes,
  getAccountTypeGroupSelectionState,
  normalizeAccountTypes,
  shouldPersistAccountTypes,
  toggleAccountTypeGroup,
};
