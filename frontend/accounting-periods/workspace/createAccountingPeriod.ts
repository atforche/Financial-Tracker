"use server";

import type { CreateAccountingPeriodRequest } from "@/accounting-periods/types";
import createApiClient from "@/framework/data/createApiClient";
import mapActionError from "@/framework/forms/mapActionError";
import propertyName from "@/framework/data/propertyName";
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
  const client = await createApiClient();
  const { error } = await client.POST("/accounting-periods", {
    body: request,
  });
  if (error) {
    return mapActionError(error, {
      [propertyName<CreateAccountingPeriodRequest>("year")]: "yearErrors",
      [propertyName<CreateAccountingPeriodRequest>("month")]: "monthErrors",
    });
  }
  revalidatePath(redirectUrl);
  return { success: true };
};

export default createAccountingPeriod;
