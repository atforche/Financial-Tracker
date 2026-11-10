"use server";

import type {
  WorkspaceActionPayload,
  WorkspaceActionState,
} from "@/framework/forms/workspaceAction";
import type { CreateAccountRequest } from "@/accounts/types";
import createApiClient from "@/framework/data/createApiClient";
import mapActionError from "@/framework/forms/mapActionError";
import propertyName from "@/framework/data/propertyName";
import { revalidatePath } from "next/cache";

/**
 * Interface representing the state of creating an account.
 */
interface ActionState extends WorkspaceActionState {
  readonly nameErrors?: string | null;
  readonly typeErrors?: string | null;
  readonly minimumEndingBalanceErrors?: string | null;
  readonly maximumEndingBalanceErrors?: string | null;
  readonly accountingPeriodErrors?: string | null;
  readonly dateOpenedErrors?: string | null;
}

/**
 * Payload for the create account server action.
 */
interface ActionPayload extends WorkspaceActionPayload {
  readonly request: CreateAccountRequest;
}

/**
 * Server action that creates a new account.
 */
const createAccount = async function (
  _: ActionState,
  { redirectUrl, request }: ActionPayload,
): Promise<ActionState> {
  const apiClient = await createApiClient();
  const { error } = await apiClient.POST("/accounts", {
    body: request,
  });
  if (error) {
    return mapActionError(error, {
      [propertyName<CreateAccountRequest>("name")]: "nameErrors",
      [propertyName<CreateAccountRequest>("type")]: "typeErrors",
      [propertyName<CreateAccountRequest>("minimumEndingBalance")]:
        "minimumEndingBalanceErrors",
      [propertyName<CreateAccountRequest>("maximumEndingBalance")]:
        "maximumEndingBalanceErrors",
      [propertyName<CreateAccountRequest>("openingAccountingPeriodId")]:
        "accountingPeriodErrors",
      [propertyName<CreateAccountRequest>("dateOpened")]: "dateOpenedErrors",
    });
  }
  revalidatePath(redirectUrl);
  return { success: true };
};

export default createAccount;
