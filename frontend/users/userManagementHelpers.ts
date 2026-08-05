import type { UserRole } from "@/users/types";
import { UserRoleModel } from "@/framework/data/api";

const roles: readonly UserRole[] = [
  UserRoleModel.Admin,
  UserRoleModel.Standard,
  UserRoleModel.ReadOnly,
];

/**
 * Formats an optional UTC timestamp for compact list presentation.
 */
const formatDate = function (value: string | null): string {
  if (value === null) {
    return "—";
  }

  return new Intl.DateTimeFormat(undefined, {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(new Date(value));
};

export { formatDate, roles };
