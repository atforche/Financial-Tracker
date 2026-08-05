"use client";

import { type JSX, type ReactNode, createContext, useContext } from "react";
import type { CurrentApplicationUser } from "@/framework/auth/currentApplicationUser";
import { UserRoleModel } from "@/framework/data/api";

const ApplicationUserContext = createContext<CurrentApplicationUser | null>(
  null,
);

/**
 * Props for the database-backed application-user context.
 */
interface ApplicationUserProviderProps {
  readonly children: ReactNode;
  readonly user: CurrentApplicationUser | null;
}

/**
 * Makes the current database-backed application user available to client UI.
 */
const ApplicationUserProvider = function ({
  children,
  user,
}: ApplicationUserProviderProps): JSX.Element {
  return (
    <ApplicationUserContext.Provider value={user}>
      {children}
    </ApplicationUserContext.Provider>
  );
};

/**
 * Returns the current database-backed application user for client UI.
 */
const useApplicationUser = function (): CurrentApplicationUser | null {
  return useContext(ApplicationUserContext);
};

/**
 * Returns whether the current application user can perform financial writes.
 */
const useWriteAccess = function (): boolean {
  const user = useApplicationUser();
  return (
    user?.role === UserRoleModel.Admin || user?.role === UserRoleModel.Standard
  );
};

export { useApplicationUser, useWriteAccess };
export default ApplicationUserProvider;
