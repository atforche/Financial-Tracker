"use server";

import type { FundGoal, UpdateFundGoalRequest } from "@/fund-goals/types";
import createApiClient from "@/framework/data/createApiClient";
import { isApiError } from "@/framework/data/apiError";
import mapApiValidationError from "@/framework/forms/mapApiValidationError";
import propertyName from "@/framework/data/propertyName";
import { revalidatePath } from "next/cache";

/**
 * State of the updateFundGoal action.
 */
interface ActionState {
  readonly success?: boolean;
  readonly errorTitle?: string | null;
  readonly plannedMonthlyContributionErrors?: string | null;
  readonly minimumEndingBalanceErrors?: string | null;
  readonly maximumEndingBalanceErrors?: string | null;
  readonly unmappedErrors?: string | null;
}

/**
 * Payload for the updateFundGoal action.
 */
interface ActionPayload {
  readonly fundGoal: FundGoal;
  readonly request: UpdateFundGoalRequest;
  readonly redirectUrl: string;
}

/**
 * Updates a Fund Goal with the provided request data and revalidates the specified path.
 */
const updateFundGoal = async function (
  _: ActionState,
  payload: ActionPayload,
): Promise<ActionState> {
  const apiClient = await createApiClient();
  const response = await apiClient.POST("/fund-goals/{fundGoalId}", {
    params: { path: { fundGoalId: payload.fundGoal.id } },
    body: payload.request,
  });
  const error: unknown = response.error;
  if (error !== undefined && error !== null) {
    if (!isApiError(error)) {
      throw new Error("An unexpected error occurred", { cause: error });
    }
    const mappedError = mapApiValidationError(error, {
      [propertyName<UpdateFundGoalRequest>("plannedMonthlyContribution")]:
        "plannedMonthlyContributionErrors",
      [propertyName<UpdateFundGoalRequest>("minimumEndingBalance")]:
        "minimumEndingBalanceErrors",
      [propertyName<UpdateFundGoalRequest>("maximumEndingBalance")]:
        "maximumEndingBalanceErrors",
    });
    return { ...mappedError, ...mappedError.fieldErrors };
  }
  revalidatePath(payload.redirectUrl);
  return { success: true };
};

export default updateFundGoal;
