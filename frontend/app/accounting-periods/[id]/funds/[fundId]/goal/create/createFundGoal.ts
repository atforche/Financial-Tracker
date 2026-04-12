"use server";

import type { CreateFundGoalRequest } from "@/data/fundTypes";
import formatErrors from "@/framework/forms/formatErrors";
import getApiClient from "@/data/getApiClient";
import { isApiError } from "@/data/apiError";
import nameof from "@/data/nameof";
import { redirect } from "next/navigation";
import { revalidatePath } from "next/cache";

/**
 * Interface representing the state of creating a fund goal.
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
 * Server action that creates a fund goal.
 */
const createFundGoal = async function (
  { fundId, redirectUrl }: ActionState,
  request: CreateFundGoalRequest,
): Promise<ActionState> {
  const client = getApiClient();
  const { error } = await client.POST("/funds/{fundId}/goals", {
    params: {
      path: {
        fundId,
      },
    },
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
          nameof<CreateFundGoalRequest>("accountingPeriodId").toUpperCase()
        ) {
          accountingPeriodErrorMessage = formatErrors(
            error.errors?.[key] ?? null,
          );
        } else if (
          key.toUpperCase() ===
          nameof<CreateFundGoalRequest>("goalType").toUpperCase()
        ) {
          goalTypeErrorMessage = formatErrors(error.errors?.[key] ?? null);
        } else if (
          key.toUpperCase() ===
          nameof<CreateFundGoalRequest>("goalAmount").toUpperCase()
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

export default createFundGoal;
