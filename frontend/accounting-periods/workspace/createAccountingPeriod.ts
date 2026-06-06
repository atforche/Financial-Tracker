"use server";

import type { CreateAccountingPeriodRequest } from "@/accounting-periods/types";
import formatErrors from "@/framework/forms/formatErrors";
import getApiClient from "@/framework/data/getApiClient";
import { isApiError } from "@/framework/data/apiError";
import nameof from "@/framework/data/nameof";
import { revalidatePath } from "next/cache";

/**
 * Interface representing the state of creating an accounting period.
 */
interface ActionState {
  readonly success?: boolean;
  readonly errorTitle?: string | null;
  readonly yearErrors?: string | null;
  readonly monthErrors?: string | null;
  readonly unmappedErrors?: string | null;
}

/**
 * Payload for the create accounting period server action.
 */
interface ActionPayload {
  readonly redirectUrl: string;
  readonly request: CreateAccountingPeriodRequest;
}

/**
 * Server action that creates a new accounting period.
 */
const createAccountingPeriod = async function (
  _: ActionState,
  { redirectUrl, request }: ActionPayload,
): Promise<ActionState> {
  const client = getApiClient();
  const { error } = await client.POST("/accounting-periods", {
    body: request,
  });
  if (error) {
    if (isApiError(error)) {
      let yearErrorMessage = null;
      let monthErrorMessage = null;
      const unmappedErrors: (string | null)[] = [];
      for (const key of Object.keys(error.errors ?? {})) {
        if (
          key.toUpperCase() ===
          nameof<CreateAccountingPeriodRequest>("year").toUpperCase()
        ) {
          yearErrorMessage = formatErrors(error.errors?.[key] ?? null);
        } else if (
          key.toUpperCase() ===
          nameof<CreateAccountingPeriodRequest>("month").toUpperCase()
        ) {
          monthErrorMessage = formatErrors(error.errors?.[key] ?? null);
        } else {
          unmappedErrors.push(formatErrors(error.errors?.[key] ?? null));
        }
      }
      return {
        errorTitle: error.title ?? null,
        unmappedErrors: formatErrors(
          unmappedErrors.filter((e): e is string => e !== null),
        ),
        yearErrors: yearErrorMessage,
        monthErrors: monthErrorMessage,
      };
    }
    throw new Error("An unexpected error occurred", { cause: error });
  }
  revalidatePath(redirectUrl);
  return { success: true };
};

export default createAccountingPeriod;
