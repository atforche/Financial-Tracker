import type { UserRole } from "@/users/types";
import { UserRoleModel } from "@/framework/data/api";

const roles: readonly UserRole[] = [
  UserRoleModel.Admin,
  UserRoleModel.Standard,
  UserRoleModel.ReadOnly,
];

/** Formats an application role for display. */
const formatUserRole = function (role: UserRole): string {
  return role === UserRoleModel.ReadOnly ? "Read only" : role;
};

/**
 * Formats an optional UTC timestamp for compact list presentation.
 */
const formatDate = function (value: string | null | undefined): string {
  if (value === null || typeof value === "undefined") {
    return "—";
  }

  const date = new Date(value);
  if (Number.isNaN(date.getTime())) {
    return "—";
  }

  return new Intl.DateTimeFormat(undefined, {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(date);
};

export { formatDate, formatUserRole, roles };
