"use server";

import type { SpendingGoal, UpdateSpendingGoalRequest } from "@/goals/types";
import formatErrors from "@/framework/forms/formatErrors";
import getApiClient from "@/framework/data/getApiClient";
import { isApiError } from "@/framework/data/apiError";
import nameof from "@/framework/data/nameof";
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
  const client = getApiClient();
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
      let typeErrorMessage = null;
      const unmappedErrors: (string | null)[] = [];
      for (const key of Object.keys(error.errors ?? {})) {
        if (
          key.toUpperCase() ===
          nameof<UpdateSpendingGoalRequest>("spendingGoalType").toUpperCase()
        ) {
          typeErrorMessage = formatErrors(error.errors?.[key] ?? null);
        } else {
          unmappedErrors.push(formatErrors(error.errors?.[key] ?? null));
        }
      }
      return {
        errorTitle: error.title ?? null,
        typeErrors: typeErrorMessage,
        unmappedErrors: unmappedErrors.join(", ") || null,
      };
    }
    throw new Error("An unexpected error occurred", { cause: error });
  }
  revalidatePath(redirectUrl);
  return { success: true };
};

export default updateSpendingGoal;
