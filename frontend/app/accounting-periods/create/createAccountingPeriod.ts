"use server";

import type { CreateAccountingPeriodRequest } from "@/data/accountingPeriodTypes";
import getApiClient from "@/data/getApiClient";
import { isApiError } from "@/data/apiError";
import nameof from "@/data/nameof";
import { redirect } from "next/navigation";
import { revalidatePath } from "next/cache";

/**
 * Interface representing the state of creating an accounting period.
 */
interface CreateAccountingPeriodState {
  readonly overallErrorMessage?: string | null;
  readonly year?: number | null;
  readonly yearErrorMessage?: string | null;
  readonly month?: number | null;
  readonly monthErrorMessage?: string | null;
}

/**
 * Server action that creates a new accounting period.
 */
const createAccountingPeriod = async function (
  _: CreateAccountingPeriodState,
  formData: FormData,
): Promise<CreateAccountingPeriodState> {
  const year = formData.get(nameof<CreateAccountingPeriodRequest>("year"));
  const month = formData.get(nameof<CreateAccountingPeriodRequest>("month"));

  const client = getApiClient();
  const { error } = await client.POST("/accounting-periods", {
    body: {
      year: Number(year),
      month: Number(month),
    },
  });
  if (error) {
    if (isApiError(error)) {
      let yearErrorMessage = null;
      let monthErrorMessage = null;
      for (const key of Object.keys(error.errors ?? {})) {
        if (
          key.toUpperCase() ===
          nameof<CreateAccountingPeriodRequest>("year").toUpperCase()
        ) {
          yearErrorMessage = error.errors?.[key]?.[0] ?? null;
        }
        if (
          key.toUpperCase() ===
          nameof<CreateAccountingPeriodRequest>("month").toUpperCase()
        ) {
          monthErrorMessage = error.errors?.[key]?.[0] ?? null;
        }
      }
      return {
        overallErrorMessage: error.title ?? null,
        year: Number(year),
        yearErrorMessage,
        month: Number(month),
        monthErrorMessage,
      };
    }
    throw new Error("An unexpected error occurred", { cause: error });
  }

  revalidatePath("/accounting-periods");
  redirect("/accounting-periods");
};

export default createAccountingPeriod;
