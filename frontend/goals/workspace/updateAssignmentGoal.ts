"use server";

import type {
  AssignmentGoal,
  UpdateAssignmentGoalRequest,
} from "@/goals/types";
import createApiClient from "@/framework/data/createApiClient";
import { isApiError } from "@/framework/data/apiError";
import mapApiValidationError from "@/framework/forms/mapApiValidationError";
import propertyName from "@/framework/data/propertyName";
import { revalidatePath } from "next/cache";

/**
 * Interface representing the state of updating an assignment goal.
 */
interface ActionState {
  readonly success?: boolean;
  readonly errorTitle?: string | null;
  readonly typeErrors?: string | null;
  readonly goalAmountErrors?: string | null;
  readonly unmappedErrors?: string | null;
}

/**
 * Payload for the update goal action.
 */
interface ActionPayload {
  readonly goal: AssignmentGoal;
  readonly request: UpdateAssignmentGoalRequest;
  readonly redirectUrl: string;
}

/**
 * Server action that updates an assignment goal.
 */
const updateAssignmentGoal = async function (
  _: ActionState,
  { goal, request, redirectUrl }: ActionPayload,
): Promise<ActionState> {
  const client = createApiClient();
  const { error } = await client.POST("/goals/assignment/{goalId}", {
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
        [propertyName<UpdateAssignmentGoalRequest>("assignmentGoalType")]:
          "typeErrors",
        [propertyName<UpdateAssignmentGoalRequest>("goalAmount")]:
          "goalAmountErrors",
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

export default updateAssignmentGoal;
