"use server";

import type { CreateAccountingPeriodRequest } from "@/data/accountingPeriodTypes";
import formatErrors from "@/framework/forms/formatErrors";
import getApiClient from "@/data/getApiClient";
import { isApiError } from "@/data/apiError";
import nameof from "@/data/nameof";
import { redirect } from "next/navigation";
import { revalidatePath } from "next/cache";
import routes from "@/framework/routes";

/**
 * Interface representing the state of creating an accounting period.
 */
interface ActionState {
  readonly errorTitle?: string | null;
  readonly yearErrors?: string | null;
  readonly monthErrors?: string | null;
  readonly unmappedErrors?: string | null;
}

/**
 * Server action that creates a new accounting period.
 */
const createAccountingPeriod = async function (
  _: ActionState,
  request: CreateAccountingPeriodRequest,
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

  revalidatePath(routes.accountingPeriods.index);
  redirect(routes.accountingPeriods.index);
};

export default createAccountingPeriod;
