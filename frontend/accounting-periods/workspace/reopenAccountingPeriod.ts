"use server";

import formatErrors from "@/framework/forms/formatErrors";
import getApiClient from "@/framework/data/getApiClient";
import { isApiError } from "@/framework/data/apiError";
import { revalidatePath } from "next/cache";

/**
 * Interface representing the state of reopening an accounting period.
 */
interface ActionState {
  readonly errorTitle?: string | null;
  readonly unmappedErrors?: string | null;
}

/**
 * Payload for the reopen accounting period server action.
 */
interface ActionPayload {
  readonly accountingPeriodId: string;
  readonly redirectUrl: string;
}

/**
 * Server action that reopens an existing accounting period.
 */
const reopenAccountingPeriod = async function (
  _: ActionState,
  { accountingPeriodId, redirectUrl }: ActionPayload,
): Promise<ActionState> {
  const client = getApiClient();
  const { error } = await client.POST(
    "/accounting-periods/{accountingPeriodId}/reopen",
    {
      params: {
        path: {
          accountingPeriodId,
        },
      },
    },
  );
  if (error) {
    if (isApiError(error)) {
      const unmappedErrors: (string | null)[] = [];
      for (const key of Object.keys(error.errors ?? {})) {
        unmappedErrors.push(formatErrors(error.errors?.[key] ?? null));
      }
      return {
        errorTitle: error.title ?? null,
        unmappedErrors: unmappedErrors.join(", ") || null,
      };
    }
    throw new Error("An unexpected error occurred", { cause: error });
  }
  revalidatePath(redirectUrl);
  return {};
};

export default reopenAccountingPeriod;
