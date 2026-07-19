"use server";

import type {
  AssignmentGoal,
  SpendingGoal,
  UpdateAssignmentGoalRequest,
  UpdateSpendingGoalRequest,
} from "@/goals/types";
import createApiClient from "@/framework/data/createApiClient";
import { isApiError } from "@/framework/data/apiError";
import mapApiValidationError from "@/framework/forms/mapApiValidationError";
import propertyName from "@/framework/data/propertyName";
import { revalidatePath } from "next/cache";

/**
 * Defines the state of the action for updating paired goals, including success status and any validation errors encountered during the update process.
 */
interface ActionState {
  readonly success?: boolean;
  readonly errorTitle?: string | null;
  readonly assignmentTypeErrors?: string | null;
  readonly assignmentGoalAmountErrors?: string | null;
  readonly spendingTypeErrors?: string | null;
  readonly unmappedErrors?: string | null;
}

/**
 * Defines the payload structure for the updateGoalPair action, including the assignment and spending goals, their respective update requests, and the redirect URL to navigate to upon successful update.
 */
interface ActionPayload {
  readonly assignmentGoal: AssignmentGoal;
  readonly assignmentRequest: UpdateAssignmentGoalRequest;
  readonly spendingGoal: SpendingGoal;
  readonly spendingRequest: UpdateSpendingGoalRequest;
  readonly redirectUrl: string;
}

/**
 * Maps an error updating the assignment goal.
 */
const mapAssignmentError = function (error: unknown): ActionState {
  if (!isApiError(error)) {
    throw new Error("An unexpected error occurred", { cause: error });
  }
  const mapped = mapApiValidationError(error, {
    [propertyName<UpdateAssignmentGoalRequest>("assignmentGoalType")]:
      "assignmentTypeErrors",
    [propertyName<UpdateAssignmentGoalRequest>("goalAmount")]:
      "assignmentGoalAmountErrors",
  });
  return { ...mapped, ...mapped.fieldErrors };
};

/**
 * Maps an error updating the spending goal.
 */
const mapSpendingError = function (error: unknown): ActionState {
  if (!isApiError(error)) {
    throw new Error("An unexpected error occurred", { cause: error });
  }
  const mapped = mapApiValidationError(error, {
    [propertyName<UpdateSpendingGoalRequest>("spendingGoalType")]:
      "spendingTypeErrors",
  });
  return { ...mapped, ...mapped.fieldErrors };
};

/**
 * Updates both paired goal configurations with one form action.
 */
const updateGoalPair = async function (
  _: ActionState,
  payload: ActionPayload,
): Promise<ActionState> {
  const client = createApiClient();
  const assignmentResponse = await client.POST("/goals/assignment/{goalId}", {
    params: { path: { goalId: payload.assignmentGoal.id } },
    body: payload.assignmentRequest,
  });
  if (assignmentResponse.error) {
    return mapAssignmentError(assignmentResponse.error);
  }

  const spendingResponse = await client.POST("/goals/spending/{goalId}", {
    params: { path: { goalId: payload.spendingGoal.id } },
    body: payload.spendingRequest,
  });
  if (spendingResponse.error) {
    return mapSpendingError(spendingResponse.error);
  }

  revalidatePath(payload.redirectUrl);
  return { success: true };
};

export default updateGoalPair;
