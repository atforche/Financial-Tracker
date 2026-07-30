import {
  normalizeAccountTypes,
  shouldPersistAccountTypes,
} from "@/accounts/accountTypeFilterHelpers";
import type { AccountType } from "@/accounts/types";
import type { AccountWorkspaceSearchParams } from "@/accounts/workspace/types";
import propertyName from "@/framework/data/propertyName";

/**
 * Normalized filters used by account workspace clients.
 */
interface AccountWorkspaceFilters {
  readonly search: string;
  readonly accountTypes: readonly AccountType[];
  readonly hasActiveFilters: boolean;
}

/**
 * Defines the parameter names used in the account workspace search parameters.
 */
const accountWorkspaceParamNames = {
  action: propertyName<AccountWorkspaceSearchParams>("action"),
  accountType: propertyName<AccountWorkspaceSearchParams>("accountType"),
  search: propertyName<AccountWorkspaceSearchParams>("search"),
} as const;

/**
 * Removes the filters shared by the workspace list and empty state.
 */
const clearAccountWorkspaceFilters = function (params: URLSearchParams): void {
  params.delete(accountWorkspaceParamNames.search);
  params.delete(accountWorkspaceParamNames.accountType);
};

/**
 * Parses account workspace filters from URL search parameters.
 */
const parseAccountWorkspaceFilters = function (
  params: Pick<URLSearchParams, "get" | "getAll">,
): AccountWorkspaceFilters {
  const search = (params.get(accountWorkspaceParamNames.search) ?? "").trim();
  const accountTypes = normalizeAccountTypes(
    params.getAll(accountWorkspaceParamNames.accountType),
  );

  return {
    search,
    accountTypes,
    hasActiveFilters: search !== "" || shouldPersistAccountTypes(accountTypes),
  };
};

export {
  accountWorkspaceParamNames,
  clearAccountWorkspaceFilters,
  parseAccountWorkspaceFilters,
};
