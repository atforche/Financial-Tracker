"use server";

import createApiClient from "@/framework/data/createApiClient";
import { revalidatePath } from "next/cache";

/** Gets a required string value from submitted form data. */
const getRequiredString = function (formData: FormData, name: string): string {
  const value = formData.get(name);
  if (typeof value !== "string" || value.trim() === "") {
    throw new Error(`${name} is required.`);
  }
  return value.trim();
};

/** Renames an existing Location. */
const renameLocation = async function (formData: FormData): Promise<void> {
  const id = getRequiredString(formData, "id");
  const name = getRequiredString(formData, "name");
  const client = await createApiClient();
  const response = await client.POST("/locations/{locationId}", {
    params: { path: { locationId: id } },
    body: { name },
  });
  if (response.error) {
    throw new Error("Unable to rename Location.");
  }
  revalidatePath("/locations");
};

/** Deletes an unused Location. */
const deleteLocation = async function (formData: FormData): Promise<void> {
  const id = getRequiredString(formData, "id");
  const client = await createApiClient();
  const response = await client.DELETE("/locations/{locationId}", {
    params: { path: { locationId: id } },
  });
  if (response.error) {
    throw new Error(
      "This Location cannot be deleted while transactions use it.",
    );
  }
  revalidatePath("/locations");
};

/** Consolidates a source Location into a target Location. */
const consolidateLocation = async function (formData: FormData): Promise<void> {
  const sourceId = getRequiredString(formData, "sourceId");
  const targetLocationId = getRequiredString(formData, "targetLocationId");
  const client = await createApiClient();
  const response = await client.POST(
    "/locations/{sourceLocationId}/consolidate",
    {
      params: { path: { sourceLocationId: sourceId } },
      body: { targetLocationId },
    },
  );
  if (response.error) {
    throw new Error("Unable to consolidate Locations.");
  }
  revalidatePath("/locations");
};

export { renameLocation, deleteLocation, consolidateLocation };
