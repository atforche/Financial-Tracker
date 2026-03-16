"use server";

import type { CreateAccountingPeriodRequest } from "@/data/accountingPeriodTypes";
import type StateElement from "@/framework/forms/StateElement";
import formatErrors from "@/framework/forms/formatErrors";
import getApiClient from "@/data/getApiClient";
import { isApiError } from "@/data/apiError";
import nameof from "@/data/nameof";
import { redirect } from "next/navigation";
import { revalidatePath } from "next/cache";
import { z } from "zod";

/**
 * Interface representing the state of creating an accounting period.
 */
interface ActionState {
  readonly errorTitle?: string | null;
  readonly unmappedErrors?: string | null;
  readonly year?: StateElement<number | null>;
  readonly month?: StateElement<number | null>;
}

/**
 * Schema for validation of the form data when creating a new accounting period.
 */
const FormSchema = z.object({
  year: z.number(),
  month: z.number(),
});

/**
 * Server action that creates a new accounting period.
 */
const createAccountingPeriod = async function (
  _: ActionState,
  formData: FormData,
): Promise<ActionState> {
  const { year, month } = FormSchema.parse({
    year: Number(formData.get("year")),
    month: Number(formData.get("month")),
  });

  const client = getApiClient();
  const { error } = await client.POST("/accounting-periods", {
    body: {
      year,
      month,
    },
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
        year: {
          value: year,
          errorMessage: yearErrorMessage,
        },
        month: {
          value: month,
          errorMessage: monthErrorMessage,
        },
      };
    }
    throw new Error("An unexpected error occurred", { cause: error });
  }

  revalidatePath("/accounting-periods");
  redirect("/accounting-periods");
};

export default createAccountingPeriod;
