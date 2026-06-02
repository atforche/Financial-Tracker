"use server";

import type { CreateAccountRequest } from "@/accounts/types";
import formatErrors from "@/framework/forms/formatErrors";
import getApiClient from "@/framework/data/getApiClient";
import { isApiError } from "@/framework/data/apiError";
import nameof from "@/framework/data/nameof";
import { redirect } from "next/navigation";
import { revalidatePath } from "next/cache";
import routes from "@/accounts/routes";

/**
 * Interface representing the state of creating an account.
 */
interface ActionState {
  readonly errorTitle?: string | null;
  readonly nameErrors?: string | null;
  readonly typeErrors?: string | null;
  readonly accountingPeriodErrors?: string | null;
  readonly dateOpenedErrors?: string | null;
  readonly unmappedErrors?: string | null;
}

/**
 * Payload for the create account server action.
 */
interface ActionPayload {
  readonly redirectUrl: string;
  readonly request: CreateAccountRequest;
}

/**
 * Ensures redirects stay within the app.
 */
const getSafeRedirectUrl = function (redirectUrl: string): string {
  if (redirectUrl.startsWith("/") && !redirectUrl.startsWith("//")) {
    return redirectUrl;
  }

  return routes.index({});
};

/**
 * Server action that creates a new account.
 */
const createAccount = async function (
  _: ActionState,
  { redirectUrl, request }: ActionPayload,
): Promise<ActionState> {
  const safeRedirectUrl = getSafeRedirectUrl(redirectUrl);
  const client = getApiClient();
  const { error } = await client.POST("/accounts", {
    body: request,
  });
  if (error) {
    if (isApiError(error)) {
      let nameErrorMessage = null;
      let typeErrorMessage = null;
      let accountingPeriodErrorMessage = null;
      let dateOpenedErrorMessage = null;
      const unmappedErrors: (string | null)[] = [];
      for (const key of Object.keys(error.errors ?? {})) {
        if (
          key.toUpperCase() ===
          nameof<CreateAccountRequest>("name").toUpperCase()
        ) {
          nameErrorMessage = formatErrors(error.errors?.[key] ?? null);
        } else if (
          key.toUpperCase() ===
          nameof<CreateAccountRequest>("type").toUpperCase()
        ) {
          typeErrorMessage = formatErrors(error.errors?.[key] ?? null);
        } else if (
          key.toUpperCase() ===
          nameof<CreateAccountRequest>(
            "openingAccountingPeriodId",
          ).toUpperCase()
        ) {
          accountingPeriodErrorMessage = formatErrors(
            error.errors?.[key] ?? null,
          );
        } else if (
          key.toUpperCase() ===
          nameof<CreateAccountRequest>("dateOpened").toUpperCase()
        ) {
          dateOpenedErrorMessage = formatErrors(error.errors?.[key] ?? null);
        } else {
          unmappedErrors.push(formatErrors(error.errors?.[key] ?? null));
        }
      }

      return {
        errorTitle: error.title ?? null,
        nameErrors: nameErrorMessage,
        typeErrors: typeErrorMessage,
        accountingPeriodErrors: accountingPeriodErrorMessage,
        dateOpenedErrors: dateOpenedErrorMessage,
        unmappedErrors: unmappedErrors.join(", ") || null,
      };
    }
    throw new Error("An unexpected error occurred", { cause: error });
  }

  revalidatePath(safeRedirectUrl);
  redirect(safeRedirectUrl);
};

export default createAccount;
