"use server";

import type {
  AccountGoal,
  UpdateAccountGoalRequest,
} from "@/account-goals/types";
import createApiClient from "@/framework/data/createApiClient";
import { isApiError } from "@/framework/data/apiError";
import mapApiValidationError from "@/framework/forms/mapApiValidationError";
import propertyName from "@/framework/data/propertyName";
import { revalidatePath } from "next/cache";

interface ActionState {
  readonly success?: boolean;
  readonly errorTitle?: string | null;
  readonly minimumEndingBalanceErrors?: string | null;
  readonly maximumEndingBalanceErrors?: string | null;
  readonly unmappedErrors?: string | null;
}

interface ActionPayload {
  readonly accountGoal: AccountGoal;
  readonly request: UpdateAccountGoalRequest;
  readonly redirectUrl: string;
}

/**
 * Updates an Account Goal and revalidates the detail page.
 */
const updateAccountGoal = async function (
  _: ActionState,
  payload: ActionPayload,
): Promise<ActionState> {
  const apiClient = await createApiClient();
  const response = await apiClient.POST("/account-goals/{accountGoalId}", {
    params: { path: { accountGoalId: payload.accountGoal.id } },
    body: payload.request,
  });
  const error: unknown = response.error;
  if (error !== undefined && error !== null) {
    if (!isApiError(error)) {
      throw new Error("An unexpected error occurred", { cause: error });
    }
    const mappedError = mapApiValidationError(error, {
      [propertyName<UpdateAccountGoalRequest>("minimumEndingBalance")]:
        "minimumEndingBalanceErrors",
      [propertyName<UpdateAccountGoalRequest>("maximumEndingBalance")]:
        "maximumEndingBalanceErrors",
    });
    return {
      ...mappedError,
      ...mappedError.fieldErrors,
    };
  }

  revalidatePath(payload.redirectUrl);
  return { success: true };
};

export default updateAccountGoal;
