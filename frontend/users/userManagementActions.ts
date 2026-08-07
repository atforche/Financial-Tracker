"use server";

import type {
  UserManagementActionState,
  UserRole,
} from "@/users/userManagementActionState";
import createApiClient from "@/framework/data/createApiClient";

/**
 * Converts a safe API error into presentation state.
 */
const toActionState = function (error: unknown): UserManagementActionState {
  if (typeof error === "object" && error !== null) {
    const problem = error as {
      readonly detail?: unknown;
      readonly title?: unknown;
    };
    return {
      errorTitle:
        typeof problem.title === "string" ? problem.title : "Request failed",
      unmappedErrors:
        typeof problem.detail === "string"
          ? problem.detail
          : "The request could not be completed. Please try again.",
      success: false,
    };
  }

  return {
    errorTitle: "Request failed",
    unmappedErrors: "The request could not be completed. Please try again.",
    success: false,
  };
};

/**
 * Returns a successful administration action state.
 */
const successfulAction = function (): UserManagementActionState {
  return { errorTitle: null, unmappedErrors: null, success: true };
};

/** Creates a pending invitation with the selected role. */
const createUserInvitation = async function (
  email: string,
  role: UserRole,
): Promise<UserManagementActionState> {
  const apiClient = await createApiClient();
  const { error } = await apiClient.POST("/user-invitations", {
    body: { email, role },
  });
  return error === undefined ? successfulAction() : toActionState(error);
};

/**
 * Changes an application user's role.
 */
const changeUserRole = async function (
  userId: string,
  role: UserRole,
): Promise<UserManagementActionState> {
  const apiClient = await createApiClient();
  const { error } = await apiClient.POST("/users/{userId}/role", {
    params: { path: { userId } },
    body: { role },
  });
  return error === undefined ? successfulAction() : toActionState(error);
};

/**
 * Enables an application user.
 */
const enableUser = async function (
  userId: string,
): Promise<UserManagementActionState> {
  const apiClient = await createApiClient();
  const { error } = await apiClient.POST("/users/{userId}/enable", {
    params: { path: { userId } },
  });
  return error === undefined ? successfulAction() : toActionState(error);
};

/**
 * Disables an application user.
 */
const disableUser = async function (
  userId: string,
): Promise<UserManagementActionState> {
  const apiClient = await createApiClient();
  const { error } = await apiClient.POST("/users/{userId}/disable", {
    params: { path: { userId } },
  });
  return error === undefined ? successfulAction() : toActionState(error);
};

/**
 * Revokes a pending invitation.
 */
const revokeUserInvitation = async function (
  invitationId: string,
): Promise<UserManagementActionState> {
  const apiClient = await createApiClient();
  const { error } = await apiClient.DELETE("/user-invitations/{invitationId}", {
    params: { path: { invitationId } },
  });
  return error === undefined ? successfulAction() : toActionState(error);
};

export {
  changeUserRole,
  createUserInvitation,
  disableUser,
  enableUser,
  revokeUserInvitation,
};
