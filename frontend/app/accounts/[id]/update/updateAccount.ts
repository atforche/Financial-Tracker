"use server";

import type { UpdateAccountRequest } from "@/data/accountTypes";
import formatErrors from "@/framework/forms/formatErrors";
import getApiClient from "@/data/getApiClient";
import { isApiError } from "@/data/apiError";
import nameof from "@/data/nameof";
import { redirect } from "next/navigation";
import { revalidatePath } from "next/cache";

/**
 * Interface representing the state of updating an account.
 */
interface ActionState {
  readonly accountId: string;
  readonly redirectUrl: string;
  readonly errorTitle?: string | null;
  readonly nameErrors?: string | null;
  readonly unmappedErrors?: string | null;
}

/**
 * Server action that updates an existing account.
 */
const updateAccount = async function (
  { accountId, redirectUrl }: ActionState,
  request: UpdateAccountRequest,
): Promise<ActionState> {
  const client = getApiClient();
  const { error } = await client.POST("/accounts/{accountId}", {
    params: {
      path: {
        accountId,
      },
    },
    body: request,
  });
  if (error) {
    if (isApiError(error)) {
      let nameErrorMessage = null;
      const unmappedErrors: (string | null)[] = [];
      for (const key of Object.keys(error.errors ?? {})) {
        if (
          key.toUpperCase() ===
          nameof<UpdateAccountRequest>("name").toUpperCase()
        ) {
          nameErrorMessage = formatErrors(error.errors?.[key] ?? null);
        } else {
          unmappedErrors.push(formatErrors(error.errors?.[key] ?? null));
        }
      }
      return {
        accountId,
        redirectUrl,
        errorTitle: error.title ?? null,
        nameErrors: nameErrorMessage,
        unmappedErrors: unmappedErrors.join(", ") || null,
      };
    }
    throw new Error("An unexpected error occurred", { cause: error });
  }

  revalidatePath(redirectUrl);
  redirect(redirectUrl);
};

export default updateAccount;
