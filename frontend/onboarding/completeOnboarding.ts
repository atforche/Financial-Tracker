"use server";

import type { components } from "@/framework/data/api";
import formatErrors from "@/framework/forms/formatErrors";
import getApiClient from "@/framework/data/getApiClient";
import { isApiError } from "@/framework/data/apiError";
import { redirect } from "next/navigation";
import { revalidatePath } from "next/cache";

interface ActionState {
  readonly errorTitle?: string | null;
  readonly unmappedErrors?: string | null;
}

/**
 * Completes onboarding with an empty initial setup.
 */
const completeOnboarding = async function (): Promise<ActionState> {
  const client = getApiClient();
  const request: components["schemas"]["OnboardingModel"] = {
    accounts: [],
    funds: [],
  };

  const { error } = await client.POST("/onboarding", {
    body: request,
  });
  if (error) {
    if (isApiError(error)) {
      const unmappedErrors = Object.keys(error.errors ?? {})
        .map((key) => formatErrors(error.errors?.[key] ?? null))
        .filter((message): message is string => message !== null)
        .join(", ");

      return {
        errorTitle: error.title ?? null,
        unmappedErrors: unmappedErrors || null,
      };
    }

    throw new Error("An unexpected error occurred", { cause: error });
  }

  revalidatePath("/");
  redirect("/");
};

export default completeOnboarding;
