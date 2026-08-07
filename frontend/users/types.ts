import type { components } from "@/framework/data/api";

/**
 * Type representing a user invitation.
 */
type UserInvitation = components["schemas"]["UserInvitationModel"];

/**
 * Type representing an application user.
 */
type User = components["schemas"]["UserModel"];

/**
 * Type representing a User Role.
 */
type UserRole = components["schemas"]["UserRoleModel"];

export type { User, UserInvitation, UserRole };
