import type { AccountWorkspaceSearchParams } from "@/accounts/workspace/AccountWorkspace";
import nameof from "@/framework/data/nameof";

/**
 * Defines the parameter names used in the account workspace search parameters.
 */
const accountWorkspaceParamNames = {
  action: nameof<AccountWorkspaceSearchParams>("action"),
  accountType: nameof<AccountWorkspaceSearchParams>("accountType"),
  search: nameof<AccountWorkspaceSearchParams>("search"),
} as const;

/**
 * Removes the filters shared by the workspace list and empty state.
 */
const clearAccountWorkspaceFilters = function (params: URLSearchParams): void {
  params.delete(accountWorkspaceParamNames.search);
  params.delete(accountWorkspaceParamNames.accountType);
};

export { accountWorkspaceParamNames, clearAccountWorkspaceFilters };
