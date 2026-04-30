"use server";

import type { CreateGoalRequest } from "@/data/goalTypes";
import formatErrors from "@/framework/forms/formatErrors";
import getApiClient from "@/data/getApiClient";
import { isApiError } from "@/data/apiError";
import nameof from "@/data/nameof";
import { redirect } from "next/navigation";
import { revalidatePath } from "next/cache";

/**
 * Interface representing the state of creating a goal.
 */
interface ActionState {
  readonly fundId: string;
  readonly redirectUrl: string;
  readonly errorTitle?: string | null;
  readonly accountingPeriodErrors?: string | null;
  readonly goalTypeErrors?: string | null;
  readonly goalAmountErrors?: string | null;
  readonly unmappedErrors?: string | null;
}

/**
 * Server action that creates a goal.
 */
const createGoal = async function (
  { fundId, redirectUrl }: ActionState,
  request: CreateGoalRequest,
): Promise<ActionState> {
  const client = getApiClient();
  const { error } = await client.POST("/goals", {
    body: request,
  });
  if (error) {
    if (isApiError(error)) {
      let accountingPeriodErrorMessage = null;
      let goalTypeErrorMessage = null;
      let goalAmountErrorMessage = null;
      const unmappedErrors: (string | null)[] = [];
      for (const key of Object.keys(error.errors ?? {})) {
        if (
          key.toUpperCase() ===
          nameof<CreateGoalRequest>("accountingPeriodId").toUpperCase()
        ) {
          accountingPeriodErrorMessage = formatErrors(
            error.errors?.[key] ?? null,
          );
        } else if (
          key.toUpperCase() ===
          nameof<CreateGoalRequest>("goalType").toUpperCase()
        ) {
          goalTypeErrorMessage = formatErrors(error.errors?.[key] ?? null);
        } else if (
          key.toUpperCase() ===
          nameof<CreateGoalRequest>("goalAmount").toUpperCase()
        ) {
          goalAmountErrorMessage = formatErrors(error.errors?.[key] ?? null);
        } else {
          unmappedErrors.push(formatErrors(error.errors?.[key] ?? null));
        }
      }
      return {
        fundId,
        redirectUrl,
        errorTitle: error.title ?? null,
        accountingPeriodErrors: accountingPeriodErrorMessage,
        goalTypeErrors: goalTypeErrorMessage,
        goalAmountErrors: goalAmountErrorMessage,
        unmappedErrors: unmappedErrors.join(", ") || null,
      };
    }
    throw new Error("An unexpected error occurred", { cause: error });
  }

  revalidatePath(redirectUrl);
  redirect(redirectUrl);
};

export default createGoal;
