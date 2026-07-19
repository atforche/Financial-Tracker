"use server";

import type { SpendingGoal, UpdateSpendingGoalRequest } from "@/goals/types";
import createApiClient from "@/framework/data/createApiClient";
import { isApiError } from "@/framework/data/apiError";
import mapApiValidationError from "@/framework/forms/mapApiValidationError";
import propertyName from "@/framework/data/propertyName";
import { revalidatePath } from "next/cache";

/**
 * Interface representing the state of updating a spending goal.
 */
interface ActionState {
  readonly success?: boolean;
  readonly errorTitle?: string | null;
  readonly typeErrors?: string | null;
  readonly unmappedErrors?: string | null;
}

/**
 * Payload for the update goal action.
 */
interface ActionPayload {
  readonly goal: SpendingGoal;
  readonly request: UpdateSpendingGoalRequest;
  readonly redirectUrl: string;
}

/**
 * Server action that updates a spending goal.
 */
const updateSpendingGoal = async function (
  _: ActionState,
  { goal, request, redirectUrl }: ActionPayload,
): Promise<ActionState> {
  const client = createApiClient();
  const { error } = await client.POST("/goals/spending/{goalId}", {
    params: {
      path: {
        goalId: goal.id,
      },
    },
    body: request,
  });
  if (error) {
    if (isApiError(error)) {
      const mappedError = mapApiValidationError(error, {
        [propertyName<UpdateSpendingGoalRequest>("spendingGoalType")]:
          "typeErrors",
      });
      return {
        ...mappedError,
        ...mappedError.fieldErrors,
      };
    }
    throw new Error("An unexpected error occurred", { cause: error });
  }
  revalidatePath(redirectUrl);
  return { success: true };
};

export default updateSpendingGoal;
