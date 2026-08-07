import type { components } from "@/framework/data/api";

/** Role value accepted by user-management actions. */
type UserRole = components["schemas"]["UserRoleModel"];

/** Result returned to the user-management client after an administration action. */
interface UserManagementActionState {
  readonly errorTitle: string | null;
  readonly unmappedErrors: string | null;
  readonly success: boolean;
}

export type { UserManagementActionState, UserRole };
