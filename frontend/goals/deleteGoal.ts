"use server";

import formatErrors from "@/framework/forms/formatErrors";
import getApiClient from "@/framework/data/getApiClient";
import { isApiError } from "@/framework/data/apiError";
import { redirect } from "next/navigation";
import { revalidatePath } from "next/cache";

/**
 * Interface representing the state of deleting a goal.
 */
interface ActionState {
  readonly goalId: string;
  readonly redirectUrl: string;
  readonly errorTitle?: string | null;
  readonly unmappedErrors?: string | null;
}

/**
 * Server action that deletes a goal.
 */
const deleteGoal = async function ({
  goalId,
  redirectUrl,
}: ActionState): Promise<ActionState> {
  const client = getApiClient();
  const { error } = await client.DELETE("/goals/{goalId}", {
    params: {
      path: {
        goalId,
      },
    },
  });
  if (error) {
    if (isApiError(error)) {
      const unmappedErrors: (string | null)[] = [];
      for (const key of Object.keys(error.errors ?? {})) {
        unmappedErrors.push(formatErrors(error.errors?.[key] ?? null));
      }
      return {
        goalId,
        redirectUrl,
        errorTitle: error.title ?? null,
        unmappedErrors: unmappedErrors.join(", ") || null,
      };
    }
    throw new Error("An unexpected error occurred", { cause: error });
  }

  revalidatePath(redirectUrl);
  redirect(redirectUrl);
};

export default deleteGoal;
