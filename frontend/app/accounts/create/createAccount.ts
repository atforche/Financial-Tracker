"use server";

import type { CreateAccountRequest } from "@/data/accountTypes";
import formatErrors from "@/framework/forms/formatErrors";
import getApiClient from "@/data/getApiClient";
import { isApiError } from "@/data/apiError";
import nameof from "@/data/nameof";
import { redirect } from "next/navigation";
import { revalidatePath } from "next/cache";

/**
 * Interface representing the state of creating an account.
 */
interface ActionState {
  readonly redirectUrl: string;
  readonly errorTitle?: string | null;
  readonly nameErrors?: string | null;
  readonly typeErrors?: string | null;
  readonly accountingPeriodErrors?: string | null;
  readonly addDateErrors?: string | null;
  readonly initialBalanceErrors?: string | null;
  readonly initialFundAssignmentsErrors?: string | null;
  readonly unmappedErrors?: string | null;
}

/**
 * Server action that creates a new account.
 */
const createAccount = async function (
  { redirectUrl }: ActionState,
  request: CreateAccountRequest,
): Promise<ActionState> {
  const client = getApiClient();
  const { error } = await client.POST("/accounts", {
    body: request,
  });
  if (error) {
    if (isApiError(error)) {
      let nameErrorMessage = null;
      let typeErrorMessage = null;
      let accountingPeriodErrorMessage = null;
      let addDateErrorMessage = null;
      let initialBalanceErrorMessage = null;
      let initialFundAssignmentsErrorMessage = null;
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
          nameof<CreateAccountRequest>("accountingPeriodId").toUpperCase()
        ) {
          accountingPeriodErrorMessage = formatErrors(
            error.errors?.[key] ?? null,
          );
        } else if (
          key.toUpperCase() ===
          nameof<CreateAccountRequest>("addDate").toUpperCase()
        ) {
          addDateErrorMessage = formatErrors(error.errors?.[key] ?? null);
        } else if (
          key.toUpperCase() ===
          nameof<CreateAccountRequest>("initialBalance").toUpperCase()
        ) {
          initialBalanceErrorMessage = formatErrors(
            error.errors?.[key] ?? null,
          );
        } else if (
          key.toUpperCase() ===
          nameof<CreateAccountRequest>("initialFundAssignments").toUpperCase()
        ) {
          initialFundAssignmentsErrorMessage = formatErrors(
            error.errors?.[key] ?? null,
          );
        } else {
          unmappedErrors.push(formatErrors(error.errors?.[key] ?? null));
        }
      }

      return {
        redirectUrl,
        errorTitle: error.title ?? null,
        nameErrors: nameErrorMessage,
        typeErrors: typeErrorMessage,
        accountingPeriodErrors: accountingPeriodErrorMessage,
        addDateErrors: addDateErrorMessage,
        initialBalanceErrors: initialBalanceErrorMessage,
        initialFundAssignmentsErrors: initialFundAssignmentsErrorMessage,
        unmappedErrors: unmappedErrors.join(", ") || null,
      };
    }
    throw new Error("An unexpected error occurred", { cause: error });
  }

  revalidatePath(redirectUrl);
  redirect(redirectUrl);
};

export default createAccount;
