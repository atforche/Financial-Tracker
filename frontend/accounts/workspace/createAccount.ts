"use server";

import type {
  AccountActionPayload,
  AccountActionState,
} from "@/accounts/workspace/accountAction";
import type { CreateAccountRequest } from "@/accounts/types";
import createApiClient from "@/framework/data/createApiClient";
import { isApiError } from "@/framework/data/apiError";
import mapApiValidationError from "@/framework/forms/mapApiValidationError";
import propertyName from "@/framework/data/propertyName";
import { revalidatePath } from "next/cache";

/**
 * Interface representing the state of creating an account.
 */
interface ActionState extends AccountActionState {
  readonly nameErrors?: string | null;
  readonly typeErrors?: string | null;
  readonly accountingPeriodErrors?: string | null;
  readonly dateOpenedErrors?: string | null;
}

/**
 * Payload for the create account server action.
 */
interface ActionPayload extends AccountActionPayload {
  readonly request: CreateAccountRequest;
}

/**
 * Server action that creates a new account.
 */
const createAccount = async function (
  _: ActionState,
  { redirectUrl, request }: ActionPayload,
): Promise<ActionState> {
  const apiClient = createApiClient();
  const { error } = await apiClient.POST("/accounts", {
    body: request,
  });
  if (error) {
    if (isApiError(error)) {
      const formattedError = mapApiValidationError(error, {
        [propertyName<CreateAccountRequest>("name")]: "nameErrors",
        [propertyName<CreateAccountRequest>("type")]: "typeErrors",
        [propertyName<CreateAccountRequest>("openingAccountingPeriodId")]:
          "accountingPeriodErrors",
        [propertyName<CreateAccountRequest>("dateOpened")]: "dateOpenedErrors",
      });
      return {
        errorTitle: formattedError.errorTitle,
        nameErrors: formattedError.fieldErrors.nameErrors ?? null,
        typeErrors: formattedError.fieldErrors.typeErrors ?? null,
        accountingPeriodErrors:
          formattedError.fieldErrors.accountingPeriodErrors ?? null,
        dateOpenedErrors: formattedError.fieldErrors.dateOpenedErrors ?? null,
        unmappedErrors: formattedError.unmappedErrors,
      };
    }
    throw new Error("An unexpected error occurred", { cause: error });
  }
  revalidatePath(redirectUrl);
  return { success: true };
};

export default createAccount;
