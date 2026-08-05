import "server-only";

import type { components } from "@/framework/data/api";
import createApiClient from "@/framework/data/createApiClient";

type CurrentApplicationUser = components["schemas"]["UserModel"];

interface CurrentApplicationUserResult {
  readonly accessDenied: boolean;
  readonly user: CurrentApplicationUser | null;
}

/**
 * Loads the current database-backed application user for the server session.
 */
const getCurrentApplicationUser =
  async function (): Promise<CurrentApplicationUserResult> {
    const apiClient = await createApiClient();
    const response = await apiClient.GET("/users/me");
    if (response.response.status === 401 || response.response.status === 403) {
      return { accessDenied: true, user: null };
    }
    if (response.error !== undefined) {
      throw new Error("The current application user could not be loaded.", {
        cause: response.error,
      });
    }

    return { accessDenied: false, user: response.data };
  };

export type { CurrentApplicationUser, CurrentApplicationUserResult };
export default getCurrentApplicationUser;
