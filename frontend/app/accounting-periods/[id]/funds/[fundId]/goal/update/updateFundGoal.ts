"use server";

import type { UpdateFundGoalRequest } from "@/data/fundTypes";
import formatErrors from "@/framework/forms/formatErrors";
import getApiClient from "@/data/getApiClient";
import { isApiError } from "@/data/apiError";
import nameof from "@/data/nameof";
import { redirect } from "next/navigation";
import { revalidatePath } from "next/cache";

/**
 * Interface representing the state of updating a fund goal.
 */
interface ActionState {
  readonly fundId: string;
  readonly accountingPeriodId: string;
  readonly redirectUrl: string;
  readonly errorTitle?: string | null;
  readonly goalTypeErrors?: string | null;
  readonly goalAmountErrors?: string | null;
  readonly unmappedErrors?: string | null;
}

/**
 * Server action that updates a fund goal.
 */
const updateFundGoal = async function (
  { fundId, accountingPeriodId, redirectUrl }: ActionState,
  request: UpdateFundGoalRequest,
): Promise<ActionState> {
  const client = getApiClient();
  const { error } = await client.POST(
    "/funds/{fundId}/goals/{accountingPeriodId}",
    {
      params: {
        path: {
          fundId,
          accountingPeriodId,
        },
      },
      body: request,
    },
  );
  if (error) {
    if (isApiError(error)) {
      let goalTypeErrorMessage = null;
      let goalAmountErrorMessage = null;
      const unmappedErrors: (string | null)[] = [];
      for (const key of Object.keys(error.errors ?? {})) {
        if (
          key.toUpperCase() ===
          nameof<UpdateFundGoalRequest>("goalType").toUpperCase()
        ) {
          goalTypeErrorMessage = formatErrors(error.errors?.[key] ?? null);
        } else if (
          key.toUpperCase() ===
          nameof<UpdateFundGoalRequest>("goalAmount").toUpperCase()
        ) {
          goalAmountErrorMessage = formatErrors(error.errors?.[key] ?? null);
        } else {
          unmappedErrors.push(formatErrors(error.errors?.[key] ?? null));
        }
      }
      return {
        fundId,
        accountingPeriodId,
        redirectUrl,
        errorTitle: error.title ?? null,
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

export default updateFundGoal;
