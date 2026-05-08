"use server";

import type { CreateGoalRequest } from "@/goals/types";
import formatErrors from "@/framework/forms/formatErrors";
import getApiClient from "@/framework/data/getApiClient";
import { isApiError } from "@/framework/data/apiError";
import nameof from "@/framework/data/nameof";
import { redirect } from "next/navigation";
import { revalidatePath } from "next/cache";

/**
 * Interface representing the state of creating a goal.
 */
interface ActionState {
  readonly redirectUrl: string;
  readonly errorTitle?: string | null;
  readonly fundErrors?: string | null;
  readonly accountingPeriodErrors?: string | null;
  readonly goalTypeErrors?: string | null;
  readonly goalAmountErrors?: string | null;
  readonly unmappedErrors?: string | null;
}

/**
 * Server action that creates a goal.
 */
const createGoal = async function (
  { redirectUrl }: ActionState,
  request: CreateGoalRequest,
): Promise<ActionState> {
  const client = getApiClient();
  const { data, error } = await client.POST("/goals", {
    body: request,
  });
  if (error) {
    if (isApiError(error)) {
      let fundErrorMessage = null;
      let accountingPeriodErrorMessage = null;
      let goalTypeErrorMessage = null;
      let goalAmountErrorMessage = null;
      const unmappedErrors: (string | null)[] = [];
      for (const key of Object.keys(error.errors ?? {})) {
        if (
          key.toUpperCase() ===
          nameof<CreateGoalRequest>("fundId").toUpperCase()
        ) {
          fundErrorMessage = formatErrors(error.errors?.[key] ?? null);
        } else if (
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
        redirectUrl,
        errorTitle: error.title ?? null,
        fundErrors: fundErrorMessage,
        accountingPeriodErrors: accountingPeriodErrorMessage,
        goalTypeErrors: goalTypeErrorMessage,
        goalAmountErrors: goalAmountErrorMessage,
        unmappedErrors: unmappedErrors.join(", ") || null,
      };
    }
    throw new Error("An unexpected error occurred", { cause: error });
  }

  if (typeof data === "undefined") {
    throw new Error("Goal creation did not return a Goal.");
  }

  revalidatePath(redirectUrl);
  redirect(redirectUrl);
};

export default createGoal;
