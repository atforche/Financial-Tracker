import "server-only";

import createClient, { type Client } from "openapi-fetch";
import getGoogleIdToken from "@/framework/data/getGoogleIdToken";
import type { paths } from "@/framework/data/api";

/**
 * Creates an API client for the current authenticated user.
 */
const createApiClient = async function (): Promise<Client<paths>> {
  const apiUrl = process.env["API_URL"];
  if (typeof apiUrl === "undefined" || apiUrl.trim() === "") {
    throw new Error("API_URL must be configured before using the API client.");
  }

  const idToken = await getGoogleIdToken();
  if (idToken === null || idToken === "") {
    throw new Error(
      "An authenticated Google session is required before using the API client.",
    );
  }

  return createClient<paths>({
    baseUrl: apiUrl,
    headers: { Authorization: `Bearer ${idToken}` },
  });
};

export default createApiClient;
