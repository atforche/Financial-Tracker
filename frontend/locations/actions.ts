"use server";

import type { Location } from "@/locations/types";
import createApiClient from "@/framework/data/createApiClient";
import { isApiError } from "@/framework/data/apiError";
import mapApiValidationError from "@/framework/forms/mapApiValidationError";
import { revalidatePath } from "next/cache";

/**
 * Describes why a location request failed, using the API's messages when it sent any. Server actions return this
 * rather than throwing, because production builds hide a thrown error's message.
 */
const describeFailure = function (error: unknown, fallback: string): string {
  if (!isApiError(error)) {
    return fallback;
  }
  const { errorTitle, unmappedErrors } = mapApiValidationError(error, {});
  return unmappedErrors ?? errorTitle ?? fallback;
};

/**
 * Renames a location, returning why it failed or null on success.
 */
const renameLocation = async function (
  locationId: string,
  name: string,
): Promise<string | null> {
  const client = await createApiClient();
  const { error } = await client.POST("/locations/{locationId}", {
    params: { path: { locationId } },
    body: { name: name.trim() },
  });
  if (error !== undefined) {
    return describeFailure(error, "Unable to rename this location.");
  }
  revalidatePath("/locations", "layout");
  return null;
};

/**
 * Deletes a location that no transaction uses, returning why it failed or null on success.
 */
const deleteLocation = async function (
  locationId: string,
): Promise<string | null> {
  const client = await createApiClient();
  const { error } = await client.DELETE("/locations/{locationId}", {
    params: { path: { locationId } },
  });
  if (error !== undefined) {
    return describeFailure(
      error,
      "This location cannot be deleted while transactions use it.",
    );
  }
  revalidatePath("/locations", "layout");
  return null;
};

/**
 * Moves every transaction from one location to another and removes the first, returning why it failed or null on
 * success.
 */
const mergeLocation = async function (
  sourceLocationId: string,
  targetLocationId: string,
): Promise<string | null> {
  const client = await createApiClient();
  const { error } = await client.POST(
    "/locations/{sourceLocationId}/consolidate",
    {
      params: { path: { sourceLocationId } },
      body: { targetLocationId },
    },
  );
  if (error !== undefined) {
    return describeFailure(error, "Unable to merge these locations.");
  }
  revalidatePath("/locations", "layout");
  return null;
};

/**
 * Gets every location, for choosing which one to merge into.
 */
const getLocationOptions = async function (): Promise<Location[]> {
  const client = await createApiClient();
  const { data } = await client.GET("/locations");
  return data?.items ?? [];
};

export { deleteLocation, getLocationOptions, mergeLocation, renameLocation };
